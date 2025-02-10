using WorkdayCalendar.Configuration;
using WorkdayCalendar.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Services, DbContext Registry
builder.Services.AddServices(builder.Configuration);

//  CORS policy config
builder.Services.AddCorsPolicy(builder.Configuration);

// Swagger
builder.Services.AddSwagger();

// Controllers Registry
builder.Services.AddControllers();

var app = builder.Build();

// Enable Swagger for Development Env
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Workday Calendar API v1");
        c.RoutePrefix = "swagger";
    });
}

// Enable CORS policy
app.UseCors("AllowReactApp");

// Midldleware to handle global errors
app.UseMiddleware<GlobalErrorHandlingMiddleware>();

// Map controllers to routes
app.MapControllers();

app.Run();
