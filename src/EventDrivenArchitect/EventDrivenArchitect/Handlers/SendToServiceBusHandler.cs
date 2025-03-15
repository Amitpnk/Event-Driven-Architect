using EventDrivenArchitect.Events;
using EventDrivenArchitect.Services;
using MediatR;

namespace EventDrivenArchitect.Handlers;

public class SendToServiceBusHandler : INotificationHandler<ServiceBusMessageSent>
{
    private readonly ServiceBusProducerService _serviceBusProducer;
    private readonly ILogger<SendToServiceBusHandler> _logger;

    public SendToServiceBusHandler(ServiceBusProducerService serviceBusProducer, ILogger<SendToServiceBusHandler> logger)
    {
        _serviceBusProducer = serviceBusProducer;
        _logger = logger;
    }

    public async Task Handle(ServiceBusMessageSent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Sending message to Service Bus: {notification.Message}");
        await _serviceBusProducer.SendMessageAsync(notification.Message);
    }
}