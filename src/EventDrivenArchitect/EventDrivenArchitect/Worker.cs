using EventDrivenArchitect.Services;

namespace EventDrivenArchitect;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly EventHubConsumerService _eventHubConsumerService;

    public Worker(ILogger<Worker> logger,   EventHubConsumerService eventHubConsumerService)
    {
        _logger = logger;
        _eventHubConsumerService = eventHubConsumerService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        _logger.LogInformation($"Starting event hub");

        while (!stoppingToken.IsCancellationRequested)
        {
            await _eventHubConsumerService.StartProcessingAsync();
            
            await Task.Delay(1000, stoppingToken);
        }
    }
}