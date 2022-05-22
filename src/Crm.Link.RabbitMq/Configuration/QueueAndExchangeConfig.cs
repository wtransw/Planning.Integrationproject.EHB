namespace Crm.Link.RabbitMq.Configuration
{
    public static class QueueAndEchangeConfig
    {
        public static Dictionary<string, string[]> EchangeQueuList { get; set; } = new Dictionary<string, string[]>
        {
            /// Echange => binding to Queues 
            {"CrmAccount", new[] { "FrontAccount" } },
            {"CrmSession", new[] { "PlanningSession", "FrontSession" } },
            {"CrmAccountSession", new[] { "FrontAccountSession" } },
            {"CrmAttendee", new[] { "PlanningAttendee", "FrontAttendee" } },
            {"CrmAttendeeSession", new[] { "PlanningAttendeeSession", "FrontAttendeeSession" } },

            {"PlanningSession", new[] { "CrmSession", "FrontSession" } },
            {"PlanningAttendee", new[] { "CrmAttendee", "FrontAttendee" } },
            {"PlanningAttendeeSession", new[] { "CrmAttendeeSession", "FrontAttendeeSession" } },

            {"FrontAccount", new[] { "CrmAccount", "PlanningAccount" } },
            {"FrontSession", new[] { "CrmSession", "PlanningSession" } },
            {"FrontAttendee", new[] { "CrmAttendee", "PlanningAttendee" } },
            {"FrontAccountSession", new[] { "CrmAccountSession" } },
            {"FrontAttendeeSession", new[] { "CrmAttendeeSession", "PlanningAttendeeSession "} }
        };
    }
}
