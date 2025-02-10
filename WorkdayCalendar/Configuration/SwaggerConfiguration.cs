using Microsoft.OpenApi.Models;

namespace WorkdayCalendar.Configuration
{
    public static class SwaggerConfiguration
    {
        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Workday Calendar API",
                    Version = "v1"
                });
            });
        }
    }
}
