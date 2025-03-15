using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs.Consumer;
using EventDrivenArchitect.Configurations;
using EventDrivenArchitect.Events;
using MediatR;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Wrap;

namespace EventDrivenArchitect.Services
{
    public class EventHubConsumerService
    {
        private readonly EventHubSettings _settings;
        private readonly ILogger<EventHubConsumerService> _logger;
        private readonly ServiceBusProducerService _serviceBusProducer;
        private readonly IMediator _mediator;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly AsyncPolicyWrap _policyWrap;

        public EventHubConsumerService(IOptions<EventHubSettings> settings,
            ILogger<EventHubConsumerService> logger, ServiceBusProducerService serviceBusProducer, IMediator mediator)
        {
            _settings = settings.Value;
            _logger = logger;
            _serviceBusProducer = serviceBusProducer;
            _mediator = mediator;

            // Retry policy: 3 retries with exponential backoff
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Retry {retryCount} for Event Hub processing due to {exception.Message}. Waiting {timeSpan.TotalSeconds} seconds...");
                    });

            // Circuit Breaker: Open after 5 failures, reset after 30 seconds
            _circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
                    (exception, duration) =>
                    {
                        _logger.LogError($"Circuit opened for Event Hub consumer. Blocking requests for {duration.TotalSeconds} seconds.");
                    },
                    () =>
                    {
                        _logger.LogInformation("Event Hub consumer circuit reset. Resuming normal operations.");
                    });

            // Wrap Retry & Circuit Breaker policies
            _policyWrap = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy);
        }

        public async Task StartProcessingAsync()
        {
            _logger.LogInformation("Starting Event Hub consumer...");

            await using var consumer = new EventHubConsumerClient(
                _settings.ConsumerGroup,
                _settings.ConnectionString,
                _settings.EventHubName);

            await foreach (PartitionEvent receivedEvent in consumer.ReadEventsAsync())
            {
                if (receivedEvent.Data != null)
                {
                    string message = Encoding.UTF8.GetString(receivedEvent.Data.Body.ToArray());
                    _logger.LogInformation($"Received message: {message}");

                    await _policyWrap.ExecuteAsync(async () =>
                    {
                        await _mediator.Publish(new EventReceived(message));
                        //await _serviceBusProducer.SendMessageAsync(message);
                    });
                }
            }
        }
    }

}
