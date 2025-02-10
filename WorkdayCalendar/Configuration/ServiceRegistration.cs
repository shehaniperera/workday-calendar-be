using Microsoft.EntityFrameworkCore;
using WorkdayCalendar.Data;
using WorkdayCalendar.IRepository;
using WorkdayCalendar.IService;
using WorkdayCalendar.Service;
using WorkdayCalendar.Services;

namespace WorkdayCalendar.Configuration
{
    public static class ServiceRegistration
    {
        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            // DbContext Registry
            services.AddDbContext<WorkdayCalendarDBContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));


            services.AddLogging();

            // Repo Registry
            services.AddScoped<IHolidayRepository, HolidayRepository>();

            // Services Registry
            services.AddScoped<IHolidayService, HolidayService>();
            services.AddScoped<IHolidayValidatorService, HolidayValidatorService>();

            services.AddScoped<IWorkdayCalculationService, WorkdayCalculationService>();
            services.AddScoped<IWorkdayValidationService, WorkdayValidationService>();
            services.AddScoped<IDateModificationService, DateModificationService>();

            services.AddSingleton<IErrorHandlingService, ErrorHandlingService>();
        }
    }
}
