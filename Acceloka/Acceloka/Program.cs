using Microsoft.EntityFrameworkCore;
using Serilog;
using Acceloka.Entities;
using Acceloka.Common.Behaviors;
using Hellang.Middleware.ProblemDetails;
using FluentValidation;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("logs/Log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Acceloka API",
        Version = "v1",
        Description = "Online Ticket Booking System API"
    });
});

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// FluentValidation Pipeline Behavior
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Configure Problem Details (RFC 7807)
builder.Services.AddProblemDetails(options =>
{ 
    options.IncludeExceptionDetails = (ctx, ex) => builder.Environment.IsDevelopment();
    
    options.MapToStatusCode<ArgumentException>(StatusCodes.Status400BadRequest);
    options.MapToStatusCode<InvalidOperationException>(StatusCodes.Status400BadRequest);
    options.MapToStatusCode<ValidationException>(StatusCodes.Status400BadRequest);
    options.MapToStatusCode<KeyNotFoundException>(StatusCodes.Status404NotFound);
    options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
});

builder.Services.AddEntityFrameworkSqlServer();
builder.Services.AddDbContextPool<AccelokaContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServerDB"));
});

var app = builder.Build();

app.UseProblemDetails();

app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();
app.UseAuthorization();
app.MapControllers();

app.Run();