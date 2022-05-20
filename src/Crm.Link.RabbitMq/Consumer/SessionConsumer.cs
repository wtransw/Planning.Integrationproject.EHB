using Crm.Link.RabbitMq.Common;
using Crm.Link.RabbitMq.Producer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Crm.Link.RabbitMq.Consumer
{
    public class SessionConsumer : ConsumerBase, IHostedService
    {
        private readonly ILogger<SessionConsumer> sessionLogger;

        protected override string QueueName => "Session";

        public SessionConsumer(
            ConnectionProvider connectionProvider,
            ILogger<SessionConsumer> sessionLogger,
            ILogger<ConsumerBase> consumerLogger,
            ILogger<RabbitMqClientBase> logger) :
            base(connectionProvider, consumerLogger, logger)
        {
            this.sessionLogger = sessionLogger;
            TimerMethode += async () => await StartAsync(new CancellationToken(false)); 
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (Channel is not null)
            {
                    try
                {
                    var consumer = new AsyncEventingBasicConsumer(Channel);
                    consumer.Received += OnEventReceived<SessionEvent>;
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
            Channel?.Dispose();

            return Task.CompletedTask;
        }
    }
}
