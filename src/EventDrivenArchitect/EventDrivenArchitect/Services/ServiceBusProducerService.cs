using EventDrivenArchitect.Configurations;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Polly.Retry;
using Polly;

namespace EventDrivenArchitect.Services
{
    public class ServiceBusProducerService
    {
        private readonly ServiceBusSettings _settings;
        private readonly ILogger<ServiceBusProducerService> _logger;
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly AsyncRetryPolicy _retryPolicy;

        public ServiceBusProducerService(IOptions<ServiceBusSettings> settings, ILogger<ServiceBusProducerService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            _client = new ServiceBusClient(_settings.ConnectionString);
            _sender = _client.CreateSender(_settings.TopicName);

            // Retry policy: 3 attempts with exponential backoff (2^n seconds)
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Retry {retryCount} for Service Bus message due to {exception.Message}. Waiting {timeSpan.TotalSeconds} seconds...");
                    });
        }

        public async Task SendMessageAsync(string message)
        {
            try
            {
                var serviceBusMessage = new ServiceBusMessage(message);

                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await _sender.SendMessageAsync(serviceBusMessage);
                    _logger.LogInformation("Message sent to Service Bus.");
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send message to Service Bus after retries: {ex.Message}");
            }
        }
    }

}
