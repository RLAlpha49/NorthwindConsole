using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using Microsoft.EntityFrameworkCore;
using NLog;
using NorthwindConsole.Model;

namespace NorthwindConsole
{
    public static class DatabaseActions
    {
        public static void ShowDatabaseMenu(Logger logger)
        {
            var db = new DataContext();

            do
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("========================================");
                Console.WriteLine("      Database Administration Menu");
                Console.WriteLine("========================================");
                Console.WriteLine(" 1) View Database Tables");
                Console.WriteLine(" 2) View Table Schema");
                Console.WriteLine(" 3) Execute Custom SQL Query");
                Console.WriteLine(" 4) Execute Data Modification Command");
                Console.WriteLine(" [Enter] Return to Main Menu");
                Console.WriteLine("----------------------------------------");
                Console.ForegroundColor = ConsoleColor.White;

                string? choice = Console.ReadLine();
                Console.Clear();
                logger.Info("Database Option {choice} selected", choice);

                switch (choice)
                {
                    case "1":
                        ViewAllTables(db, logger);
                        break;
                    case "2":
                        ViewTableSchema(db, logger);
                        break;
                    case "3":
                        ExecuteCustomSql(db, logger);
                        break;
                    case "4":
                        ExecuteDataModificationCommand(db, logger);
                        break;
                    case null:
                    case "":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
                Console.Clear();
            } while (true);
        }

        private static void ViewAllTables(DataContext db, Logger logger)
        {
            Console.WriteLine("Database Tables:");
            Console.WriteLine("----------------------------------------");

            // Get all DbSet properties from the context
            var tableProperties = typeof(DataContext)
                .GetProperties()
                .Where(p =>
                    p.PropertyType.IsGenericType
                    && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
                )
                .ToList();

            foreach (var property in tableProperties)
            {
                string tableName = property.Name;
                Type entityType = property.PropertyType.GetGenericArguments()[0];

                // Use EF Core to get the actual table name if it's different from the property name
                var entityType2 = db.Model.FindEntityType(entityType);
                string actualTableName = entityType2?.GetTableName() ?? tableName;

                Console.WriteLine($"• {tableName} ({actualTableName})");
            }

            logger.Info("Displayed all database tables");
        }

        private static void ViewTableSchema(DataContext db, Logger logger)
        {
            Console.WriteLine("Select a table to view its schema:");
            var tables = typeof(DataContext)
                .GetProperties()
                .Where(p =>
                    p.PropertyType.IsGenericType
                    && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
                )
                .ToList();

            for (int i = 0; i < tables.Count; i++)
            {
                Console.WriteLine($"  {i + 1}) {tables[i].Name}");
            }

            Console.Write("\nEnter table number: ");
            if (
                int.TryParse(Console.ReadLine(), out int tableIndex)
                && tableIndex > 0
                && tableIndex <= tables.Count
            )
            {
                var selectedProperty = tables[tableIndex - 1];
                Type entityType = selectedProperty.PropertyType.GetGenericArguments()[0];

                Console.WriteLine($"\nSchema for {selectedProperty.Name}:");
                Console.WriteLine("----------------------------------------");

                // Get all properties of the entity type
                var properties = entityType.GetProperties();
                foreach (var property in properties)
                {
                    string dataType = property.PropertyType.Name;
                    if (
                        property.PropertyType.IsGenericType
                        && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                    )
                    {
                        dataType = $"{Nullable.GetUnderlyingType(property.PropertyType).Name}?";
                    }

                    // Check for key attribute
                    bool isKey = property.GetCustomAttributes(typeof(KeyAttribute), false).Any();

                    // Check for column attributes
                    var columnAttr =
                        property
                            .GetCustomAttributes(typeof(ColumnAttribute), false)
                            .FirstOrDefault() as ColumnAttribute;
                    string columnName = columnAttr?.Name ?? property.Name;

                    string keyMarker = isKey ? "(PK) " : "";
                    Console.WriteLine($"• {keyMarker}{property.Name} ({dataType})");
                }

                logger.Info("Displayed schema for table {TableName}", selectedProperty.Name);
            }
            else
            {
                Console.WriteLine("Invalid selection.");
            }
        }

        private static void ExecuteCustomSql(DataContext db, Logger logger)
        {
            Console.WriteLine("Enter SQL query to execute (read-only queries supported):");
            string? sql = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(sql))
            {
                Console.WriteLine("Query cannot be empty.");
                return;
            }

            try
            {
                // Only allow SELECT queries for safety
                sql = sql.Trim();
                if (!sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Only SELECT queries are allowed for safety reasons.");
                    return;
                }

                // Use ADO.NET directly for more flexibility with raw SQL
                using (var command = db.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    // Ensure connection is open
                    if (db.Database.GetDbConnection().State != ConnectionState.Open)
                    {
                        db.Database.GetDbConnection().Open();
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        // Display column names
                        Console.WriteLine("\nQuery Results:");
                        Console.WriteLine("----------------------------------------");

                        if (!reader.HasRows)
                        {
                            Console.WriteLine("No results returned.");
                            return;
                        }

                        // Get column names
                        var fieldCount = reader.FieldCount;
                        var columnNames = new string[fieldCount];
                        for (int i = 0; i < fieldCount; i++)
                        {
                            columnNames[i] = reader.GetName(i);
                            Console.Write($"{columnNames[i], -20} ");
                        }
                        Console.WriteLine("\n----------------------------------------");

                        // Display data rows
                        int rowCount = 0;
                        while (reader.Read())
                        {
                            rowCount++;
                            for (int i = 0; i < fieldCount; i++)
                            {
                                string value = reader.IsDBNull(i)
                                    ? "NULL"
                                    : reader.GetValue(i).ToString() ?? "";
                                Console.Write($"{value, -20} ");
                            }
                            Console.WriteLine();

                            // Limit display to a reasonable number of rows
                            if (rowCount >= 100)
                            {
                                Console.WriteLine(
                                    "\n... more rows exist but display limited to 100 rows"
                                );
                                break;
                            }
                        }

                        Console.WriteLine($"\n{rowCount} row(s) displayed.");
                    }
                }

                logger.Info("Executed custom SQL query: {Sql}", sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing query: {ex.Message}");
                logger.Error(ex, "Error executing SQL query");
            }
        }

        private static void ExecuteDataModificationCommand(DataContext db, Logger logger)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("!!! WARNING !!!");
            Console.WriteLine("You are about to execute a command that can modify database data.");
            Console.WriteLine("This can potentially delete or corrupt data if used incorrectly.");
            Console.WriteLine(
                "Ensure you have a backup before proceeding with destructive operations."
            );
            Console.WriteLine("----------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(
                "Enter SQL command to execute (INSERT, UPDATE, DELETE, CREATE, ALTER, DROP supported):"
            );
            string? sql = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(sql))
            {
                Console.WriteLine("Command cannot be empty.");
                return;
            }

            Console.WriteLine(
                "\nAre you sure you want to execute this command? (Type 'YES' to confirm)"
            );
            string? confirmation = Console.ReadLine();

            if (confirmation != "YES")
            {
                Console.WriteLine("Command execution cancelled.");
                return;
            }

            try
            {
                // Trim the SQL command and check its type
                sql = sql.Trim();

                // Disallow certain potentially dangerous operations
                if (
                    sql.Contains("DROP DATABASE", StringComparison.OrdinalIgnoreCase)
                    || sql.Contains("TRUNCATE TABLE", StringComparison.OrdinalIgnoreCase)
                    || sql.Contains("sys.", StringComparison.OrdinalIgnoreCase)
                    || sql.Contains("xp_", StringComparison.OrdinalIgnoreCase)
                )
                {
                    Console.WriteLine(
                        "This command contains potentially harmful operations and is not allowed."
                    );
                    logger.Warn("Potentially harmful SQL command blocked: {Sql}", sql);
                    return;
                }

                using (var command = db.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    // Ensure connection is open
                    if (db.Database.GetDbConnection().State != ConnectionState.Open)
                    {
                        db.Database.GetDbConnection().Open();
                    }

                    // Execute the command and get affected rows
                    int affectedRows = command.ExecuteNonQuery();
                    Console.WriteLine(
                        $"\nCommand executed successfully. {affectedRows} row(s) affected."
                    );

                    logger.Info("Executed SQL command: {Sql}", sql);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing command: {ex.Message}");
                logger.Error(ex, "Error executing SQL command");
            }
        }
    }
}
