using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CalendarServices.Models
{
    [XmlRoot(ElementName = "SessionAttendeeEvent")]
    public class PlanningSessionAttendee
    {
        public MethodEnum Methode { get; set; }
        public string AccountUUID { get; set; } = "";
        public string SessionUUID { get; set; } = "";
        public NotificationStatus InvitationStatus { get; set; } = NotificationStatus.pending;

        public PlanningSessionAttendee()
        { }

        public PlanningSessionAttendee(MethodEnum methode, string accountUUID, string sessionUUID, NotificationStatus invitationStatus)
        {
            AccountUUID = accountUUID;
            SessionUUID = sessionUUID;
            Methode = methode;
            InvitationStatus = invitationStatus;
        }
        
    }
}
