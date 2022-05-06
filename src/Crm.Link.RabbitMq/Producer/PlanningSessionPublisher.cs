using Crm.Link.RabbitMq.Common;
using Microsoft.Extensions.Logging;
using CalendarServices.Models;

namespace Crm.Link.RabbitMq.Producer
{
    public class PlanningSessionPublisher : ProducerBase<PlanningSession>
    {
        public PlanningSessionPublisher(
            ConnectionProvider connectionProvider,
            ILogger<RabbitMqClientBase> logger,
            ILogger<ProducerBase<PlanningSession>> producerBaseLogger)
            : base(connectionProvider, logger, producerBaseLogger)
        {
        }

        protected override string ExchangeName => "INTEGRATION_HOST.Sessions";

        protected override string RoutingKeyName => "";

        protected override string AppId => "Planning";
    }
}