using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    var kestrelSection = context.Configuration.GetSection("Kestrel");

    serverOptions.ConfigureEndpointDefaults(listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
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

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", context =>
{
    ;
    context.Response.ContentType = "text/plain";
    //Request ontvangen, dus opnieuw de lijst van events opvragen, want er is iets gewijzigd. 

    //https://www.googleapis.com/calendar/v3/calendars/planning.integrationproject.ehb%40gmail.com/events?alt=json 
    Console.WriteLine("Got something");
    return context.Response.WriteAsync("Hello World!");
});

app.Run();
