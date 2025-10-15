namespace GAG_Proc_Generator.Constants;

public static class DatabaseConstants
{
    public const string DefaultApplication = "GlobalValues.API";
    public const string DefaultDatabase = "GlobalValues";
    public const string DefaultOutputFolder = "GeneratedProcs";

    public const string GetByAttributesProcedureSuffix = "_GetByAttributes";
    public const string DeleteProcedureSuffix = "_Delete";
    public const string SaveProcedureSuffix = "_Save";
    public const string InsertProcedureSuffix = "_Insert";

    public const string ProcedurePrefix = "usp";
    public const string SqlFileExtension = ".sql";
}

public static class SqlQueries
{
    public const string GetPrimaryKeys =
        "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = @TableName AND CONSTRAINT_NAME LIKE 'PK_%'";

    public const string GetForeignKeys =
        @"SELECT
            fk.name AS ForeignKeyName,
            c.name AS ColumnName,
            OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
            rc.name AS ReferencedColumn
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
        INNER JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
        INNER JOIN sys.columns rc ON fkc.referenced_object_id = rc.object_id AND fkc.referenced_column_id = rc.column_id
        WHERE OBJECT_NAME(fk.parent_object_id) = @TableName";

    public const string GetTables =
        "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME";

    public const string GetDatabases =
        "SELECT name FROM sys.databases WHERE database_id > 4 ORDER BY name";
}

public static class MessageStrings
{
    public const string ConnectToDbFirst = "Please connect to database first.";
    public const string SelectColumnsForFiltering = "Please select columns for filtering.";
    public const string SelectColumnsForDeletion = "Please select columns for deletion criteria.";
    public const string SelectColumnsToSave = "Please select columns to save.";
    public const string SelectDatabase = "Please select a database.";
    public const string ConnectionTestSuccess = "Connection test successful!";

    public const string TitleWarning = "Warning";
    public const string TitleError = "Error";
    public const string TitleSuccess = "Success";
    public const string TitleInfo = "Info";
    public const string TitleFileSaved = "File Saved";
}
