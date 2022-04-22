using CalendarServices;
using Newtonsoft.Json.Converters;
using NLog;
using NLog.Web;
using PlanningApi.Configuration;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("Booting Planning API");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.AddNLog();
    builder.Services.AddControllers()
                    .AddNewtonsoftJson(options => options.SerializerSettings.Converters.Add(new StringEnumConverter()));
    builder.Services.AddOpenApi();
    //builder.Services.Configure<CalendarOptions>(builder.Configuration.GetSection(CalendarOptions.SectionName));
    builder.Services.AddSingleton<CalendarOptions>(provider =>
        new CalendarOptions()
        {
            CalendarGuid = builder.Configuration.GetSection(CalendarOptions.SectionName).GetValue<string>("CalendarGuid"),
        });

    builder.Services.AddSingleton<IGoogleCalendarService>(provider => new GoogleCalendarService());
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

    

    

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    //app.UseAuthorization();
    app.UseAuthentication();

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
