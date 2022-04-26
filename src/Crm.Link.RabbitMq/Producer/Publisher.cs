using Crm.Link.RabbitMq.Common;
using Microsoft.Extensions.Hosting;

namespace Crm.Link.RabbitMq.Producer
{
    public class Publisher
    {
        private readonly IRabbitMqProducer<IntegrationEvent> _producer;
        public Publisher(IRabbitMqProducer<IntegrationEvent> producer) => _producer = producer;
        protected async Task Publish(IntegrationEvent message, CancellationToken stoppingToken)
        {
            
                var @event = new IntegrationEvent
                {
                    Id = Guid.NewGuid(),
                    Message = $"Hello! Message generated at {DateTime.UtcNow.ToString("O")}"
                };

                _producer.Publish(@event);            

            await Task.CompletedTask;
        }
    }
}
