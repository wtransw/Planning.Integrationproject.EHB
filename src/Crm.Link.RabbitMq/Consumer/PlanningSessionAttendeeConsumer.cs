﻿using CalendarServices.Models;
using Crm.Link.RabbitMq.Common;
using Crm.Link.RabbitMq.Producer;
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

        public PlanningSessionAttendeeConsumer(
            ConnectionProvider connectionProvider,
            ILogger<PlanningSessionAttendeeConsumer> sessionAttendeeLogger,
            ILogger<ConsumerBase> consumerLogger,
            ILogger<RabbitMqClientBase> logger) :
            base(connectionProvider, consumerLogger, logger)
        {
            this.sessionAttendeeLogger = sessionAttendeeLogger;
            TimerMethode += async () => await StartAsync(new CancellationToken(false));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (Channel is not null)
            {
                try
                {
                    var consumer = new AsyncEventingBasicConsumer(Channel);
                    consumer.Received += OnEventReceived<PlanningSessionAttendee>;
                    Channel?.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
                }
                catch (Exception ex)
                {
                    sessionAttendeeLogger.LogCritical(ex, "Error while consuming message");
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
                sessionAttendeeLogger.LogInformation($"Base path: {basePath}");
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
           //sessionAttendeeLogger.LogInformation($"Handling planning attendee {attendee.Email}");
        }


        }
    }
