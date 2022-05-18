using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace CalendarServices.Models
{

    [System.CodeDom.Compiler.GeneratedCodeAttribute("XmlSchemaClassGenerator", "2.0.210.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute("SessionEvent", Namespace = "", AnonymousType = true)]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute("SessionEvent", Namespace = "")]
    public partial class PlanningSession
    {
        public static string XmlElementName = "SessionEvent";

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

        [System.ComponentModel.DataAnnotations.MaxLengthAttribute(30)]
        [System.Xml.Serialization.XmlElementAttribute("Title", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Title { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("StartDateUTC", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "dateTime")]
        public System.DateTime StartDateUTC { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("EndDateUTC", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "dateTime")]
        public System.DateTime EndDateUTC { get; set; }

        [System.ComponentModel.DataAnnotations.MinLengthAttribute(32)]
        [System.Xml.Serialization.XmlElementAttribute("OrganiserUUID", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string OrganiserUUID { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("IsActive", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsActive { get; set; }
    }

    







    /*
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
    */
}
}
