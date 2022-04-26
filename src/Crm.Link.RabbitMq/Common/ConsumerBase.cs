using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Crm.Link.RabbitMq.Common
{
    public abstract class ConsumerBase : RabbitMqClientBase
    {
        private readonly ILogger<ConsumerBase> _logger;
        protected abstract string QueueName { get; }

        public ConsumerBase(
            IConnectionFactory connectionFactory,
            ILogger<ConsumerBase> consumerLogger,
            ILogger<RabbitMqClientBase> logger) :
            base(connectionFactory, logger)
        {

            _logger = consumerLogger;
        }

        protected virtual async Task OnEventReceived<T>(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                var body = Encoding.UTF8.GetString(@event.Body.ToArray());
                var message = JsonConvert.DeserializeObject<T>(body);

            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Error while retrieving message from queue.");
            }
            finally
            {
                Channel.BasicAck(@event.DeliveryTag, false);
            }
        }
    }
}
