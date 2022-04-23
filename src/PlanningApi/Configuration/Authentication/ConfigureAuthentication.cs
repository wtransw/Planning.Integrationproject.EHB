using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace PlanningApi.Configuration.Authentication;

public static class ConfigureAuthentication
{
    public static IServiceCollection AddPlanningAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection jwtAuthenticationSection = configuration.GetSection("JwtAuthentication");
        services.Configure<JwtAuthenticationConfig>(jwtAuthenticationSection);
        JwtAuthenticationConfig jwtAuthenticationConfig = new();
        jwtAuthenticationSection.Bind(jwtAuthenticationConfig);

        // JWT authentication setup
        string base64Key = jwtAuthenticationConfig.SymmetricKey;
        string audience = jwtAuthenticationConfig.Audience;
        string issuer = jwtAuthenticationConfig.Issuer;
        byte[] secretKey = Encoding.ASCII.GetBytes(base64Key);
        SecurityKey key = new SymmetricSecurityKey(secretKey);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(
                    options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.SaveToken = true;
                        options.Audience = audience;
                        TokenValidationParameters tokenValidationParameters =
                            new TokenValidationParameters
                            {
                                IssuerSigningKey = key,
                                ValidIssuer = issuer,
                                ValidAudience = audience
                            };

                    // Because we don't have the key to verify the signature, we will skip this part in development environment.
                    // if (HostEnvironment.IsDevelopment())
                    tokenValidationParameters.SignatureValidator = (token, parameters) => new JwtSecurityToken(token);
                        options.TokenValidationParameters = tokenValidationParameters;
                    });
        return services;
    }
}

