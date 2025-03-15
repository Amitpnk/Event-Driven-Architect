using MediatR;

namespace EventDrivenArchitect.Events;

public record EventReceived(string Message) : INotification;