using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using XlerionLeaderboardAPI.Data;
using XlerionLeaderboardAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    // Evita excepciones por ciclos de navegación (EF Core entities with backrefs)
    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Xlerion Leaderboard API",
        Version = "v1",
        Description = "REST API for managing game scores, rankings, and achievements across Xlerion games."
    });
});

string connectionString;
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Railway usa formato: postgresql://user:password@host:port/database
    // Npgsql necesita: Host=...;Port=...;Database=...;Username=...;Password=...
    try
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = Uri.UnescapeDataString(userInfo[1]);
        connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        Console.WriteLine($"[Startup] Using Railway PostgreSQL at {host}:{port}/{database}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup] Error parsing DATABASE_URL: {ex.Message}");
        Console.WriteLine($"[Startup] DATABASE_URL value starts with: {databaseUrl.Substring(0, Math.Min(20, databaseUrl.Length))}...");
        throw;
    }
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    Console.WriteLine($"[Startup] Using local connection string");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("XlerionPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://xlerion.com",
                "https://*.xlerion.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Xlerion Leaderboard API v1");
    c.RoutePrefix = string.Empty;
});

// Global JSON exception handler
app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var err = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        var message = err?.Message ?? "An unexpected error occurred.";
        var payload = System.Text.Json.JsonSerializer.Serialize(new { error = message, statusCode = context.Response.StatusCode });
        await context.Response.WriteAsync(payload);
    });
});

app.UseCors("XlerionPolicy");
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Apply migrations in normal environments to keep schema managed
    db.Database.Migrate();
    await SeedData.InitializeAsync(db);
}

app.Run();
