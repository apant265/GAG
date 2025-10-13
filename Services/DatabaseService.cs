using System.Data;
using GAG_Proc_Generator.Constants;
using GAG_Proc_Generator.Models;
using Microsoft.Data.SqlClient;

namespace GAG_Proc_Generator.Services;

public class DatabaseService
{
    public async Task<List<string>> GetTablesAsync(string connectionString)
    {
        var tables = new List<string>();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(SqlQueries.GetTables, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    public async Task<List<string>> GetDatabasesAsync(string connectionString)
    {
        var databases = new List<string>();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(SqlQueries.GetDatabases, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            databases.Add(reader.GetString(0));
        }

        return databases;
    }

    public async Task<List<string>> GetProceduresAsync(string connectionString)
    {
        var procedures = new List<string>();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(SqlQueries.GetProcedures, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            procedures.Add(reader.GetString(0));
        }

        return procedures;
    }

    public async Task<List<ColumnInfo>> GetColumnsAsync(string connectionString, string tableName)
    {
        var columns = new List<ColumnInfo>();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var schema = await Task.Run(() => connection.GetSchema("Columns", new[] { null, null, tableName }));
        var primaryKeys = await GetPrimaryKeysAsync(connection, tableName);

        foreach (DataRow row in schema.Rows)
        {
            var column = new ColumnInfo
            {
                Name = row["COLUMN_NAME"].ToString()!,
                DataType = row["DATA_TYPE"].ToString()!,
                IsNullable = row["IS_NULLABLE"].ToString() == "YES",
                IsPrimaryKey = primaryKeys.Contains(row["COLUMN_NAME"].ToString()!)
            };
            columns.Add(column);
        }

        return columns;
    }

    public async Task<List<string>> GetPrimaryKeysAsync(SqlConnection connection, string tableName)
    {
        var keys = new List<string>();

        await using var command = new SqlCommand(SqlQueries.GetPrimaryKeys, connection);
        command.Parameters.AddWithValue("@TableName", tableName);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            keys.Add(reader.GetString(0));
        }

        return keys;
    }

    public async Task<List<ForeignKeyInfo>> GetForeignKeysAsync(string connectionString, string tableName)
    {
        var foreignKeys = new List<ForeignKeyInfo>();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(SqlQueries.GetForeignKeys, connection);
        command.Parameters.AddWithValue("@TableName", tableName);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            foreignKeys.Add(new ForeignKeyInfo
            {
                ForeignKeyName = reader.GetString(0),
                ColumnName = reader.GetString(1),
                ReferencedTable = reader.GetString(2),
                ReferencedColumn = reader.GetString(3)
            });
        }

        return foreignKeys;
    }

    public async Task<bool> TestConnectionAsync(string connectionString)
    {
        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
