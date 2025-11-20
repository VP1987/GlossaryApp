using FinitiGlossary.Application.Interfaces.Auth;
using FinitiGlossary.Application.Interfaces.Email;
using FinitiGlossary.Application.Interfaces.Repositories.Token;
using FinitiGlossary.Application.Interfaces.Repositories.UserIRepo;
using FinitiGlossary.Infrastructure.Auth;
using FinitiGlossary.Infrastructure.Auth.Services;
using FinitiGlossary.Infrastructure.DAL;
using FinitiGlossary.Infrastructure.Email;
using FinitiGlossary.Infrastructure.Repositories.Token;
using FinitiGlossary.Infrastructure.Repositories.UserRepo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinitiGlossary.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IEmailService, EmailService>();


            return services;
        }
    }
}
