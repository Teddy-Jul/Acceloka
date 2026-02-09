using Microsoft.EntityFrameworkCore;
using Serilog;
using Acceloka.Entities;
using Acceloka.Services;
using Hellang.Middleware.ProblemDetails;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
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

// Configure Problem Details (RFC 7807)
builder.Services.AddProblemDetails(options =>
{
    options.IncludeExceptionDetails = (ctx, ex) => builder.Environment.IsDevelopment();
    
    options.MapToStatusCode<ArgumentException>(StatusCodes.Status400BadRequest);
    options.MapToStatusCode<InvalidOperationException>(StatusCodes.Status400BadRequest);
    options.MapToStatusCode<KeyNotFoundException>(StatusCodes.Status404NotFound);
    options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
});

builder.Services.AddEntityFrameworkSqlServer();
builder.Services.AddDbContextPool<AccelokaContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServerDB"));
});

builder.Services.AddTransient<TicketServices>();

var app = builder.Build();

app.UseProblemDetails();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseSerilogRequestLogging();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
