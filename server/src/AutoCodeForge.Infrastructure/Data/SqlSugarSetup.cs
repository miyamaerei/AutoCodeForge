using AutoCodeForge.Core.Entities.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Data;

/// <summary>
/// Registers and configures the shared SqlSugar client.
/// </summary>
public static class SqlSugarSetup
{
    /// <summary>
    /// Adds the SqlSugar client and global soft-delete filter to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSqlSugar(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=autocodeforge.db";

        var db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        db.QueryFilter.AddTableFilter<UserOwnedEntity>(entity => !entity.IsDeleted);

        services.AddSingleton<ISqlSugarClient>(db);
        return services;
    }
}