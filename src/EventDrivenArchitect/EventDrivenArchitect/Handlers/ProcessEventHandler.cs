using EventDrivenArchitect.Events;
using MediatR;

namespace EventDrivenArchitect.Handlers;

public class ProcessEventHandler : INotificationHandler<EventReceived>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProcessEventHandler> _logger;

    public ProcessEventHandler(IMediator mediator, ILogger<ProcessEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(EventReceived notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Processing event: {notification.Message}");

        // Pass message to the next handler for Service Bus publishing
        await _mediator.Publish(new ServiceBusMessageSent(notification.Message), cancellationToken);
    }
}