﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;


namespace CalendarServices.Models
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("XmlSchemaClassGenerator", "2.0.210.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute("AttendeeEvent", Namespace = "", AnonymousType = true)]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlRootAttribute("AttendeeEvent", Namespace = "")]
    public class PlanningAttendee
    {
        public static string XmlElementName = "AttendeeEvent";

        [System.ComponentModel.DataAnnotations.MinLengthAttribute(32)]
        [System.Xml.Serialization.XmlElementAttribute("UUID_Nr", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string UUID_Nr { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("SourceEntityId", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SourceEntityId { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("EntityVersion", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public int EntityVersion { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLengthAttribute(30)]
        [System.Xml.Serialization.XmlElementAttribute("EntityType", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string EntityType { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("Source", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public SourceEnum Source { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("Method", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public MethodEnum Method { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("Name", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.ComponentModel.DataAnnotations.MaxLength(30)]
        public string Name { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("LastName", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string LastName { get; set; }

        [System.ComponentModel.DataAnnotations.RegularExpressionAttribute("[^@]+@[^\\.]+\\..+")]
        [System.Xml.Serialization.XmlElementAttribute("Email", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Email { get; set; }
    }

}


