using CalendarServices;
using Crm.Link.RabbitMq.Configuration;
using Newtonsoft.Json.Converters;
using NLog;
using NLog.Web;
using PlanningApi.Configuration;
using PlanningApi.Configuration.Authentication;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

Logger logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("Booting Planning API");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.AddNLog();
    builder.Services.AddPlanningAuthentication(builder.Configuration);
    
    builder.Services.AddControllers()
                    .AddNewtonsoftJson(options => 
                    { 
                        options.SerializerSettings.Converters.Add(new StringEnumConverter()); 
                        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore; 
                    });
    
    builder.Services.AddOpenApi();

    // configuration rabbitmq
    builder.Services.StartConsumers(builder.Configuration.GetConnectionString("RabbitMq"));
    builder.Services.AddPublisher();

    builder.Services.AddSingleton<CalendarOptions>(provider =>
        new CalendarOptions()
        {
            CalendarGuid = builder.Configuration.GetSection(CalendarOptions.SectionName).GetValue<string>("CalendarGuid"),
        });

    builder.Services.AddSingleton<NLog.ILogger>(provider => NLog.LogManager.GetLogger("logger"));

    builder.Services.AddSingleton<IGoogleCalendarService>(provider => new GoogleCalendarService());
    builder.Services.AddSingleton<IPlanningService, PlanningService>();
    //builder.Services.AddSingleton<IGoogleCalendarService>(provider => new GoogleCalendarService(provider.GetService<CalendarOptions>().CalendarGuid));    

    logger.Info("Planning API started");
    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll",
            builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
    });

    //builder.Services.AddAuthentication(options =>
    //                            {
    //                                // This forces challenge results to be handled by Google OpenID Handler, so there's no
    //                                // need to add an AccountController that emits challenges for Login.
    //                                options.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
    //                                // This forces forbid results to be handled by Google OpenID Handler, which checks if
    //                                // extra scopes are required and does automatic incremental auth.
    //                                options.DefaultForbidScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
    //                                // Default scheme that will handle everything else.
    //                                // Once a user is authenticated, the OAuth2 token info is stored in cookies.
    //                                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //                            })
    //                .AddCookie()
    //                .AddGoogleOpenIdConnect(options =>
    //                {
    //                    options.ClientId = { YOUR_CLIENT_ID};
    //                    options.ClientSecret = { YOUR_CLIENT_SECRET};
    //                });


        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
       .AddCookie()
       .AddOpenIdConnect(options =>
       {
           /*
            *     "client_id": "281496544249-7l0127vpa5kuetv6r4a10b13g5hd8jia.apps.googleusercontent.com",
                    "project_id": "integrationprojplanningwt2022",
                    "auth_uri": "https://accounts.google.com/o/oauth2/auth",
                    "token_uri": "https://oauth2.googleapis.com/token",
                    "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
                    "client_secret": "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",
                    "redirect_uris": [ "urn:ietf:wg:oauth:2.0:oob", "http://localhost" ]
            * 
            */
           options.SignInScheme = "Cookies";
           options.Authority = "https://www.googleapis.com/oauth2/v1/certs";
           options.RequireHttpsMetadata = true;
           options.ClientId = "281496544249-7l0127vpa5kuetv6r4a10b13g5hd8jia.apps.googleusercontent.com";
           options.ClientSecret = "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";
           options.ResponseType = "code";
           options.UsePkce = true;
           //options.Scope.Add("profile");
           options.Scope.Add("https://www.googleapis.com/auth/calendar");
           options.Scope.Add("https://www.googleapis.com/auth/calendar.events");
           options.Scope.Add("https://www.googleapis.com/auth/calendar.settings.readonly");
           options.SaveTokens = true;
       });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment() || true)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();
    //app.UseAuthentication();
    
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, $"Unhandled exception: {ex.Message}");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}
