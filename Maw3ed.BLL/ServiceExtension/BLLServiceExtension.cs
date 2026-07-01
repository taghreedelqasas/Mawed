using Maw3ed.BLL.Services.Classes;
using Maw3ed.BLL.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Maw3ed.BLL.ServiceExtension
{
    public static class BLLServiceExtension
    {
        public static void AddBLLServices(this IServiceCollection services)
        {
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IReviewService, ReviewService>();
        }
    }
}
