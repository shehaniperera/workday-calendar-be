using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using WorkdayCalendar.IRepository;
using WorkdayCalendar.Models;
using Microsoft.EntityFrameworkCore;
using WorkdayCalendar.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WorkdayCalendar.IService;
using WorkdayCalendar.Service;
using WorkdayCalendar.Services;

var builder = WebApplication.CreateBuilder(args);

// InMemoryDatabase 
builder.Services.AddDbContext<WorkdayCalendarDBContext>(options =>
    options.UseInMemoryDatabase("TestDatabase"));

// Repo Registry
builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();

// Services Registry
builder.Services.AddScoped<IHolidayService, HolidayService>();
builder.Services.AddScoped<IHolidayValidatorService, HolidayValidatorService>();

builder.Services.AddScoped<IWorkdayCalculationService, WorkdayCalculationService>();
builder.Services.AddScoped<IWorkdayValidationService, WorkdayValidationService>();
builder.Services.AddScoped<IDateModificationService, DateModificationService>();

// Singleton Services
builder.Services.AddSingleton<IErrorHandlingService, ErrorHandlingService>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);



var app = builder.Build();

app.MapPost("/api/holiday/AddHoliday", async (Holiday holiday, IHolidayService holidayService) =>
{
    if (holiday == null)
        return Results.BadRequest(new { message = "Invalid request." });

    if (holiday.Id == Guid.Empty || holiday.Date == DateTime.MinValue || string.IsNullOrEmpty(holiday.Name) || holiday.IsRecurring == null)
        return Results.BadRequest(new { message = "Holiday has incomplete or empty values" });

    await holidayService.AddHolidayAsync(holiday);
    return Results.Ok(new { message = "Holiday added successfully!" });
});

app.MapGet("/api/holiday/GetHolidays", async (IHolidayService IHolidayService) =>
{
    var holidays = await IHolidayService.GetAllHolidaysAsync();
    return Results.Ok(new { Result = holidays, Count = holidays.Count() });
});

app.MapGet("/api/holiday/GetRecurringHolidays", async (IHolidayService holidayService) =>
{
    var holidays = await holidayService.GetRecurringHolidaysAsync();
    return Results.Ok(new { Result = holidays, Count = holidays.Count() });
});

app.MapGet("/api/holiday/GetFixedHolidays", async (IHolidayService holidayService) =>
{
    var holidays = await holidayService.GetFixedHolidaysAsync();
    return Results.Ok(new { Result = holidays, Count = holidays.Count() });
});

app.MapGet("/api/holiday/GetHolidaysById", async (IHolidayService holidayService, Guid id) =>
{
    var holiday = await holidayService.GetHolidayByIdAsync(id);
    return Results.Ok(new { Result = holiday });
});


app.MapPost("/api/holiday/UpdateHoliday", async (Holiday holiday, IHolidayService holidayService) =>
{
    if (holiday == null)
        return Results.BadRequest(new { message = "Invalid request." });

    if (holiday.Id == Guid.Empty || holiday.Date == DateTime.MinValue || string.IsNullOrEmpty(holiday.Name) || holiday.IsRecurring == null)
        return Results.BadRequest(new { message = "Holiday has incomplete or empty values" });

    await holidayService.UpdateHolidayAsync(holiday);
    return Results.Ok(new { message = "Holiday updated successfully!" });
});

app.MapDelete("/api/holiday/DeleteHoliday", async (IHolidayService holidayService, Guid id) =>
{
    var success = await holidayService.DeleteHolidayAsync(id);

    if (success)
    {
        return Results.Ok(new { message = "Holiday deleted successfully!" });
    }

    return Results.NotFound(new { message = "Holiday not found" });
});


app.MapPost("/api/workday/CalculateWorkDay", async (WorkdayCalculation request, IWorkdayCalculationService workdayService) =>
{
    if (request == null)
        return Results.BadRequest("Invalid workday calculation request.");

    var resultDateTime = await workdayService.CalculateWorkday(request);

    if (resultDateTime == DateTime.MinValue)
        return Results.Problem("Error occurred while calculating the workday.");

    return Results.Ok(new { result = resultDateTime });
});

app.Run();
