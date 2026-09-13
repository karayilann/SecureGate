using FluentAssertions;
using SecureGate.Application.Features.ApiKeys.Commands.CreateApiKey;
using SecureGate.Domain.Enums;
using Xunit;

namespace SecureGate.Tests;

public class CreateApiKeyValidatorTests
{
    private readonly CreateApiKeyCommandValidator _validator = new();

    [Fact]
    public void BosUserId_ValidasyonHatasiVermeli()
    {
        var command = new CreateApiKeyCommand(Guid.Empty, PlanType.Free);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GecerliVeri_ValidasyonuGecmeli()
    {
        var command = new CreateApiKeyCommand(Guid.NewGuid(), PlanType.Pro);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}