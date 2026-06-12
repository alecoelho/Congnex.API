using Congnex.Application.Interfaces;
using Congnex.Domain.Enums;
using MediatR;

namespace Congnex.Application.Notifications.Commands;

public record RegisterDeviceCommand(Guid UserId, string Token, DevicePlatform Platform)
    : IRequest<string>;

public sealed class RegisterDeviceCommandHandler(INotificationService notifications)
    : IRequestHandler<RegisterDeviceCommand, string>
{
    public Task<string> Handle(RegisterDeviceCommand req, CancellationToken ct) =>
        notifications.RegisterDeviceAsync(req.Token, req.Platform, req.UserId);
}
