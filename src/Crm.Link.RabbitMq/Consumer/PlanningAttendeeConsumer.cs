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
        protected override string QueueName => "Attendees";
        private readonly ILogger<PlanningAttendeeConsumer> attendeeLogger;

        public PlanningAttendeeConsumer(
            ConnectionProvider connectionProvider,
            ILogger<PlanningAttendeeConsumer> attendeeLogger,
            ILogger<ConsumerBase> consumerLogger,
            ILogger<RabbitMqClientBase> logger) :
            base(connectionProvider, consumerLogger, logger)
        {
            this.attendeeLogger = attendeeLogger;
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
                Console.WriteLine(basePath);

                XmlReader reader = new XmlTextReader(@event.Body.AsStream());
                XmlDocument document = new();
                document.Load(reader);

                // xsd for validation
                XmlSchemaSet xmlSchemaSet = new();
                xmlSchemaSet.Add("", $"{basePath}/Resources/AttendeeEvent.xsd");
                xmlSchemaSet.Add("", $"{basePath}/Resources/SessionEvent.xsd");
                xmlSchemaSet.Add("", $"{basePath}/Resources/SessionAttendeeEvent.xsd");
                xmlSchemaSet.Add("", $"{basePath}/Resources/UUID.xsd");

                document.Schemas.Add(xmlSchemaSet);
                ValidationEventHandler eventHandler = new(ValidationEventHandler);

                document.Validate(eventHandler);

                XmlRootAttribute xRoot = new XmlRootAttribute();
                xRoot.ElementName = PlanningAttendee.XmlElementName;
                xRoot.IsNullable = true;

                var xmlSerializer = new XmlSerializer(typeof(PlanningAttendee), xRoot);
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

        public async Task HandleAttendee(PlanningAttendee? attendee)
        {
            //doe iets
            ;
        }

    }
}
