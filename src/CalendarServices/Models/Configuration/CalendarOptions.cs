namespace CalendarServices.Models.Configuration
{
    public class CalendarOptions : ICalendarOptions
    {
        public static string SectionName = "Calendar";
        public string CalendarGuid { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = "urn:ietf:wg:oauth:2.0:oob";
        public string RefreshToken { get; set; } = string.Empty;
        public string AccessType { get; set; } = "offline";
        public string TokenType { get; set; } = "refresh";
        public string Scope { get; set; } = "https://www.googleapis.com/auth/calendar.events https://www.googleapis.com/auth/calendar https://www.googleapis.com/auth/calendar.settings.readonly";

    }
}