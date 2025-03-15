using EventDrivenArchitect.Common.Configurations;
using EventDrivenArchitect.Services;
using Microsoft.Extensions.Options;

namespace EventDrivenArchitect;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly EventHubConsumerService _eventHubConsumerService;
    private readonly EventHubSettings _settings;

    public Worker(ILogger<Worker> logger, IOptions<EventHubSettings> settings, EventHubConsumerService eventHubConsumerService)
    {
        _logger = logger;
        _eventHubConsumerService = eventHubConsumerService;
        _settings = settings.Value;
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