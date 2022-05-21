using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Timers;

namespace Crm.Link.RabbitMq.Common
{
    public class RabbitMqClientBase : IDisposable
    {
        //protected const string VirtualHost = "INTEGRATION_HOST";
        //protected readonly string LoggerExchange = $"{VirtualHost}.Exchange";
        //protected readonly string LoggerQueue = $"{VirtualHost}.message";
        //protected const string LoggerQueueAndExchangeRoutingKey = "message";

        protected IModel? Channel { get; private set; }
        private System.Timers.Timer? _timer;
        private IConnection? _connection;
        private readonly ConnectionProvider connectionProvider;
        private readonly ILogger<RabbitMqClientBase> _logger;

        protected RabbitMqClientBase(
            ConnectionProvider connectionProvider,
            ILogger<RabbitMqClientBase> logger)
        {
            this.connectionProvider = connectionProvider;
            _logger = logger;
            ConnectToRabbitMq();
        }

        private void ConnectToRabbitMq()
        {
            if (_connection == null || _connection.IsOpen == false)
            {                
                try
                {
                    _connection = connectionProvider.GetConnection();
                }
                catch (BrokerUnreachableException)
                {
                    _logger.LogError("RabbitMq not reachable: Timer Active, will retry in 10 sec");
                    SetTimer();
                }
            }

            if (_connection is not null && (Channel == null || Channel.IsOpen == false))
            {
                var channelMsg = Channel != null ? "Channel was not open" : "Channel was null";
                _logger.LogInformation($"Opening Channel because {channelMsg}");
                Console.WriteLine($"Opening Channel because {channelMsg}");
                Channel = _connection.CreateModel();
                
                //Channel.ExchangeDeclare(exchange: LoggerExchange, type: ExchangeType.Direct, durable: true, autoDelete: false);
                //Channel.QueueDeclare(queue: LoggerQueue, durable: false, exclusive: false, autoDelete: false);
                //Channel.QueueBind(queue: LoggerQueue, exchange: LoggerExchange, routingKey: LoggerQueueAndExchangeRoutingKey);
            }
        }

        private void SetTimer()
        {
            if (_timer == null)
            {
                _timer = new System.Timers.Timer(10000);

                _timer.Elapsed += OnTimedEvent;
                _timer.AutoReset = true;
                _timer.Enabled = true;
            }
        }

        private void OnTimedEvent(Object? source, ElapsedEventArgs e)
        {
             ConnectToRabbitMq();

            if (_connection is not null)
            {
                _logger.LogError("RabbitMq is reachable: Timer stop and disposed");
                _timer!.Stop();
                _timer.Dispose();
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

                connectionProvider?.Dispose();
                _timer?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Cannot dispose RabbitMQ channel or connection");
            }
        }
    }
}
