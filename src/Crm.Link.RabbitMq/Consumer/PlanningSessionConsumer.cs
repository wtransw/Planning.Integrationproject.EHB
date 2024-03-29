﻿using CalendarServices;
using CalendarServices.Models;
using CalendarServices.Models.Configuration;
using Crm.Link.RabbitMq.Common;
using Crm.Link.RabbitMq.Producer;
using Crm.Link.UUID;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Crm.Link.RabbitMq.Consumer
{
    public class PlanningSessionConsumer : ConsumerBase, IHostedService
    {
        protected override string QueueName => "PlanningSession";
        private readonly ILogger<PlanningSessionConsumer> sessionLogger;

        private readonly IUUIDGateAway UuidMaster;
        private readonly IGoogleCalendarService GoogleCalendarService;

        public PlanningSessionConsumer(
            ConnectionProvider connectionProvider,
            ILogger<PlanningSessionConsumer> sessionLogger,
            ILogger<ConsumerBase> consumerLogger,
            ILogger<RabbitMqClientBase> logger,
            IUUIDGateAway uuidMaster,
            IGoogleCalendarService googleCalendarService,
            ICalendarOptions calendarOptions
            ) :
            base(connectionProvider, consumerLogger, logger)
        {
            this.sessionLogger = sessionLogger;
            this.UuidMaster = uuidMaster;
            this.GoogleCalendarService = googleCalendarService;
            googleCalendarService.CreateCalendarService(calendarOptions);
            TimerMethode += async () => await StartAsync(new CancellationToken(false));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (Channel is not null)
            {
                try
                {
                    var consumer = new AsyncEventingBasicConsumer(Channel);
                    consumer.Received += OnEventReceived<PlanningSession>;
                    Channel?.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
                }
                catch (Exception ex)
                {
                    sessionLogger.LogCritical(ex, "Error while binding to queue.");
                    SetTimer();
                    ConnectToRabbitMq();
                }
            }
            else
            {
                SetTimer();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public override async Task OnEventReceived<T>(object sender, BasicDeliverEventArgs @event)
        {
            var basePath = System.AppDomain.CurrentDomain.BaseDirectory;
            try
            {
                sessionLogger.LogInformation($"Received Planning Attendee Event");
                XmlReader reader = new XmlTextReader(@event.Body.AsStream());
                XmlDocument document = new();
                document.Load(reader);

                // xsd for validation
                XmlSchemaSet xmlSchemaSet = new();
                xmlSchemaSet.Add("", $"{basePath}/Resources/AttendeeEvent_w.xsd");
                xmlSchemaSet.Add("", $"{basePath}/Resources/SessionEvent_w.xsd");
                xmlSchemaSet.Add("", $"{basePath}/Resources/SessionAttendeeEvent_w.xsd");

                document.Schemas.Add(xmlSchemaSet);
                ValidationEventHandler eventHandler = new(ValidationEventHandler);

                document.Validate(eventHandler);

                XmlRootAttribute xRoot = new XmlRootAttribute();
                // xRoot.ElementName = PlanningAttendee.XmlElementName;

                xRoot.ElementName = PlanningSession.XmlElementName;
                xRoot.IsNullable = true;

                var xmlSerializer = new XmlSerializer(typeof(PlanningSession), xRoot);
                sessionLogger.LogInformation("deserializing planning attendee");
                var planningSession = xmlSerializer.Deserialize(@event.Body.AsStream());

                if (planningSession != null)
                    await HandleSession((PlanningSession)planningSession);
            }
            catch (Exception ex)
            {
                // attendeeLogger.LogCritical(ex, "Error while retrieving message from queue.");
                sessionLogger.LogCritical(ex, "Error while retrieving message from queue.");
            }
            finally
            {
                Channel!.BasicAck(@event.DeliveryTag, false);
            }
        }

        private async Task UpdateSessionInGoogleCalendar(PlanningSession planningSession)
        {
            var sessionEvent = new Event()
            {
                Description = planningSession.Title,
                Start = new EventDateTime()
                {
                    //Date = planningSession.StartDateUTC.ToString("yyyy-mm-dd"),
                    DateTime = planningSession.StartDateUTC,
                    TimeZone = "Europe/Zurich"
                },
                End = new EventDateTime()
                {
                    //Date = planningSession.EndDateUTC.ToString("yyyy-mm-dd"),
                    DateTime = planningSession.EndDateUTC,
                    TimeZone = "Europe/Zurich"
                },
                Summary = planningSession.UUID_Nr,
                Location = "Koln",
                //Attendees = new List<EventAttendee>()
                Attendees = new EventAttendee[] {
                                new EventAttendee
                                {
                                    Email = "dummy@default.com",
                                    DisplayName = "Organizer",
                                    ResponseStatus = "accepted",
                                    Organizer = true                //bij ons de spreker
                                }
                            }
            };


            await GoogleCalendarService.UpdateSession(GoogleCalendarService.CalendarGuid, sessionEvent);
            //planningsession.Title of id
            await UuidMaster.UpdateEntity(planningSession.Title, SourceEnum.PLANNING.ToString(), UUID.Model.EntityTypeEnum.Session);
        }

        public async Task HandleSession(PlanningSession planningSession)
        {
            var maxRetries = 5;
            sessionLogger.LogInformation($"Handling planning session {planningSession.Title}");

            //Kijken welke versie wij hebben van dit object.
            var uuidData = await UuidMaster.GetGuid(planningSession.Title, SourceEnum.PLANNING.ToString(), UUID.Model.EntityTypeEnum.Session);

            //enkel afhandelen als de versienummer hoger is dan wat al bestond. 
            if (uuidData != null && uuidData.EntityVersion < planningSession.EntityVersion)
            {
                try
                {
                    await UpdateSessionInGoogleCalendar(planningSession);
                }
                catch (Exception ex)
                {
                    sessionLogger.LogError($"Error while handling Session {planningSession.Title}: {ex.Message}", ex);
                    SetTimer();
                }
            }


            // We krijgen een Session binnen die nog niet bestaat. Create dus.
            else if (uuidData == null)
            {
                sessionLogger.LogInformation($"Creating session with title {planningSession.Title} with UUID {planningSession.UUID_Nr}.");
                sessionLogger.LogInformation($"From {planningSession.StartDateUTC} to {planningSession.EndDateUTC}.");
                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        var newSession = new Event()
                        {
                            Description = planningSession.UUID_Nr,
                            Start = new EventDateTime()
                            {
                                //Date = planningSession.StartDateUTC.ToString("yyyy-mm-dd"),
                                DateTime = planningSession.StartDateUTC,
                                TimeZone = "Europe/Zurich"
                            },
                            End = new EventDateTime()
                            {
                                //Date = planningSession.EndDateUTC.ToString("yyyy-mm-dd"),
                                DateTime = planningSession.EndDateUTC,
                                TimeZone = "Europe/Zurich"
                            },
                            Summary = planningSession.Title,
                            //Location = "Koln",
                            Location = "Brussel",
                            //Attendees = new List<EventAttendee>()
                            Attendees = new EventAttendee[] {
                                new EventAttendee
                                {
                                    Email = "dummy@default.com",
                                    DisplayName = "Organizer",
                                    ResponseStatus = "accepted",
                                    Organizer = true                //bij ons de spreker
                                }
                            }
                        };

                        var createdSession = await GoogleCalendarService.CreateSessionForEvent(GoogleCalendarService.CalendarGuid, planningSession.Title, newSession);
                        if (createdSession != null)
                            sessionLogger.LogInformation("New session successfully created.");
                        else
                            sessionLogger.LogInformation("Failed to create new session.");

                        var guidOk = Guid.TryParse(createdSession.Id, out var parsedGuid);
                        var guidUuidOk = Guid.TryParse(createdSession.Id, out var parsedGuidUuid);
                        var guidToUse = guidOk ? parsedGuid : guidUuidOk ? parsedGuidUuid : Guid.NewGuid();

                        //await UuidMaster.PublishEntity(SourceEnum.PLANNING.ToString(), UUID.Model.EntityTypeEnum.Session, createdSession.Id ?? planningSession.UUID_Nr, planningSession.EntityVersion);
                        await UuidMaster.PublishEntity(guidToUse, SourceEnum.PLANNING.ToString(), UUID.Model.EntityTypeEnum.Session, createdSession.Id ?? planningSession.UUID_Nr, 1);


                        i = maxRetries;
                    }
                    catch (Exception ex)
                    {
                        sessionLogger.LogError($"Error while handling Session {planningSession.Title}: {ex.Message}. Retry in 2 minutes ({i}/{maxRetries}).", ex);
                        await Task.Delay(2 * 60 * 1000);
                        i++;
                    }

                }


            }
        }

    }
}
