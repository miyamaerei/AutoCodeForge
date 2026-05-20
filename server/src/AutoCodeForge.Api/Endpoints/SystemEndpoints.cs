using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers system information endpoints for monitoring and debugging.
/// </summary>
public static class SystemEndpoints
{
    /// <summary>
    /// Maps system endpoints.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/system/info", GetSystemInfo).AllowAnonymous();
        app.MapGet("/system/environment", GetEnvironmentInfo).AllowAnonymous();
        return app;
    }

    private static IResult GetSystemInfo(CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var assembly = typeof(SystemEndpoints).Assembly;
        var version = assembly.GetName().Version?.ToString() ?? "unknown";
        var fileVersion = System.Reflection.Assembly.GetExecutingAssembly()
            .GetCustomAttributes(typeof(System.Reflection.AssemblyFileVersionAttribute), false)
            .FirstOrDefault() as System.Reflection.AssemblyFileVersionAttribute;

        return Results.Ok(new
        {
            application = "AutoCodeForge",
            version = fileVersion?.Version ?? version,
            environment = GetEnvironment(),
            timestamp = DateTime.UtcNow,
            runtime = new
            {
                framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                osDescription = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                processorCount = Environment.ProcessorCount,
            },
        });
    }

    private static IResult GetEnvironmentInfo(CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var env = GetEnvironment();
        var uptime = GetProcessUptime();

        return Results.Ok(new
        {
            environment = env,
            timestamp = DateTime.UtcNow,
            process = new
            {
                id = Environment.ProcessId,
                uptime = uptime,
                workingSet = GC.GetTotalMemory(false) / 1024 / 1024 + " MB",
            },
            dotnet = new
            {
                version = Environment.Version.ToString(),
                runtimeVersion = System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier,
            },
        });
    }

    private static string GetEnvironment()
    {
        var aspNetCoreEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        return aspNetCoreEnv;
    }

    private static string GetProcessUptime()
    {
        var uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
        var days = uptime.Days;
        var hours = uptime.Hours;
        var minutes = uptime.Minutes;
        var seconds = uptime.Seconds;

        if (days > 0)
        {
            return $"{days}d {hours}h {minutes}m";
        }

        if (hours > 0)
        {
            return $"{hours}h {minutes}m {seconds}s";
        }

        if (minutes > 0)
        {
            return $"{minutes}m {seconds}s";
        }

        return $"{seconds}s";
    }
}
