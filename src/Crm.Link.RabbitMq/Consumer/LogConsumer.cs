using Crm.Link.RabbitMq.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Crm.Link.RabbitMq.Consumer
{
    public class LogConsumer : ConsumerBase, IHostedService
    {
        protected override string QueueName => "CUSTOM_HOST.log.message";

        public LogConsumer(
            ConnectionProvider connectionProvider,
            ILogger<LogConsumer> logConsumerLogger,
            ILogger<ConsumerBase> consumerLogger,
            ILogger<RabbitMqClientBase> logger) :
            base(connectionProvider, consumerLogger, logger)
        {
            try
            {
                var consumer = new AsyncEventingBasicConsumer(Channel);
                consumer.Received += OnEventReceived<LogCommand>;
                Channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
            }
            catch (Exception ex)
            {
                logConsumerLogger.LogCritical(ex, "Error while consuming message");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }
    }
}
