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
using Polly.CircuitBreaker;
using Polly.Wrap;

namespace EventDrivenArchitect.Services
{
    public class ServiceBusProducerService
    {
        private readonly ServiceBusSettings _settings;
        private readonly ILogger<ServiceBusProducerService> _logger;
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly ServiceBusSender _deadLetterSender;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly AsyncPolicyWrap _policyWrap;

        public ServiceBusProducerService(IOptions<ServiceBusSettings> settings, ILogger<ServiceBusProducerService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            _client = new ServiceBusClient(_settings.ConnectionString);
            _sender = _client.CreateSender(_settings.TopicName);
            _deadLetterSender = _client.CreateSender($"{_settings.TopicName}/$DeadLetterQueue");

            // Retry policy: 3 attempts with exponential backoff (2^n seconds)
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Retry {retryCount} for Service Bus message due to {exception.Message}. Waiting {timeSpan.TotalSeconds} seconds...");
                    });

            // Circuit Breaker: Open after 5 failures, reset after 30 seconds
            _circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
                    (exception, duration) =>
                    {
                        _logger.LogError($"Circuit opened due to repeated failures. Blocking requests for {duration.TotalSeconds} seconds.");
                    },
                    () =>
                    {
                        _logger.LogInformation("Circuit reset. Resuming normal operations.");
                    });

            // Wrap Retry & Circuit Breaker policies
            _policyWrap = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy);
        }

        public async Task SendMessageAsync(string message)
        {
            try
            {
                var serviceBusMessage = new ServiceBusMessage(message);

                await _policyWrap.ExecuteAsync(async () =>
                {
                    await _sender.SendMessageAsync(serviceBusMessage);
                    _logger.LogInformation("Message sent to Service Bus.");
                });
            }
            catch (BrokenCircuitException)
            {
                _logger.LogError("Circuit breaker is open. Message will be sent to Dead-Letter Queue.");
                await SendToDeadLetterQueueAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send message after retries: {ex.Message}");
                await SendToDeadLetterQueueAsync(message);
            }
        }

        private async Task SendToDeadLetterQueueAsync(string message)
        {
            try
            {
                var deadLetterMessage = new ServiceBusMessage(message);
                await _deadLetterSender.SendMessageAsync(deadLetterMessage);
                _logger.LogWarning("Message moved to Dead-Letter Queue.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send message to Dead-Letter Queue: {ex.Message}");
            }
        }
    }

}
