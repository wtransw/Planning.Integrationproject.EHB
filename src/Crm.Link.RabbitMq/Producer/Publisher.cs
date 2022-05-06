using Crm.Link.RabbitMq.Common;

namespace Crm.Link.RabbitMq.Producer
{
    public class Publisher<T> // hummm moet nog afgewerkt worden
    {
        private readonly IRabbitMqProducer<T> _producer;
        public Publisher(IRabbitMqProducer<T> producer) => _producer = producer;
        protected async Task Publish(T message, CancellationToken stoppingToken)
        {

            _producer.Publish(message);            

            await Task.CompletedTask;
        }
    }
}
