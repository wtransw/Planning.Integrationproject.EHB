using CalendarServices;
using Crm.Link.RabbitMq.Configuration;
using Newtonsoft.Json.Converters;
using NLog;
using NLog.Web;
using PlanningApi.Configuration;
using PlanningApi.Configuration.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using CalendarServices.Models.Configuration;
using Crm.Link.UUID.Configuration;

Logger logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("Booting Planning API");

try
{
    // Boot order. Waiting for queues to be created.
    await Task.Delay(TimeSpan.FromSeconds(90));

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.AddNLog();
    //builder.Services.AddPlanningAuthentication(builder.Configuration);
    
    builder.Services.AddControllers()
                    .AddNewtonsoftJson(options => 
                    { 
                        options.SerializerSettings.Converters.Add(new StringEnumConverter()); 
                        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore; 
                    });
    
    builder.Services.AddOpenApi();
    builder.Services.UseUUID();

    // configuration rabbitmq
    builder.Services.StartConsumers(builder.Configuration.GetConnectionString("RabbitMq"));
    builder.Services.AddPublisher();
    var calendarSection = builder.Configuration.GetSection(CalendarOptions.SectionName);
    builder.Services.AddSingleton<ICalendarOptions>(provider =>
        new CalendarOptions()
        {
            CalendarGuid = calendarSection.GetValue<string>("CalendarGuid"),
            AccessToken = calendarSection.GetValue<string>("AccessToken"),
            AccessType = calendarSection.GetValue<string>("AccessType"),
            ClientId = calendarSection.GetValue<string>("ClientId"),
            ClientSecret = calendarSection.GetValue<string>("ClientSecret"),
            RedirectUri = calendarSection.GetValue<string>("RedirectUri"),
            RefreshToken = calendarSection.GetValue<string>("RefreshToken"),
            Scope = calendarSection.GetValue<string>("Scope"),
            TokenType = calendarSection.GetValue<string>("TokenType")
        });

    builder.Services.AddSingleton<IGoogleCalendarService>(provider => new GoogleCalendarService());
    //builder.Services.AddSingleton<IGoogleCalendarService>(provider => new GoogleCalendarService(provider.GetService<CalendarOptions>().CalendarGuid));    

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

    var app = builder.Build();
    logger.Info("Planning API started");

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
