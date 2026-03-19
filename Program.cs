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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Xlerion Leaderboard API v1");
        c.RoutePrefix = string.Empty;
    });
}

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
