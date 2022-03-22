using CalendarWorkerService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddLogging();                  //TODO: logging instellen in appsettings. (console logger en naar file).    

        //services.AddSingleton<>               //TODO: add singleton for calendar service
    })
    .Build();


await host.RunAsync();
