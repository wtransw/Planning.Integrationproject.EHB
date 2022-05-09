﻿using CalendarServices;
using Crm.Link.RabbitMq.Configuration;
using Newtonsoft.Json.Converters;
using NLog;
using NLog.Web;
using PlanningApi.Configuration;
using PlanningApi.Configuration.Authentication;
using Microsoft.AspNetCore.Authentication.Google;

Logger logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("Booting Planning API");

try
{
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

    // configuration rabbitmq
    builder.Services.StartConsumers(builder.Configuration.GetConnectionString("RabbitMq"));
    builder.Services.AddPublisher();

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
    //builder.Services.AddAuthentication().AddGoogle(googleOptions =>
    //{
    //    var googleSection = builder.Configuration.GetSection("Google");
    //    googleOptions.ClientId = googleSection.GetValue<string>("ClientId");
    //    googleOptions.ClientSecret = googleSection.GetValue<string>("client_secret");
    //});

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
