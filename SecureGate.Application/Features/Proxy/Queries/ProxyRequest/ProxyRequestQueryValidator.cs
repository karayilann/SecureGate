using FluentValidation;

namespace SecureGate.Application.Features.Proxy.Queries.ProxyRequest;

public class ProxyRequestQueryValidator : AbstractValidator<ProxyRequestQuery>
{
    public ProxyRequestQueryValidator()
    {
        RuleFor(x => x.Resource)
            .NotEmpty()
            .MaximumLength(200);
    }
}
