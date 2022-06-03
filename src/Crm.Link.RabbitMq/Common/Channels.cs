using Crm.Link.RabbitMq.Configuration;
using RabbitMQ.Client;

namespace Crm.Link.RabbitMq.Common
{
    public static class Channels
    {
        public static void Create(IModel model)
        {
            _ = model ?? throw new ArgumentNullException(nameof(model));

            foreach (var change in QueueAndEchangeConfig.EchangeQueuList)
            {
                if (change.Key != "None")
                    model.ExchangeDeclare(exchange: change.Key, type: ExchangeType.Direct, durable: true, autoDelete: false);

                foreach (var queue in change.Value)
                {
                    model.QueueDeclare(queue: queue, durable: false, exclusive: false, autoDelete: false);

                    if (change.Key != "None")
                        model.QueueBind(queue: queue, exchange: change.Key, routingKey: "");
                }
            }
        }
    }
}
