using System;
using System.Collections.Generic;
using System.Text;

namespace CalendarServices.Models.Configuration
{
    public interface ICalendarOptions
    {
        public static string SectionName { get { return "Calendar"; } }
        public string CalendarGuid { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AccessToken { get; set; }
        public string RedirectUri { get; set; }
        public string RefreshToken { get; set; }
        public string AccessType { get; set; }
        public string TokenType { get; set; }
        public string Scope { get; set; }
    }
}
