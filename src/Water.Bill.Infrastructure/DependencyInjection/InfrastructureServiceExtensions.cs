using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Water.Bill.Application.Interfaces;
using Water.Bill.Infrastructure.Data;
using Water.Bill.Infrastructure.Services;

namespace Water.Bill.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServer =>
            {
                sqlServer.UseCompatibilityLevel(120);
                sqlServer.CommandTimeout(30);
                sqlServer.EnableRetryOnFailure(3);
                sqlServer.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            }));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<ISecuritySettingsService, SecuritySettingsService>();
        services.AddScoped<IConsumerOtpService, ConsumerOtpService>();
        services.AddScoped<IPublicNewConnectionOtpService, PublicNewConnectionOtpService>();
        services.AddScoped<IConsumerSmsSender, LoggingConsumerSmsSender>();
        services.AddScoped<IConsumerAccountService, ConsumerAccountService>();
        services.AddScoped<INewConnectionApplicationService, NewConnectionApplicationService>();
        services.AddScoped<INewConnectionFeeService, NewConnectionFeeService>();
        services.AddScoped<INewConnectionLookupService, NewConnectionLookupService>();
        services.AddScoped<INewConnectionFinalizationService, NewConnectionFinalizationService>();
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<IConsumerPaymentService, ConsumerPaymentService>();
        services.AddScoped<ITemplateRenderer, TemplateRenderer>();
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<ISmsSender, SmsSender>();
        services.AddScoped<IWhatsAppSender, WhatsAppSender>();
        services.AddScoped<IInAppNotificationSender, InAppNotificationSender>();
        services.AddScoped<ICommunicationService, CommunicationService>();
        services.AddScoped<INotificationDispatchService, NotificationDispatchService>();

        return services;
    }
}
