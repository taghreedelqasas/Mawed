using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Maw3ed.BLL
{
    public static class ServiceExtention
    {
        public static void AddBLLServices(this IServiceCollection services, IConfiguration configuration)
        {
            // JWT settings (read from appsettings.json "JwtSettings" section)
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

            // Brevo (email) settings
            services.Configure<BrevoSettings>(configuration.GetSection("BrevoSettings"));

            // Managers
            services.AddScoped<IAuthManager, AuthManager>();
            services.AddScoped<ITokenManager, TokenManager>();
            services.AddScoped<IEmailService, EmailService>();
          

            // FluentValidation - auto registers every validator in this assembly
            // (RegisterDTOValidator, LoginDTOValidator, ...).
            services.AddScoped<IValidator<LoginDto>, LoginDtoValidator>();
            services.AddScoped<IValidator<RegisterDto>, RegisterDtoValidator>();
            //services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            // services.AddValidatorsFromAssemblyContaining<LoginDtoValidator>();
            //services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
        }
    }
}
