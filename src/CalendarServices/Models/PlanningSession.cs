using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CalendarServices.Models
{
    [XmlRoot(ElementName = "SessionEvent")]
    public class PlanningSession
    {
        public int Version { get; set; }
        public string Title { get; set; }
        public DateTime StartDateUTC { get; set; }
        public DateTime EndDateUTC { get; set; }
        public string Description { get; set; }
        public string OrganiserUUID { get; set; }
        public string IsActive { get; set; }
        
        public PlanningSession()
        { }
        
        public PlanningSession(string title, DateTime startDateUtc, DateTime endDateUtc, string description, string organiserUuid, int version = 0,string isactive = "1")
        {
            this.Version = version;
            this.Title = title;
            this.StartDateUTC = startDateUtc;
            this.EndDateUTC = endDateUtc;
            this.Description = description;
            this.OrganiserUUID = organiserUuid;
            this.IsActive = isactive;
            
        }
    }
}
