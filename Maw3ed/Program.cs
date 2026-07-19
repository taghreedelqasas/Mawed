using FluentValidation;
using Maw3ed.APIs.Hubs;
using Maw3ed.BLL;
using Maw3ed.BLL.AI.Configuration;
using Maw3ed.BLL.AI.Interfaces;
using Maw3ed.BLL.AI.MedicalImages;
using Maw3ed.BLL.AI.MedicalReports;
using Maw3ed.BLL.AI.Services;
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
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

namespace Maw3ed.APIs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ==========================================================
            // 1. REGISTRATION OF SERVICES (Dependency Injection)
            // ==========================================================

            Console.WriteLine(builder.Configuration.GetConnectionString("DefaultConnection"));

            // ---------------- DAL ----------------
            builder.Services.AddDALServices(builder.Configuration);

            // ---------------- BLL ----------------
            builder.Services.AddBLLServices(builder.Configuration);

            // ---------------- Payment Services ----------------
            builder.Services.AddHttpClient<IPaymobGateway, PaymobGateway>();

            builder.Services.AddScoped<IWalletService, WalletService>();
            builder.Services.AddScoped<IWithdrawService, WithdrawService>();
            builder.Services.AddScoped<IAdminWithdrawService, AdminWithdrawService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();

            // ---------------- Application Services ----------------
            builder.Services.AddScoped<IMedicalFileService, MedicalFileService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<IConversationService, ConversationService>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            builder.Services.AddScoped<IAdminPaymentsService, AdminPaymentsService>();
            builder.Services.AddScoped<IDoctorService, DoctorService>();
            builder.Services.AddScoped<IAdminAppointmentService, AdminAppointmentService>();
            builder.Services.AddHttpClient<IChatService, ChatService>();

            // ---------------- Validation ----------------
            builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserProfileDtoValidator>();

            // ---------------- AI ----------------
            builder.Services.Configure<StudentBedrockSettings>(
                builder.Configuration.GetSection("StudentBedrock"));

            builder.Services.AddScoped<IMedicalReportService, MedicalReportService>();
            builder.Services.AddHttpClient<IMedicalImageService, MedicalImageService>();

            // ---------------- Identity ----------------
            //builder.Services
            //    .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            //    {
            //        options.Password.RequireDigit = true;
            //        options.Password.RequiredLength = 8;
            //        options.Password.RequireNonAlphanumeric = false;
            //    })
            //    .AddEntityFrameworkStores<AppDbContext>()
            //    .AddDefaultTokenProviders();

            // ---------------- JWT Authentication ----------------
            builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));
            var jwtSettings = builder.Configuration
                .GetSection("JwtSettings")
                .Get<JwtSettings>()!;
            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.Key)
                        ),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine("JWT ERROR:");
                            Console.WriteLine(context.Exception);
                            return Task.CompletedTask;
                        },

                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/hubs/chat"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            // ---------------- Controllers ----------------
            builder.Services.AddControllers();

            // ---------------- OpenAPI / Scalar ----------------
            builder.Services.AddOpenApi();

            // ---------------- SignalR ----------------
            builder.Services.AddSignalR();

            // ---------------- Rate Limiting ----------------
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddPolicy("chat", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey:
                            httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? httpContext.Connection.RemoteIpAddress?.ToString()
                            ?? "anonymous",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromHours(1),
                            QueueLimit = 0
                        }));
            });

            // ---------------- CORS ----------------
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? Array.Empty<string>();

            if (!allowedOrigins.Contains("http://localhost:4200"))
            {
                allowedOrigins = allowedOrigins.Append("http://localhost:4200").ToArray();
            }

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
            // ---------------- Repositories & Managers ----------------
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IDoctorAvailabilityManager, DoctorAvailabilityManager>();
            builder.Services.AddScoped<IDoctorManager, DoctorManager>();

            // ==========================================================
            // 2. HTTP REQUEST PIPELINE (Middlewares)
            // ==========================================================

            var app = builder.Build();

            //using (var scope = app.Services.CreateScope())
            //{
            //    var services = scope.ServiceProvider;
            //    await AdminSeeder.SeedAsync(services);
            //}

            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseCors("AllowAll");

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseRateLimiter();

            app.MapHub<ChatHub>("/hubs/chat");

            app.MapControllers();

            app.Run();
        }
    }
}