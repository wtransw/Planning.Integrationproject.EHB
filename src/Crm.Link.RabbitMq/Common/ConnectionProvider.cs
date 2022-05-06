using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Crm.Link.RabbitMq.Common
{
    public class ConnectionProvider : IDisposable
    {
        private readonly IConnectionFactory connectionFactory;
        private readonly ILogger<ConnectionProvider> logger;
        private IConnection? connection;

        public ConnectionProvider(IConnectionFactory connectionFactory, ILogger<ConnectionProvider> logger)
        {
            this.connectionFactory = connectionFactory;
            this.logger = logger;
        }

        public void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
            }
        }

        public IConnection? GetConnection()
        {
            if (connection == null || !connection!.IsOpen)
                return connection = OpenConnection();

            return connection;
        }

        private IConnection? OpenConnection()
        {
            try
            {
                return connectionFactory.CreateConnection();
            }
            catch (BrokerUnreachableException)
            {                
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Tried to connect to rabbitMq");
            }

            return null;
        }
    }
}
