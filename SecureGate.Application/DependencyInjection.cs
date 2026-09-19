using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SecureGate.Application.Anomaly;
using SecureGate.Application.Common.Behaviors;
using SecureGate.Application.Interfaces;
using SecureGate.Application.Mapping;
using System.Reflection;

namespace SecureGate.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
        services.AddValidatorsFromAssembly(assembly);
        services.AddSingleton<IAnomalyDetector, AnomalyDetector>();

        return services;
    }
}