using Microsoft.EntityFrameworkCore;
using Npgsql;
using StackExchange.Redis;
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

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        return services;
    }

    public static IServiceCollection AddCustomRedis(this IServiceCollection services)
    {
        var redisHost = Environment.GetEnvironmentVariable("REDISHOST");
        ConfigurationOptions redisOptions;

        if (!string.IsNullOrEmpty(redisHost))
        {
            var port = int.TryParse(Environment.GetEnvironmentVariable("REDISPORT"), out var p) ? p : 6379;
            redisOptions = new ConfigurationOptions
            {
                EndPoints = { { redisHost, port } },
                AbortOnConnectFail = false,
                ConnectTimeout = 5000,
                SyncTimeout = 5000,
                ConnectRetry = 3
            };

            var pass = Environment.GetEnvironmentVariable("REDISPASSWORD");
            if (!string.IsNullOrEmpty(pass)) redisOptions.Password = pass;

            var user = Environment.GetEnvironmentVariable("REDISUSER");
            if (!string.IsNullOrEmpty(user) && user != "default") redisOptions.User = user;
        }
        else
        {
            var rawRedisUrl = Environment.GetEnvironmentVariable("REDIS_URL")
                ?? Environment.GetEnvironmentVariable("REDIS_PRIVATE_URL")
                ?? Environment.GetEnvironmentVariable("REDIS_PUBLIC_URL")
                ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
                ?? "localhost:6379";

            try
            {
                if (rawRedisUrl.StartsWith("redis://", StringComparison.OrdinalIgnoreCase) || 
                    rawRedisUrl.StartsWith("rediss://", StringComparison.OrdinalIgnoreCase))
                {
                    redisOptions = ParseRedisUrl(rawRedisUrl);
                }
                else
                {
                    redisOptions = ConfigurationOptions.Parse(rawRedisUrl);
                }

                redisOptions.AbortOnConnectFail = false;
                redisOptions.ConnectTimeout = 5000;
                redisOptions.SyncTimeout = 5000;
                redisOptions.ConnectRetry = 3;
            }
            catch
            {
                redisOptions = new ConfigurationOptions
                {
                    EndPoints = { { "localhost", 6379 } },
                    AbortOnConnectFail = false
                };
            }
        }

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();
            try
            {
                // Synchronní connect vrací objekt rychle, pokud AbortOnConnectFail = false a timeouty jsou krátké
                return ConnectionMultiplexer.Connect(redisOptions);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Inicializace Redis selhala. Aplikace použije in-memory zálohu.");
                return ConnectionMultiplexer.Connect(new ConfigurationOptions
                {
                    EndPoints = { { "localhost", 6379 } },
                    AbortOnConnectFail = false
                });
            }
        });

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

    private static ConfigurationOptions ParseRedisUrl(string rawUrl)
    {
        var uri = new Uri(rawUrl);
        var options = new ConfigurationOptions
        {
            EndPoints = { { uri.Host, uri.Port > 0 ? uri.Port : 6379 } },
            Ssl = rawUrl.StartsWith("rediss://", StringComparison.OrdinalIgnoreCase)
        };

        var userInfo = uri.UserInfo.Split(':');
        if (userInfo.Length > 1)
        {
            var user = Uri.UnescapeDataString(userInfo[0]);
            if (!string.IsNullOrEmpty(user) && user != "default")
            {
                options.User = user;
            }
            options.Password = Uri.UnescapeDataString(userInfo[1]);
        }
        else if (userInfo.Length == 1 && !string.IsNullOrEmpty(userInfo[0]))
        {
            options.Password = Uri.UnescapeDataString(userInfo[0]);
        }

        return options;
    }
}
