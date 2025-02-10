using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WorkdayCalendar.Data;
using WorkdayCalendar.IRepository;
using WorkdayCalendar.Repository;
using WorkdayCalendar.Service;

var builder = WebApplication.CreateBuilder(args);

// DbContext registry
builder.Services.AddDbContext<WorkdayCalendarDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// services registry
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

// controllers
builder.Services.AddControllers();

builder.Services.AddSingleton<WorkdayService>();

//  Swagger for API documentation
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

// Get the current environment
var env = app.Environment;

if (env.IsDevelopment())
{
    // Enable Swagger
    app.UseSwagger();
    // Enable Swagger UI
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Workday Calendar API v1");
        c.RoutePrefix = "swagger";
    });
}


// Enable CORS
app.UseCors("AllowReactApp");

// Map controllers for endpoint handling
app.MapControllers();

// Run the application
app.Run();
