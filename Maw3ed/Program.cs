
using Maw3ed.BLL.AI.Configuration;
using Maw3ed.BLL.AI.Interfaces;
using Maw3ed.BLL.AI.MedicalReports;
using Maw3ed.BLL.AI.Services;
using Maw3ed.DAL;
using Microsoft.AspNetCore.Identity;
using Maw3ed.BLL.AI.MedicalImages;


namespace Maw3ed
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //DAL
            builder.Services.AddDALServices(builder.Configuration);

            // Identity
            builder.Services
                .AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
            //AI
            builder.Services.AddHttpClient<IChatService, ChatService>();

            builder.Services.Configure<StudentBedrockSettings>(
    builder.Configuration.GetSection("StudentBedrock"));



            //builder.Services.AddScoped<IChatService, ChatService>();
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            //builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IMedicalReportService, MedicalReportService>();

            //builder.Services.AddScoped<IMedicalImageService, MedicalImageService>();
            builder.Services.AddHttpClient<IMedicalImageService, MedicalImageService>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
