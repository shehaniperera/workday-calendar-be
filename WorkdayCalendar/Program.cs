using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WorkdayCalendar.Data;
using WorkdayCalendar.IRepository;
using WorkdayCalendar.Repository;
using WorkdayCalendar.Service;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext
builder.Services.AddDbContext<WorkdayCalendarDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services in the container
builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();

var AppUrl = builder.Configuration["AppUrl"];

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", builder =>
    {
        builder.WithOrigins(AppUrl)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});


// Add controllers
builder.Services.AddControllers();
// Register WorkdayService if you want to inject it instead of instantiating it directly
builder.Services.AddSingleton<WorkdayService>();

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Workday Calendar API",
        Version = "v1"
    });

});

var app = builder.Build();

// Configure middleware pipeline

// Enable Swagger
app.UseSwagger();
// Enable Swagger UI
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Workday Calendar API v1");
    c.RoutePrefix = "swagger";  // Swagger UI will be available at /swagger/index.html
});

// Enable CORS
app.UseCors("AllowReactApp");

// Enable Authentication
app.UseAuthentication();

// Enable Authorization
app.UseAuthorization();

// Map controllers for endpoint handling
app.MapControllers();

// Run the application
app.Run();
