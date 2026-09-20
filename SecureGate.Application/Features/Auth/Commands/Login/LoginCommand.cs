using MediatR;

namespace SecureGate.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<string?>;
