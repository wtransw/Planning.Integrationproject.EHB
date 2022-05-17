using CalendarServices.Models;
using Crm.Link.RabbitMq.Common;
using Crm.Link.RabbitMq.Producer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

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
    }
}
