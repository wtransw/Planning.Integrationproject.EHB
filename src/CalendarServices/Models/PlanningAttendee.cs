using System.Xml.Serialization;

namespace CalendarServices.Models
{
    [XmlRoot(ElementName = "AttendeeEvent")]
    public class PlanningAttendee
    {
        public int Version { get; set; }

        public string Name { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string VatNumber { get; set; }
        const string XmlElementName = "AttendeeEvent";
        public PlanningAttendee()
        {
        }
        public PlanningAttendee(int version, string name, string lastName, string email, string vatNumber)
        {
            this.Version = version;
            this.Name = name;
            this.LastName = lastName;
            this.Email = email;
            this.VatNumber = vatNumber;
        }
    }
}
