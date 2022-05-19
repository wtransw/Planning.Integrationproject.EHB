using CalendarServices;
using CalendarServices.Models;
using Crm.Link.RabbitMq.Common;
using Crm.Link.RabbitMq.Producer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Crm.Link.RabbitMq.Consumer
{
    public class PlanningAttendeeConsumer : ConsumerBase, IHostedService
    {
        protected override string QueueName => "PlanningAttendee";
        private readonly ILogger<PlanningAttendeeConsumer> attendeeLogger;
        private readonly IGoogleCalendarService CalendarService;
        public PlanningAttendeeConsumer(
            ConnectionProvider connectionProvider,
            ILogger<PlanningAttendeeConsumer> attendeeLogger,
            ILogger<ConsumerBase> consumerLogger,
            ILogger<RabbitMqClientBase> logger,
            IGoogleCalendarService calendarService
            ) :
            base(connectionProvider, consumerLogger, logger)
        {
            this.attendeeLogger = attendeeLogger;
            this.CalendarService = calendarService;
            TimerMethode += async () => await StartAsync(new CancellationToken(false));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (Channel is not null)
            {
                try
                {
                    var consumer = new AsyncEventingBasicConsumer(Channel);
                    consumer.Received += OnEventReceived<PlanningAttendee>;
                    Channel?.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
                }
                catch (Exception ex)
                {
                    attendeeLogger.LogCritical(ex, "Error while consuming message");
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
                attendeeLogger.LogInformation($"Base path: {basePath}");
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
                xRoot.ElementName = PlanningAttendee.XmlElementName;
                xRoot.IsNullable = true;

                var xmlSerializer = new XmlSerializer(typeof(PlanningAttendee), xRoot);
                attendeeLogger.LogInformation("deserializing planning attendee");
                var attendee = xmlSerializer.Deserialize(@event.Body.AsStream());


                if (attendee != null)
                    await HandleAttendee((PlanningAttendee)attendee);
            }
            catch (Exception ex)
            {
                attendeeLogger.LogCritical(ex, "Error while retrieving message from queue.");
            }
            finally
            {
                Channel!.BasicAck(@event.DeliveryTag, false);
            }
        }

        public async Task HandleAttendee(PlanningAttendee attendee)
        {
            attendeeLogger.LogInformation($"Handling planning attendee {attendee.Email}");


            //UuidMaster.GetAttendee(source: SourceEnum.PLANNING.ToString()


            //de attendee die je hier binnen krijgt heeft ook een versienummer, bijvoorbeeld 4.
            //je moet op de UUID master gaan zoeken naar die zelfde Attendee, en in de search meegeven dat je die van planning wilt (SourceType.Planning).

            //verbind met de UUID master API, /search met de parameters meegegeven (entitytype, enum sourcetype, ...)
            // EntityType is een string (waarschijnlijk "AttendeeEvent" zoals de base van de XML,
            // maar je zult kunnen zien wat er al bestaat met /get

            // Als versienummer van die op 3 staat, dan weet je dat we hem moeten updaten in google Calendar.

            //maak een google attendee
            // Haal eerst de onze op, en update hem dan. De EventGuid haal je uit de onze. 
            //var sessieVanOnzeAttendee = CalendarService.GetSession(null, "eventId van de onze");

            //Misschien nog beter: maak een methode GetAttendeeByEmail in GoogleCalendarService, en vraag hem zo op.
            // of je kan direct een UpdateAttendeeWithEmail maken die kijkt op email.

            //als ie niet bestaat doe je een create, anders een update.


            ;
        }

    }
}
