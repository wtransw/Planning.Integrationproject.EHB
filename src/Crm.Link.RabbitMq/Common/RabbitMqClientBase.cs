using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Crm.Link.RabbitMq.Common
{
    public class RabbitMqClientBase : IDisposable
    {
        protected const string VirtualHost = "INTEGRATION_HOST";
        protected readonly string LoggerExchange = $"{VirtualHost}.Exchange";
        protected readonly string LoggerQueue = $"{VirtualHost}.message";
        protected const string LoggerQueueAndExchangeRoutingKey = "message";

        protected IModel Channel { get; private set; }
        private IConnection _connection;
        private readonly IConnectionFactory connectionFactory;
        private readonly ILogger<RabbitMqClientBase> _logger;

        protected RabbitMqClientBase(
            IConnectionFactory connectionFactory,
            ILogger<RabbitMqClientBase> logger)
        {
            this.connectionFactory = connectionFactory;
            _logger = logger;
            ConnectToRabbitMq();
        }

        private void ConnectToRabbitMq()
        {
            if (_connection == null || _connection.IsOpen == false)
            {
                do
                {
                    try
                    {
                        _connection = connectionFactory.CreateConnection();
                    }
                    catch (BrokerUnreachableException bex)
                    {
                        _logger.LogError(bex, "RabbitMq not reachable: ");                    
                    }

                    Thread.Sleep(1000);

                } while (_connection is null);
            }

            if (Channel == null || Channel.IsOpen == false)
            {
                Channel = _connection.CreateModel();
                Channel.ExchangeDeclare(exchange: LoggerExchange, type: ExchangeType.Direct, durable: true, autoDelete: false);
                Channel.QueueDeclare(queue: LoggerQueue, durable: false, exclusive: false, autoDelete: false);
                Channel.QueueBind(queue: LoggerQueue, exchange: LoggerExchange, routingKey: LoggerQueueAndExchangeRoutingKey);
            }
        }
        public void Dispose()
        {
            try
            {
                Channel?.Close();
                Channel?.Dispose();
                Channel = null;

                _connection?.Close();
                _connection?.Dispose();
                _connection = null;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Cannot dispose RabbitMQ channel or connection");
            }
        }
    }
}
