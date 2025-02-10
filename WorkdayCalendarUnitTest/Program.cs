using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WorkdayCalendar.IRepository;
using WorkdayCalendar.Models;
using WorkdayCalendar.Repository;
using Microsoft.EntityFrameworkCore;
using WorkdayCalendar.Data;
using Microsoft.AspNetCore.Http;
using WorkdayCalendar.IService;
using WorkdayCalendar.Service;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// InMemoryDatabase 
builder.Services.AddDbContext<WorkdayCalendarDBContext>(options =>
    options.UseInMemoryDatabase("TestDatabase"));

//  services registry
builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();
builder.Services.AddScoped<IWorkdayService, WorkdayService>();

// config registry
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);


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

app.MapGet("/api/holiday/GetRecurringHolidays", async (IHolidayRepository holidayRepository) =>
{
    var holidays = await holidayRepository.GetRecurringHolidaysAsync();
    return Results.Ok(new { Result = holidays });
});

app.MapGet("/api/holiday/GetFixedHolidays", async (IHolidayRepository holidayRepository) =>
{
    var holidays = await holidayRepository.GetFixedHolidaysAsync();
    return Results.Ok(new { Result = holidays });
});

app.MapGet("/api/holiday/GetHolidaysById", async (IHolidayRepository holidayRepository,Guid id) =>
{
    var holidays = await holidayRepository.GetByIdAsync(id);
    return Results.Ok(new { Result = holidays });
});

app.MapPatch("/api/holiday/UpdateHoliday", async (Holiday holiday, IHolidayRepository holidayRepository) =>
{
    if (holiday == null)
        return Results.BadRequest("Invalid request.");

    await holidayRepository.Update(holiday);
    return Results.Ok(new { message = "Holiday updated successfully!" });
});

app.MapDelete("/api/holiday/DeleteHoliday", async (IHolidayRepository holidayRepository, Guid id) =>
{
    var holidays = await holidayRepository.DeleteAsync(id);
    return Results.Ok(new { Result = holidays });
});


app.MapPost("/api/workday/CalculateWorkDay", async (WorkdayCalculation request, IWorkdayService workdayService) =>
{
    if (request == null)
        return Results.BadRequest("Invalid workday calculation request.");

    var resultDateTime = await workdayService.CalculateWorkday(request);

    if (resultDateTime == DateTime.MinValue)
        return Results.Problem("Error occurred while calculating the workday.");

    return Results.Ok(new { calculatedWorkday = resultDateTime });
});


app.Run();
