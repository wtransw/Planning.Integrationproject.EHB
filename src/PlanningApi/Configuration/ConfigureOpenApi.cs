using Microsoft.OpenApi.Models;

namespace PlanningApi.Configuration;

public static class ConfigureOpenApi
{
    public static IServiceCollection AddOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Planning API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new()
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });
            
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { 
                    new OpenApiSecurityScheme
                    { 
                        Reference = new OpenApiReference { Id = "jwt_auth", Type = ReferenceType.SecurityScheme } 
                    }, 
                    new string[] { } 
                }
            });
        });

        return services;
    }


    
}
