using Crm.Link.RabbitMq.Common;
using Microsoft.Extensions.Logging;

namespace Crm.Link.RabbitMq.Producer
{
    public class AccountPublisher : ProducerBase<AttendeeEvent>
    {
        public AccountPublisher(
            ConnectionProvider connectionProvider,
            ILogger<RabbitMqClientBase> logger,
            ILogger<ProducerBase<AttendeeEvent>> producerBaseLogger)
            : base(connectionProvider, logger, producerBaseLogger)
        {
        }

        protected override string ExchangeName => "INTEGRATION_HOST.Accounts";

        protected override string RoutingKeyName => "";

        protected override string AppId => "Crm";
    }
}
