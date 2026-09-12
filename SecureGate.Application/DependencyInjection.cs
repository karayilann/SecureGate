using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SecureGate.Application.Mapping;

namespace SecureGate.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}