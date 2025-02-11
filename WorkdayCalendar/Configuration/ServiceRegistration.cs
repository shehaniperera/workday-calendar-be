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
        // Main registration method
        public static void AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabaseServices(configuration);
            services.AddLoggingServices();
            services.AddRepositoryServices();
            services.AddApplicationServices();
        }

        //  DbContext Registry
        private static void AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<WorkdayCalendarDBContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        }

        // Logging-related services Registry
        private static void AddLoggingServices(this IServiceCollection services)
        {
            services.AddLogging();
        }

        // Reposito Registry
        private static void AddRepositoryServices(this IServiceCollection services)
        {
            services.AddScoped<IHolidayRepository, HolidayRepository>();
        }

        // Application Services Registry
        private static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IHolidayService, HolidayService>();
            services.AddScoped<IHolidayValidatorService, HolidayValidatorService>();
            services.AddScoped<IWorkdayCalculationService, WorkdayCalculationService>();
            services.AddScoped<IWorkdayValidationService, WorkdayValidationService>();
            services.AddScoped<IDateModificationService, DateModificationService>();

            // Singleton service for error
            services.AddSingleton<IErrorHandlingService, ErrorHandlingService>();
        }
    }
}
