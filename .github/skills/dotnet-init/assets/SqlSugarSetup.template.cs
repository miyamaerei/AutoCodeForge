using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using __ProjectName__.Entities;

namespace __ProjectName__.Extensions;

public static class SqlSugarSetup
{
    public static void AddSqlSugarSetup(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var db = new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute
        });

        services.AddSingleton<ISqlSugarClient>(db);

        // Configure AOP
        db.Aop.OnLogExecuting = (sql, pars) =>
        {
#if DEBUG
            Console.WriteLine(sql);
            Console.WriteLine(string.Join(",", pars.Select(p => $"{p.ParameterName}: {p.Value}")));
#endif
        };

        // Initialize tables using CodeFirst
        InitTables(db);
    }

    private static void InitTables(ISqlSugarClient db)
    {
        db.CodeFirst.InitTables(
            typeof(UserEntity)
            // Add other entities here as needed
        );
    }
}
