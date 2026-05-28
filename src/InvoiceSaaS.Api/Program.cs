using InvoiceSaaS.Api.Extensions;
using InvoiceSaaS.Api.Middleware;
using InvoiceSaaS.Infrastructure.Data.Seeds;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

// Seed / Migrate DB
await SeedData.InitializeAsync(app.Services);

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "InvoiceSaaS API v1");
    c.RoutePrefix = "swagger";
});

app.UseMiddleware<ExceptionMiddleware>();
app.UseSerilogRequestLogging();
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseCors("DefaultPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
   .WithTags("Health");

app.Run();

public partial class Program { }
