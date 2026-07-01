using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// ── الاستدعاء الصحيح 100% حسب الفولدرات اللي على اليمين ──
using Maw3ed.DAL;                                 // عشان الـ AddDALServices
using Maw3ed.BLL;                                 // عشان الـ AddBLLServices
using Maw3ed.DAL.Reposatries.Interfaces;          // لأن الـ Interfaces جوة الـ Repositories
using Maw3ed.DAL;                // لأن الـ Context جوة الـ Data
//using Maw3ed.DAL.Data.Models;            // المسار الصحيح للـ ApplicationUser والـ ApplicationRole
using Maw3ed.BLL.ServiceExtension;

namespace Maw3ed.APIs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ==========================================================
            // 1. REGISTRATION OF SERVICES (Dependency Injection Container)
            // ==========================================================

            // ── الـ DAL Services (DbContext + UnitOfWork + Repositories) ──
            builder.Services.AddDALServices(builder.Configuration);

            // ── الـ BLL Services (Business Logic Services) ──
            builder.Services.AddBLLServices();

            // ── نظام الـ Identity لإدارة المستخدمين والأدوار ──
            builder.Services
                .AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // ── إعدادات الـ JWT Authentication ──
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
                });

            // ── إضافة الـ Controllers لخدمة الـ APIs ──
            builder.Services.AddControllers();

            // ── إعداد الـ Swagger وتأمينه ليدعم إرسال الـ JWT Token ──
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Maw3ed API", Version = "v1" });

                // إعداد شكل خانة الـ Authorize في Swagger UI
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "أكتب في الخانة: Bearer {your_token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // ==========================================================
            // 2. HTTP REQUEST PIPELINE (Middlewares)
            // ==========================================================
            var app = builder.Build();

            // تفعيل Swagger في بيئة التطوير فقط
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Maw3ed API v1"));
            }

            app.UseHttpsRedirection();

            // ⚠️ الترتيب هنا إجباري وحرج جداً لعمل الـ [Authorize] بالشكل الصحيح
            app.UseAuthentication(); // التحقق من الهوية (الـ Token) أولاً
            app.UseAuthorization();  // التحقق من الصلاحيات ثانياً

            app.MapControllers();

            app.Run();
        }
    }
}