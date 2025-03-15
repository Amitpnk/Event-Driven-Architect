using System.Runtime;
using System.Text;
using Azure.Messaging.EventHubs.Consumer;
using EventDrivenArchitect.Configurations;
using Microsoft.Extensions.Options;

namespace EventDrivenArchitect;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly EventHubSettings _settings;

    public Worker(ILogger<Worker> logger, IOptions<EventHubSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        _logger.LogInformation($"Starting event hub");

        string ehubNamespaceConnectionString = _settings.ConnectionString;
        string eventHubName = _settings.EventHubName;
        string consumerGroup = _settings.ConsumerGroup;

        while (!stoppingToken.IsCancellationRequested)
        {
            var consumer = new EventHubConsumerClient(consumerGroup, ehubNamespaceConnectionString, eventHubName);

            await foreach (PartitionEvent partitionEvent in consumer.ReadEventsAsync())
            {
                _logger.LogInformation($"Message Received: {Encoding.Default.GetString(partitionEvent.Data.Body.Span)}");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}