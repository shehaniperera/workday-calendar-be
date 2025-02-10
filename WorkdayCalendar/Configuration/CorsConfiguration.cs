namespace WorkdayCalendar.Configuration
{
    public static class CorsConfiguration
    {
        public static void AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
        {
            var applicationUrl = configuration["AppUrl"];

            services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", builder =>
                {
                    builder.WithOrigins(applicationUrl)
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }
    }
}
