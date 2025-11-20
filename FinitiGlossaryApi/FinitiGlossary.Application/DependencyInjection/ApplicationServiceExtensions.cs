using FinitiGlossary.Application.Aggregators.AdminHistory;
using FinitiGlossary.Application.Aggregators.AdminView;
using FinitiGlossary.Application.Interfaces.Admin;
using FinitiGlossary.Application.Interfaces.Auth;
using FinitiGlossary.Application.Services.Admin;
using FinitiGlossary.Application.Services.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace FinitiGlossary.Application.DependencyInjection
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAdminGlossaryService, AdminGlossaryService>();
            services.AddTransient<GlossaryAdminViewAggregator>();
            services.AddTransient<GlossaryAdminViewHistoryAggregator>();

            return services;
        }
    }
}