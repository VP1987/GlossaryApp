using FinitiGlossary.Application.Interfaces.Auth;
using FinitiGlossary.Application.Services.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace FinitiGlossary.Application.DependencyInjection
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
