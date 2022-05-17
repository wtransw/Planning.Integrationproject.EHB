using CalendarServices.Models;
using Crm.Link.RabbitMq.Common;
using Microsoft.Extensions.Logging;

namespace Crm.Link.RabbitMq.Producer
{
    public class PlanningSessionAttendeePublisher : ProducerBase<PlanningSessionAttendee>
    {
        public PlanningSessionAttendeePublisher(
            ConnectionProvider connectionProvider,
            ILogger<RabbitMqClientBase> logger,
            ILogger<ProducerBase<PlanningSessionAttendee>> producerBaseLogger)
            : base(connectionProvider, logger, producerBaseLogger)
        {
        }

        protected override string ExchangeName => "PlanningSessionAttendee";

        protected override string RoutingKeyName => "";

        protected override string AppId => "Planning";
    }
}
