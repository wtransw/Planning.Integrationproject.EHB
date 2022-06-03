using Crm.Link.RabbitMq.Common;
using Microsoft.Extensions.Logging;

namespace Crm.Link.RabbitMq.Producer
{
    public class SessionPublisher : ProducerBase<SessionEvent>
    {
        public SessionPublisher(
            ConnectionProvider connectionProvider,
            ILogger<RabbitMqClientBase> logger,
            ILogger<ProducerBase<SessionEvent>> producerBaseLogger)
            : base(connectionProvider, logger, producerBaseLogger)
        {
        }

        protected override string ExchangeName => "PlanningSession";

        protected override string RoutingKeyName => "";

        protected override string AppId => "Planning";
    }
}
