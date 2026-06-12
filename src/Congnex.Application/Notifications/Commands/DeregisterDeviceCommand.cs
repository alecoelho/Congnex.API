using Congnex.Application.Interfaces;
using MediatR;

namespace Congnex.Application.Notifications.Commands;

public record DeregisterDeviceCommand(string HubRegistrationId) : IRequest;

public sealed class DeregisterDeviceCommandHandler(INotificationService notifications)
    : IRequestHandler<DeregisterDeviceCommand>
{
    public Task Handle(DeregisterDeviceCommand req, CancellationToken ct) =>
        notifications.DeregisterDeviceAsync(req.HubRegistrationId);
}
