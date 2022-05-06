using Crm.Link.RabbitMq.Messages;

namespace Crm.Link.RabbitMq.Producer
{
    public class SessionEvent
    {
        /// <summary>
        /// UUID from UUIDMaster
        /// </summary>
        public string UUId { get; set; }
        /// <summary>
        /// Create, Update, Delete
        /// </summary>
        public MethodeEnum Methode { get; set; }
        public int Version { get; set; }
        public string Title { get; set; }

        /// <summary>
        /// UTC !!!!!!!!!
        /// </summary>
        public DateTime StartDateUTC { get; set; } // in toekomst
        public DateTime EndDateUTC { get; set; } // niet voor startdate in toekomst
        public string Description { get; set; }
        public string? OrganiserUUId { get; set; }
        public bool IsActive { get; set; }

    }
}
