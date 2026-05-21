using Microsoft.Data.Sqlite;

namespace AutoCodeForge.Debug;

class Program
{
    static async Task Main(string[] args)
    {
        var dbPath = @"E:\git\AutoFrog\AutoCodeForge\server\src\AutoCodeForge.Api\autocodeforge.dev.db";

        Console.WriteLine("=== Database Diagnostic Tool ===\n");

        if (!File.Exists(dbPath))
        {
            Console.WriteLine($"❌ Database file not found: {dbPath}");
            Console.WriteLine("\nPossible solutions:");
            Console.WriteLine("1. Check if the database file exists");
            Console.WriteLine("2. Verify the connection string in appsettings.json");
            Console.WriteLine("3. Run database migrations");
            return;
        }

        Console.WriteLine($"✅ Database file found: {dbPath}");
        Console.WriteLine($"   Size: {new FileInfo(dbPath).Length} bytes\n");

        var connectionString = $"Data Source={dbPath}";

        try
        {
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            Console.WriteLine("✅ Database connection successful\n");

            // Get all tables
            var tables = new List<string>();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name";
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }

            Console.WriteLine($"📋 Found {tables.Count} tables:\n");
            foreach (var table in tables)
            {
                Console.WriteLine($"  - {table}");
            }

            // Check for ConfigurationEntry table
            Console.WriteLine("\n🔍 Checking ConfigurationEntry table...\n");
            if (tables.Contains("configuration_entries", StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine("✅ Table 'configuration_entries' exists\n");

                // Get table schema
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "PRAGMA table_info(configuration_entries)";
                    using var reader = await cmd.ExecuteReaderAsync();

                    Console.WriteLine("Schema:");
                    var columns = new List<string>();
                    while (await reader.ReadAsync())
                    {
                        var name = reader.GetString(1);
                        var type = reader.GetString(2);
                        var notNull = reader.GetInt32(3) == 1 ? "NOT NULL" : "NULL";
                        var pk = reader.GetInt32(5) == 1 ? "PRIMARY KEY" : "";
                        columns.Add($"  - {name} ({type}) {notNull} {pk}");
                    }

                    foreach (var col in columns)
                    {
                        Console.WriteLine(col);
                    }
                }

                // Count records
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM configuration_entries";
                    var count = await cmd.ExecuteScalarAsync();
                    Console.WriteLine($"\n📊 Total records: {count}");
                }
            }
            else
            {
                Console.WriteLine("❌ Table 'configuration_entries' does NOT exist!");
                Console.WriteLine("\nThis is likely the cause of the 500 error.");
                Console.WriteLine("\n💡 Solutions:");
                Console.WriteLine("1. Run database migration/initialization");
                Console.WriteLine("2. Check if SqlSugar CodeFirst tables are being created");
                Console.WriteLine("3. Verify database initialization in Program.cs");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
        }
    }
}
