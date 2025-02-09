using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WorkdayCalendar.IRepository;
using WorkdayCalendar.Models;
using WorkdayCalendar.Repository;
using Microsoft.EntityFrameworkCore;
using WorkdayCalendar.Data;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext with InMemoryDatabase for testing
builder.Services.AddDbContext<WorkdayCalendarDBContext>(options =>
    options.UseInMemoryDatabase("TestDatabase"));

// Register services
builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();

var app = builder.Build();

app.MapPost("/api/holiday/AddHoliday", async (Holiday holiday, IHolidayRepository holidayRepository) =>
{
    if (holiday == null)
        return Results.BadRequest("Invalid request.");

    await holidayRepository.Add(holiday);
    return Results.Ok(new { message = "Holiday added successfully!" });
});

app.MapGet("/api/holiday/GetHolidays", async (IHolidayRepository holidayRepository) =>
{
    var holidays = await holidayRepository.GetAllAsync();
    return Results.Ok(new { Result = holidays });
});

app.Run();
