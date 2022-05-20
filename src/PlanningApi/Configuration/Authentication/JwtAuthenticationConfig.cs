namespace PlanningApi.Configuration.Authentication;

public interface IJwtAuthenticationConfig
{
    string Issuer { get; }

    string Audience { get; }

    string SymmetricKey { get; }

    string CertificateThumbPrint { get; }

    TimeSpan TokenLifetime { get; }
}

public class JwtAuthenticationConfig : IJwtAuthenticationConfig
{
    public string Issuer { get; set; } = "";

    public string Audience { get; set; } = "";

    public string SymmetricKey { get; set; } = "";

    public string CertificateThumbPrint { get; set; } = "";

    public TimeSpan TokenLifetime { get; set; } = new TimeSpan(hours:24, 0, 0);
}
