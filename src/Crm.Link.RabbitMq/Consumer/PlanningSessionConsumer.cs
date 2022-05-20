using CalendarServices;
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

        private readonly IGoogleCalendarService CalendarService;
        private readonly IUUIDGateAway UuidMaster;
        private readonly IGoogleCalendarService GoogleCalendarService;

        public PlanningSessionConsumer(
            ConnectionProvider connectionProvider,
            ILogger<PlanningSessionConsumer> sessionLogger,
            ILogger<ConsumerBase> consumerLogger,
            ILogger<RabbitMqClientBase> logger,
            IGoogleCalendarService calendarService,
            IUUIDGateAway uuidMaster,
            IGoogleCalendarService googleCalendarService,
            ICalendarOptions calendarOptions
            ) :
            base(connectionProvider, consumerLogger, logger)
        {
            this.sessionLogger = sessionLogger;
            this.CalendarService = calendarService;
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
                    sessionLogger.LogCritical(ex, "Error while consuming message");
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
                //attendeeLogger.LogInformation($"Base path: {basePath}");
                sessionLogger.LogInformation($"Base path: {basePath}");
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

        private async Task UpdateAttendeeInGoogleCalendar(PlanningSession planningSession)
        {
            var sessionEvent = new Event()
            {
                // welke fields is hier nodig?
                Id = planningSession.SourceEntityId,
                Description = planningSession.Title,
                Organizer = planningSession.OrganiserUUID,
                Start = planningSession.StartDateUTC,
                End = planningSession.EndDateUTC
            };


            await GoogleCalendarService.UpdateSession(GoogleCalendarService.CalendarGuid, sessionEvent);
            //planningsession.Title of id
            await UuidMaster.UpdateEntity(planningSession.Title, SourceEnum.PLANNING.ToString(), UUID.Model.EntityTypeEnum.Session);
        }

        public async Task HandleSession(PlanningSession planningSession)
        {
            //attendeeLogger.LogInformation($"Handling planning attendee {attendee.Email}");
            var maxRetries = 5;
            sessionLogger.LogInformation($"Handling planning session {planningSession.Title}");

            //Kijken welke versie wij hebben van dit object.
            var uuidData = await UuidMaster.GetGuid(planningSession.Title, SourceEnum.PLANNING.ToString(), UUID.Model.EntityTypeEnum.Session);


            //enkel afhandelen als de versienummer hoger is dan wat al bestond. 
            if (uuidData != null && uuidData.EntityVersion < planningSession.EntityVersion)
            {
                try
                {
                    await UpdateAttendeeInGoogleCalendar(planningSession);
                }
                catch (Exception ex)
                {
                    sessionLogger.LogError($"Error while handling Session {planningSession.Title}: {ex.Message}", ex);
                }
            }


            // We krijgen een Attendee binnen die nog niet bestaat. We kunnen enkel een attendee toevoegen als we ook een sessie hebben waarin deze bestaat. 
            // We wachten tot er een sessie bestaat met deze attendee er in, en voegen hem dan toe.
            else if (uuidData == null)
            {
                //create attendee ALS er een sessionattendee voor dit object bestaat, eventueel met een retry over paar min? 

                for (int i = 0; i < maxRetries; i++)
                {
                    //Als we al een sessionAttendee gekregen hebben vanuit de queue, dan bestaat ie in google calendar, dus kunnen we ook gewoon updaten.
                    try
                    {
                        await Task.Delay(5 * 60 * 1000).ContinueWith(async t =>
                            uuidData = await UuidMaster.GetGuid(planningSession.Title, SourceEnum.PLANNING.ToString(), UUID.Model.EntityTypeEnum.Session));

                        if (uuidData != null && uuidData.EntityVersion < planningSession.EntityVersion)
                        {
                            await UpdateAttendeeInGoogleCalendar(planningSession);
                            i = maxRetries;
                        }
                        i++;
                    }
                    catch (Exception ex)
                    {
                        sessionLogger.LogError($"Error while handling Session {planningSession.Title}: {ex.Message}", ex);
                    }

                }


            }
        }

    }
}
