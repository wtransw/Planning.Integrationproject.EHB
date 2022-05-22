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
    public class PlanningSessionAttendeeConsumer : ConsumerBase, IHostedService
    {
        protected override string QueueName => "PlanningAttendeeSession";
        private readonly ILogger<PlanningSessionAttendeeConsumer> sessionAttendeeLogger;
        private readonly IUUIDGateAway UuidMaster;
        private readonly IGoogleCalendarService GoogleCalendarService;
        private bool StartedAlready = false;

        public PlanningSessionAttendeeConsumer(
            ConnectionProvider connectionProvider,
            ILogger<PlanningSessionAttendeeConsumer> sessionAttendeeLogger,
            ILogger<ConsumerBase> consumerLogger,
            ILogger<RabbitMqClientBase> logger,
            IUUIDGateAway uuidMaster,
            IGoogleCalendarService googleCalendarService,
            ICalendarOptions calendarOptions
            ) :
            base(connectionProvider, consumerLogger, logger)
        {
            this.sessionAttendeeLogger = sessionAttendeeLogger;
            this.UuidMaster = uuidMaster;
            this.GoogleCalendarService = googleCalendarService;
            googleCalendarService.CreateCalendarService(calendarOptions);
            TimerMethode += async () => await StartAsync(new CancellationToken(false));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (Channel is not null)
            {
                if (!StartedAlready)
                {
                    attendeeLogger.LogInformation("Waiting for queues to be created.");
                    Thread.Sleep(TimeSpan.FromSeconds(90));
                    StartedAlready = true;
                }
                try
                {
                    var consumer = new AsyncEventingBasicConsumer(Channel);
                    consumer.Received += OnEventReceived<PlanningSessionAttendee>;
                    Channel?.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
                }
                catch (Exception ex)
                {
                    sessionAttendeeLogger.LogCritical(ex, "Error while consuming message");
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
                sessionAttendeeLogger.LogInformation($"Received Planning Session Attendee Event");
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
                xRoot.ElementName = PlanningSessionAttendee.XmlElementName;
                xRoot.IsNullable = true;

                var xmlSerializer = new XmlSerializer(typeof(PlanningSessionAttendee), xRoot);
                sessionAttendeeLogger.LogInformation("deserializing planning attendee");
                var planningSessionAttendee = xmlSerializer.Deserialize(@event.Body.AsStream());


                if (planningSessionAttendee != null)
                    await HandlePlanningSessionAttendee((PlanningSessionAttendee)planningSessionAttendee);
            }
            catch (Exception ex)
            {
                sessionAttendeeLogger.LogCritical(ex, "Error while retrieving message from queue.");
            }
            finally
            {
                Channel!.BasicAck(@event.DeliveryTag, false);
            }
        }


        public async Task HandlePlanningSessionAttendee(PlanningSessionAttendee planningSessionAttendee)
        {
            sessionAttendeeLogger.LogInformation($"Handling planning Session attendee {planningSessionAttendee.AttendeeUUID}");
            var maxRetries = 10;

            //haal de session op (met retries)
            //update de attendee voor die session, of create hem.
            for (int i = 0; i < maxRetries; i++)
            {

                var session = await GoogleCalendarService.GetSession(GoogleCalendarService.CalendarGuid, planningSessionAttendee.SessionUUID);

                if (session != null)
                {
                    //update 
                    try
                    {
                        //kijk of er in de attendees van de sessie al 1 staat met deze guid, en update hem
                        var attendee = session.Attendees.FirstOrDefault(a => a.Comment == planningSessionAttendee.AttendeeUUID);
                        if (attendee != null)
                        {
                            attendee.ResponseStatus = planningSessionAttendee.InvitationStatus.ToString();
                        }
                        else
                        {
                            //maak de attendee met default waardes. We vullen nog geen versienummer in in de UUID master. 
                            //Hierdoor wordt automatisch de rest ingevuld wanneer de attendee van de queue komt. 

                            var responseStatus = (planningSessionAttendee.InvitationStatus.ToString()) switch
                            {
                                "PENDING" => "needsAction",
                                "ACCEPTED" => "accepted",
                                "DECLINED" => "declined",
                                _ => "needsAction"
                            };
                            var nieuweAttendee = new EventAttendee()
                            {
                                Id = planningSessionAttendee.UUID_Nr,
                                Comment = planningSessionAttendee.UUID_Nr,
                                DisplayName = "new attendee",
                                Email = "default@email.val",
                                ResponseStatus = responseStatus,
                            };

                            session.Attendees.Add(nieuweAttendee);
                        }

                        await GoogleCalendarService.UpdateSession(GoogleCalendarService.CalendarGuid, session);
                        i = maxRetries;
                    }
                    catch (Exception ex)
                    {
                        sessionAttendeeLogger.LogError($"Error while handling Attendee {planningSessionAttendee.UUID_Nr}: {ex.Message}", ex);
                        i++;
                    }
                }

                // anders 2 minuten wachten, en opnieuw proberen. Eerst moet de sessie aangemaakt worden, en dan pas kunnen we een attendee linken. 
                else
                    await Task.Delay(2 * 60 * 1000);
            }


        }


        }
    }
