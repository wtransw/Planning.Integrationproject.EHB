        using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Xml.Serialization;
using System.Xml.Serialization;


namespace CalendarServices.Models
{

    [System.CodeDom.Compiler.GeneratedCodeAttribute("XmlSchemaClassGenerator", "2.0.210.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute("AttendeeEvent", Namespace = "", AnonymousType = true)]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute("AttendeeEvent", Namespace = "")]
    public partial class PlanningAttendee
    {
        public static string XmlElementName = "AttendeeEvent";

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
        public Source Source { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("Method", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public MethodEnum Method { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLengthAttribute(30)]
        [System.Xml.Serialization.XmlElementAttribute("Name", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Name { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLengthAttribute(50)]
        [System.Xml.Serialization.XmlElementAttribute("LastName", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string LastName { get; set; }

        [System.ComponentModel.DataAnnotations.RegularExpressionAttribute("[^@]+@[^\\.]+\\..+")]
        [System.Xml.Serialization.XmlElementAttribute("Email", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Email { get; set; }
    }

}






    //[XmlRoot(ElementName = "AttendeeEvent")]
    //public class PlanningAttendee
    //{
    //    public int Version { get; set; }

    //    public string Name { get; set; }

    //    public string LastName { get; set; }

    //    public string Email { get; set; }

    //    public string VatNumber { get; set; }
    //    public static string XmlElementName = "AttendeeEvent";
    //    public PlanningAttendee()
    //    {
    //    }
    //    public PlanningAttendee(int version, string name, string lastName, string email, string vatNumber)
    //    {
    //        this.Version = version;
    //        this.Name = name;
    //        this.LastName = lastName;
    //        this.Email = email;
    //        this.VatNumber = vatNumber;
    //    }
    //}

