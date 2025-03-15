using MediatR;

namespace EventDrivenArchitect.Events;

public record ServiceBusMessageSent(string Message) : INotification;