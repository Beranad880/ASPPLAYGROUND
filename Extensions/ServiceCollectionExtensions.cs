using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebApplicationASP01.App;

namespace WebApplicationASP01.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomPostgres(this IServiceCollection services)
    {
        var pghost = Environment.GetEnvironmentVariable("PGHOST");
        var pgdatabase = Environment.GetEnvironmentVariable("PGDATABASE");
        string connectionString;

        if (!string.IsNullOrEmpty(pghost) && !string.IsNullOrEmpty(pgdatabase))
        {
            var port = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
            var user = Environment.GetEnvironmentVariable("PGUSER") ?? "postgres";
            var pass = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "";
            var sslMode = Environment.GetEnvironmentVariable("PGSSLMODE")
                ?? (pghost is "localhost" or "127.0.0.1" ? "Prefer" : "Require");
            var trustCert = Environment.GetEnvironmentVariable("PGTRUSTSERVERCERTIFICATE") ?? "true";

            connectionString = $"Host={pghost};Port={port};Database={pgdatabase};Username={user};Password={pass};SSL Mode={sslMode};Trust Server Certificate={trustCert}";
        }
        else
        {
            var rawUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
                ?? Environment.GetEnvironmentVariable("DATABASE_PRIVATE_URL")
                ?? Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL")
                ?? "Host=localhost;Port=5432;Database=persondb;Username=postgres;Password=postgres;SSL Mode=Prefer;Trust Server Certificate=true";

            connectionString = ParsePostgresConnectionString(rawUrl);
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(2),
                    errorCodesToAdd: null);
            }));
        return services;
    }

    private static string ParsePostgresConnectionString(string connStr)
    {
        if (connStr.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            connStr.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(connStr);
            var userInfo = uri.UserInfo.Split(':');
            var username = Uri.UnescapeDataString(userInfo[0]);
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
            
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = uri.Host,
                Port = uri.Port > 0 ? uri.Port : 5432,
                Database = uri.AbsolutePath.TrimStart('/'),
                Username = username,
                Password = password,
                SslMode = SslMode.Prefer
            };
            return builder.ConnectionString;
        }
        return connStr;
    }
}
