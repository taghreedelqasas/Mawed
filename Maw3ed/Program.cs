using FluentValidation;
using Maw3ed.APIs.Hubs;
using Maw3ed.BLL;
using Maw3ed.BLL.Services.Classes;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.BLL.Validators;
using Maw3ed.DAL;
using Maw3ed.DAL.DoctorDev.DoctorManager;
using Maw3ed.DAL.DoctorDev.DoctorManager.DoctorManagerInterfaces;
using Maw3ed.DAL.Reposatries.Classes;
using Maw3ed.DAL.Reposatries.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.APIs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ==========================================================
            // 1. REGISTRATION OF SERVICES (Dependency Injection Container)
            // ==========================================================

            // DAL Services
            builder.Services.AddDALServices(builder.Configuration);
            //BLL
            builder.Services.AddBLLServices(builder.Configuration);
            builder.Services.AddHttpClient<IPaymobGateway, PaymobGateway>();
            builder.Services.AddScoped<IWalletService, WalletService>();
            
            builder.Services.AddScoped<IWithdrawService, WithdrawService>();
            // في Program.cs
            builder.Services.AddScoped<IAdminWithdrawService, AdminWithdrawService>();

            #region Services Merna
            builder.Services.AddScoped<IMedicalFileService, MedicalFileService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserProfileDtoValidator>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<IConversationService, ConversationService>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            #endregion

            //// Identity
            //builder.Services
            //    .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            //    {
            //        options.Password.RequireDigit = true;
            //        options.Password.RequiredLength = 8;
            //        options.Password.RequireNonAlphanumeric = false;
            //    })
            //    .AddEntityFrameworkStores<AppDbContext>()
            //    .AddDefaultTokenProviders();

            // إعدادات الـ JWT Authentication
            var jwtKey = builder.Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key missing in appsettings.json");

            builder.Services
                .AddAuthentication(opt =>
                {
                    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    opt.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("JWT ERROR:");
            Console.WriteLine(context.Exception);
            return Task.CompletedTask;
        }
    };
});

            // Controllers
            builder.Services.AddControllers();

            // OpenAPI (تعتمد عليها Scalar)
            builder.Services.AddOpenApi();

            // SignalR
            builder.Services.AddSignalR();

            // CORS
            var allowedOrigins = builder.Configuration
           .GetSection("Cors:AllowedOrigins")
           .Get<string[]>() ?? Array.Empty<string>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // المجر الخاص بالأطباء والـ Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IDoctorAvailabilityManager, DoctorAvailabilityManager>();
            builder.Services.AddScoped<IDoctorManager, DoctorManager>();

            // ==========================================================
            // 2. HTTP REQUEST PIPELINE (Middlewares)
            // ==========================================================
            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await AdminSeeder.SeedAsync(services);
            }
            // تفعيل الـ Scalar والـ OpenAPI في بيئة التطوير
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseStaticFiles();
            app.UseCors("AllowAll");

            app.UseHttpsRedirection();

            // ترتيب الـ Authentication والـ Authorization حرج جداً للـ [Authorize]
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHub<ChatHub>("/hubs/chat");
            app.MapControllers();
            app.Run();
        }
    }
}