using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace CalendarServices.Models
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("XmlSchemaClassGenerator", "2.0.210.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute("SessionAttendeeEvent", Namespace = "", AnonymousType = true)]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute("SessionAttendeeEvent", Namespace = "")]
    public partial class PlanningSessionAttendee
    {
        public static string XmlElementName = "SessionAttendeeEvent";

        [System.ComponentModel.DataAnnotations.MinLengthAttribute(32)]
        [System.Xml.Serialization.XmlElementAttribute("UUID_nr", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string UUID_Nr { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("SourceEntityId", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SourceEntityId { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLengthAttribute(30)]
        [System.Xml.Serialization.XmlElementAttribute("EntityType", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string EntityType { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("EntityVersion", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string EntityVersion { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("Source", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public SourceEnum Source { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("Method", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public MethodEnum Method { get; set; }

        [System.ComponentModel.DataAnnotations.MinLengthAttribute(32)]
        [System.Xml.Serialization.XmlElementAttribute("AttendeeUUID", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AttendeeUUID { get; set; }

        [System.ComponentModel.DataAnnotations.MinLengthAttribute(32)]
        [System.Xml.Serialization.XmlElementAttribute("SessionUUID", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SessionUUID { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("InvitationStatus", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public NotificationStatus InvitationStatus { get; set; }
    }

    












    /*
    public MethodEnum Methode { get; set; }
    public string AccountUUID { get; set; } = "";
    public string SessionUUID { get; set; } = "";
    public NotificationStatus InvitationStatus { get; set; } = NotificationStatus.PENDING;

    public PlanningSessionAttendee()
    { }

    public PlanningSessionAttendee(MethodEnum methode, string accountUUID, string sessionUUID, NotificationStatus invitationStatus)
    {
        AccountUUID = accountUUID;
        SessionUUID = sessionUUID;
        Methode = methode;
        InvitationStatus = invitationStatus;
    }
    */

}
