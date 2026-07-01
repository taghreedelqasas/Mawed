
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

            //DAL
            builder.Services.AddDALServices(builder.Configuration);

            // Identity
            builder.Services
                .AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IDoctorAvailabilityManager, DoctorAvailabilityManager>();
            builder.Services.AddScoped<IDoctorManager, DoctorManagerClasses>();
            var app = builder.Build();

           
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
