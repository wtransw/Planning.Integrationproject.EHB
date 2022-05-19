using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarServices.Models
{
    public enum NotificationStatus
    {
        PENDING = 0,
        ACCEPTED = 1,
        DECLINED = 2
    }


    [System.CodeDom.Compiler.GeneratedCodeAttribute("XmlSchemaClassGenerator", "2.0.210.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute("AttendeeEventSource", Namespace = "")]
    public enum SourceEnum
    {

        FRONTEND,

        CRM,

        PLANNING,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("XmlSchemaClassGenerator", "2.0.210.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute("AttendeeEventMethod", Namespace = "")]
    public enum MethodEnum
    {

        CREATE,

        UPDATE,

        DELETE,
    }


}
