using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Water.Bill.Application.Validators;

namespace Water.Bill.Application.DependencyInjection;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        return services;
    }
}
