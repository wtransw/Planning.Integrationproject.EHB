using Crm.Link.RabbitMq.Common;
using Microsoft.Extensions.Logging;
using CalendarServices.Models;

namespace Crm.Link.RabbitMq.Producer
{
    public class PlanningAttendeePublisher : ProducerBase<PlanningAttendee>
    {
        public PlanningAttendeePublisher(
            ConnectionProvider connectionProvider,
            ILogger<RabbitMqClientBase> logger,
            ILogger<ProducerBase<PlanningAttendee>> producerBaseLogger)
            : base(connectionProvider, logger, producerBaseLogger)
        {
        }

        protected override string ExchangeName => "INTEGRATION_HOST.Attendees";

        protected override string RoutingKeyName => "";

        protected override string AppId => "Planning";
    }
}