using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using WebApplicationASP01.App;
using WebApplicationASP01.Extensions;
using WebApplicationASP01.Hubs;
using WebApplicationASP01.Services;

using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

// 1. Load .env file for local development (silently skipped on Railway if missing)
DotNetEnv.Env.Load();

// Disable config file watching to prevent inotify limit crashes in Linux / Railway containers
Environment.SetEnvironmentVariable("DOTNET_hostBuilder:reloadConfigOnChange", "false");
Environment.SetEnvironmentVariable("ASPNETCORE_hostBuilder:reloadConfigOnChange", "false");
Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "true");

var builder = WebApplication.CreateBuilder(args);

// Support Railway & dynamic container PORT environment variable
var webPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(webPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{webPort}");
}

// Global Exception Handling (Problem Details)
builder.Services.AddProblemDetails();

// Rate Limiting (Ochrana proti spamu)
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers["X-Forwarded-For"].ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 15, // 15 requests per second per IP
                QueueLimit = 2,
                Window = TimeSpan.FromSeconds(1)
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Add services to the container
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configure Custom Infrastructure (PostgreSQL & Redis)
builder.Services.AddCustomPostgres();
builder.Services.AddCustomRedis();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// App Services
builder.Services.AddScoped<WebApplicationASP01.Data.INoteRepository, WebApplicationASP01.Data.NoteRepository>();
builder.Services.AddScoped<WebApplicationASP01.Services.NoteService>();
builder.Services.AddSingleton<LinkService>();
builder.Services.AddSingleton<ChatHistoryService>();
builder.Services.AddScoped<SystemCheckService>();

var app = builder.Build();

// Ensure PostgreSQL database & tables are created on startup (Async Migrations)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var progLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await db.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        progLogger.LogWarning(ex, "Nepodařilo se spustit migrace PostgreSQL. Zkontrolujte připojení.");
    }

    var linkService = scope.ServiceProvider.GetRequiredService<LinkService>();
    if (linkService.IsRedisAvailable())
    {
        progLogger.LogInformation("Redis spojení je aktivní a připravené pro /link.");
    }
    else
    {
        progLogger.LogWarning("Redis server není momentálně dostupný. /link bude používat in-memory záložní režim.");
    }
}

app.UseForwardedHeaders();

// Configure HTTP pipeline
app.UseExceptionHandler(); // Uses ProblemDetails

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

// Ochrana proti spamu
app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");
app.MapHub<LinkHub>("/linkHub");
app.MapHub<WebApplicationASP01.Hubs.NotesHub>("/notesHub");

await app.RunAsync();
