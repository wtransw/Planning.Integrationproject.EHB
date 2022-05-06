using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crm.Link.RabbitMq.Messages
{
    /// <summary>
    /// bind attendee to a session
    /// </summary>
    public class SessionAttendeeEvent
    {
        public MethodeEnum Method { get; set; }
        public string AccountUUId { get; set; }
        public string SessionUUId { get; set; }
        public InvitationStatusEnum InvitationStatus { get; set; }
    }
}
