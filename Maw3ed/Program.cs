
using FluentValidation;
using Maw3ed.APIs.Hubs;
using Maw3ed.BLL.Services.Classes;
using Maw3ed.BLL.Services.Interfaces;
using Maw3ed.BLL.Validators;
using Maw3ed.DAL;
using Maw3ed.DAL.Reposatries.Classes;
using Maw3ed.DAL.Reposatries.Interfaces;
using Maw3ed.DAL.DoctorDev.DoctorManager.DoctorManagerInterfaces;
using Microsoft.AspNetCore.Identity;
using Maw3ed.DAL.DoctorDev.DoctorManager;
using Scalar.AspNetCore;

namespace Maw3ed
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // DAL
            builder.Services.AddDALServices(builder.Configuration);

            #region Services Merna
            builder.Services.AddScoped<IMedicalFileService, MedicalFileService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserProfileDtoValidator>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<IConversationService, ConversationService>();
            #endregion

            // Identity
            builder.Services
                .AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // Controllers
            builder.Services.AddControllers();

            // OpenAPI
            builder.Services.AddOpenApi();

            // SignalR
            builder.Services.AddSignalR();

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins("null", "http://localhost")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IDoctorAvailabilityManager, DoctorAvailabilityManager>();
            builder.Services.AddScoped<IDoctorManager, DoctorManagerClasses>();
            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseStaticFiles();
            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapHub<ChatHub>("/hubs/chat");
            app.MapControllers();
            app.Run();
        }
    }
}