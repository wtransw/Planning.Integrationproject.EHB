using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarServices.Models
{
    public enum MethodEnum
    {
        create = 0,
        update = 1,
        delete = 2
    }
    public enum NotificationStatus
    {
        pending = 0,
        accepted = 1,
        declined = 2
    }
}
