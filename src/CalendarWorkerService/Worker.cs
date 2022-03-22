using CalendarServices;

namespace CalendarWorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private IGoogleCalendarService googleCalendarService;
    public Worker(ILogger<Worker> logger, IGoogleCalendarService googleCalendarService)
    {
        _logger = logger;
        this.googleCalendarService = googleCalendarService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            //start listener (Kestrel op poort 5000)
            //als er event binnen komt -> googleCalendarService.getEvents();    //of kijk in de uri wat er binnen komt in de json
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);


        }
    }
}
