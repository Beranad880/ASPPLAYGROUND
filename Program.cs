using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using WebApplicationASP01.App;
using WebApplicationASP01.Hubs;
using WebApplicationASP01.Services;

// 1. Load .env file for local development (silently skipped on Railway if missing)
DotNetEnv.Env.Load();

// Disable config file watching to prevent inotify limit crashes in Linux / Railway containers
Environment.SetEnvironmentVariable("DOTNET_hostBuilder:reloadConfigOnChange", "false");
Environment.SetEnvironmentVariable("ASPNETCORE_hostBuilder:reloadConfigOnChange", "false");
Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "true");

// 2. Read PostgreSQL environment variables & construct connection string
var pghost = Environment.GetEnvironmentVariable("PGHOST");
var pgport = Environment.GetEnvironmentVariable("PGPORT");
var pgdatabase = Environment.GetEnvironmentVariable("PGDATABASE");
var pguser = Environment.GetEnvironmentVariable("PGUSER");
var pgpassword = Environment.GetEnvironmentVariable("PGPASSWORD");

string connectionString;

if (!string.IsNullOrEmpty(pghost) && !string.IsNullOrEmpty(pgdatabase))
{
    var port = !string.IsNullOrEmpty(pgport) ? pgport : "5432";
    var user = !string.IsNullOrEmpty(pguser) ? pguser : "postgres";
    var pass = !string.IsNullOrEmpty(pgpassword) ? pgpassword : "";
    var sslMode = Environment.GetEnvironmentVariable("PGSSLMODE")
        ?? (pghost == "localhost" || pghost == "127.0.0.1" ? "Prefer" : "Require");
    var trustCert = Environment.GetEnvironmentVariable("PGTRUSTSERVERCERTIFICATE") ?? "true";

    connectionString = $"Host={pghost};Port={port};Database={pgdatabase};Username={user};Password={pass};SSL Mode={sslMode};Trust Server Certificate={trustCert}";
}
else
{
    // Fallback: Check for URL-based environment variables
    var rawUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? Environment.GetEnvironmentVariable("DATABASE_PRIVATE_URL")
        ?? Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL")
        ?? "Host=localhost;Port=5432;Database=persondb;Username=postgres;Password=postgres;SSL Mode=Prefer;Trust Server Certificate=true";

    connectionString = ParseConnectionString(rawUrl);
}

var builder = WebApplication.CreateBuilder(args);

// Support Railway & dynamic container PORT environment variable
var webPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(webPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{webPort}");
}

// Add services to the container
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// 3. Register DbContext with PostgreSQL connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSingleton<ChatHistoryService>();
builder.Services.AddScoped<PersonService>();

var app = builder.Build();

// Ensure PostgreSQL database & tables are created on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not automatically initialize PostgreSQL database. Ensure PostgreSQL server is running and accessible.");
    }
}

app.UseForwardedHeaders();

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");

app.Run();

static string ParseConnectionString(string connStr)
{
    if (connStr.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
        connStr.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        var uri = new Uri(connStr);
        var userInfo = uri.UserInfo.Split(':');
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        var npgsqlBuilder = new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Database = database,
            Username = username,
            Password = password,
            SslMode = Npgsql.SslMode.Prefer
        };
        return npgsqlBuilder.ConnectionString;
    }

    return connStr;
}
