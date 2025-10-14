using System.Text;
using GAG_Proc_Generator.Constants;
using GAG_Proc_Generator.Models;

namespace GAG_Proc_Generator.Services;

public class SqlGenerationService
{
    public string GenerateGetAllProcedure(string tableName, string database, IEnumerable<ColumnInfo> columns, IEnumerable<ForeignKeyInfo> selectedForeignKeys, IEnumerable<TableJoinInfo> additionalJoins, string initials, string storyNumber, string description, string application)
    {
        var tableAlias = GetTableAlias(tableName);
        var selectList = string.Join(",\n        ", columns.Select(c => $"{tableAlias}.{c.Name}"));
        var joinClauses = new List<string>();
        var tableAliases = new Dictionary<string, string> { { tableName, tableAlias } };

        foreach (var fk in selectedForeignKeys)
        {
            var fkAlias = GetTableAlias(fk.ReferencedTable);
            if (!tableAliases.ContainsKey(fk.ReferencedTable))
            {
                tableAliases[fk.ReferencedTable] = fkAlias;
            }

            joinClauses.Add($"    JOIN {database}.{fk.ReferencedTable} AS {fkAlias} ON {fkAlias}.{fk.ReferencedColumn} = {tableAlias}.{fk.ColumnName}");
            selectList += $",\n        {fkAlias}.Label AS {fk.ReferencedTable}";

            if (fk.ReferencedTable == "GeographyElement")
            {
                const string gtAlias = "GT";
                joinClauses.Add($"    JOIN {database}.GeographyType AS {gtAlias} ON {gtAlias}.GeographyTypeID = {fkAlias}.GeographyTypeID");
                selectList += $",\n        {fkAlias}.GeographyTypeID,\n        {gtAlias}.Label AS GeographyType";
            }
        }

        foreach (var join in additionalJoins.Where(j => j.IsSelected))
        {
            var baseAlias = GetTableAlias(join.JoinTable);
            var joinAlias = baseAlias;

            if (tableAliases.ContainsValue(baseAlias))
            {
                int counter = 1;
                while (tableAliases.ContainsValue($"{baseAlias}{counter}"))
                {
                    counter++;
                }
                joinAlias = $"{baseAlias}{counter}";
            }

            tableAliases[$"{join.JoinTable}_{joinAlias}"] = joinAlias;

            joinClauses.Add($"    {join.JoinType} {database}.{join.JoinTable} AS {joinAlias} ON {joinAlias}.{join.JoinColumn} = {tableAlias}.{join.BaseTableColumn}");
            selectList += $",\n        {joinAlias}.*";
        }

        var fromClause = $"{database}.{tableName} AS {tableAlias}";
        if (joinClauses.Any())
        {
            fromClause += "\n" + string.Join("\n", joinClauses);
        }

        var header = BuildProcedureHeader("GetAll", $"{tableName}{DatabaseConstants.GetAllProcedureSuffix}", $"Get all {tableName} records", database, application, initials, storyNumber, description, null);
        return $"USE [{database}];\nGO\n\nCREATE OR ALTER PROCEDURE [{database}].[{DatabaseConstants.ProcedurePrefix}{tableName}{DatabaseConstants.GetAllProcedureSuffix}]\nAS\n{header}\nBEGIN\n    SELECT\n        {selectList}\n    FROM {fromClause}\nEND\nGO";
    }

    public string GenerateGetByAttributesProcedure(string tableName, string database, IEnumerable<ColumnInfo> allColumns, IEnumerable<ColumnInfo> selectedColumns, IEnumerable<ForeignKeyInfo> selectedForeignKeys, IEnumerable<TableJoinInfo> additionalJoins, string initials, string storyNumber, string description, string application)
    {
        var selectedCols = selectedColumns.ToList();
        if (!selectedCols.Any())
        {
            return string.Empty;
        }

        var tableAlias = GetTableAlias(tableName);
        var selectList = string.Join(",\n        ", allColumns.Select(c => $"{tableAlias}.{c.Name}"));
        var joinClauses = new List<string>();
        var tableAliases = new Dictionary<string, string> { { tableName, tableAlias } };

        foreach (var fk in selectedForeignKeys)
        {
            var fkAlias = GetTableAlias(fk.ReferencedTable);
            if (!tableAliases.ContainsKey(fk.ReferencedTable))
            {
                tableAliases[fk.ReferencedTable] = fkAlias;
            }

            joinClauses.Add($"    JOIN {database}.{fk.ReferencedTable} AS {fkAlias} ON {fkAlias}.{fk.ReferencedColumn} = {tableAlias}.{fk.ColumnName}");
            selectList += $",\n        {fkAlias}.Label AS {fk.ReferencedTable}";

            if (fk.ReferencedTable == "GeographyElement")
            {
                const string gtAlias = "GT";
                joinClauses.Add($"    JOIN {database}.GeographyType AS {gtAlias} ON {gtAlias}.GeographyTypeID = {fkAlias}.GeographyTypeID");
                selectList += $",\n        {fkAlias}.GeographyTypeID,\n        {gtAlias}.Label AS GeographyType";
            }
        }

        foreach (var join in additionalJoins.Where(j => j.IsSelected))
        {
            var baseAlias = GetTableAlias(join.JoinTable);
            var joinAlias = baseAlias;

            if (tableAliases.ContainsValue(baseAlias))
            {
                int counter = 1;
                while (tableAliases.ContainsValue($"{baseAlias}{counter}"))
                {
                    counter++;
                }
                joinAlias = $"{baseAlias}{counter}";
            }

            tableAliases[$"{join.JoinTable}_{joinAlias}"] = joinAlias;

            joinClauses.Add($"    {join.JoinType} {database}.{join.JoinTable} AS {joinAlias} ON {joinAlias}.{join.JoinColumn} = {tableAlias}.{join.BaseTableColumn}");
            selectList += $",\n        {joinAlias}.*";
        }

        var fromClause = $"{database}.{tableName} AS {tableAlias}";
        if (joinClauses.Any())
        {
            fromClause += "\n" + string.Join("\n", joinClauses);
        }

        var whereClause = string.Join(" AND\n        ", selectedCols.Select(c => $"{tableAlias}.[{c.Name}] = @{c.Name}"));
        var paramDeclarations = string.Join(",\n    ", selectedCols.Select(c => $"@{c.Name} {MapDataType(c)}"));

        var procedureSuffix = selectedCols.Count == 1 ? $"_GetBy{selectedCols[0].Name}" : DatabaseConstants.GetByAttributesProcedureSuffix;
        var procedureName = $"{tableName}{procedureSuffix}";

        var header = BuildProcedureHeader( "GetByAttributes", procedureName, $"Get {tableName} records by {string.Join(", ", selectedCols.Select(c => c.Name))}", database, application, initials, storyNumber, description, selectedCols);
        return $"USE [{database}];\nGO\n\nCREATE OR ALTER PROCEDURE [{database}].[{DatabaseConstants.ProcedurePrefix}{procedureName}]\n    {paramDeclarations}\nAS\n{header}\nBEGIN\n    SELECT\n        {selectList}\n    FROM {fromClause}\n    WHERE {whereClause}\nEND\nGO";
    }

    public string GenerateDeleteProcedure(string tableName, string database, IEnumerable<ColumnInfo> selectedColumns, string initials, string storyNumber, string description, string application)
    {
        var selectedCols = selectedColumns.ToList();
        if (!selectedCols.Any())
        {
            return string.Empty;
        }

        var whereClause = string.Join(" AND\n        ", selectedCols.Select(c => $"[{c.Name}] = @{c.Name}"));
        var paramDeclarations = string.Join(",\n    ", selectedCols.Select(c => $"@{c.Name} {MapDataType(c)}"));

        var header = BuildProcedureHeader( "Delete", $"{tableName}{DatabaseConstants.DeleteProcedureSuffix}", $"Delete {tableName} records", database, application, initials, storyNumber, description, selectedCols);
        return $"USE [{database}];\nGO\n\nCREATE OR ALTER PROCEDURE [{database}].[{DatabaseConstants.ProcedurePrefix}{tableName}{DatabaseConstants.DeleteProcedureSuffix}]\n    {paramDeclarations}\nAS\n{header}\nBEGIN\n    DELETE FROM [{database}].[{tableName}]\n    WHERE {whereClause}\nEND\nGO";
    }

    public string GenerateSaveProcedure(string tableName, string database, IEnumerable<ColumnInfo> allColumns, IEnumerable<ColumnInfo> selectedColumns, string initials, string storyNumber, string description, string application)
    {
        var selectedCols = selectedColumns.ToList();
        if (!selectedCols.Any())
        {
            return string.Empty;
        }

        var pkCols = allColumns.Where(c => c.IsPrimaryKey).ToList();
        var nonIdentityCols = selectedCols.Where(c => !c.IsIdentity).ToList();

        // Parameters include ALL selected columns (including identity for UPDATE)
        var paramDeclarations = string.Join(",\n    ", selectedCols.Select(c => $"@{c.Name} {MapDataType(c)}"));

        // INSERT excludes identity columns
        var insertColumns = string.Join(", ", nonIdentityCols.Select(c => $"[{c.Name}]"));
        var insertValues = string.Join(", ", nonIdentityCols.Select(c => $"@{c.Name}"));

        // UPDATE excludes identity columns
        var updateSet = string.Join(",\n            ", nonIdentityCols.Select(c => $"[{c.Name}] = @{c.Name}"));

        // WHERE uses primary key (which may include identity)
        var wherePk = string.Join(" AND ", pkCols.Select(c => $"[{c.Name}] = @{c.Name}"));

        var header = BuildProcedureHeader( "Save", $"{tableName}{DatabaseConstants.SaveProcedureSuffix}", $"Save a {tableName} record", database, application, initials, storyNumber, description, selectedCols);

        if (pkCols.Any())
        {
            return $"USE [{database}];\nGO\n\nCREATE OR ALTER PROCEDURE [{database}].[{DatabaseConstants.ProcedurePrefix}{tableName}{DatabaseConstants.SaveProcedureSuffix}]\n    {paramDeclarations}\nAS\n{header}\nBEGIN\n    IF EXISTS (SELECT 1 FROM [{database}].[{tableName}] WHERE {wherePk})\n    BEGIN\n        UPDATE [{database}].[{tableName}]\n        SET\n            {updateSet}\n        WHERE {wherePk}\n    END\n    ELSE\n    BEGIN\n        INSERT INTO [{database}].[{tableName}] ({insertColumns})\n        VALUES ({insertValues})\n    END\nEND\nGO";
        }

        return $"USE [{database}];\nGO\n\nCREATE OR ALTER PROCEDURE [{database}].[{DatabaseConstants.ProcedurePrefix}{tableName}{DatabaseConstants.SaveProcedureSuffix}]\n    {paramDeclarations}\nAS\n{header}\nBEGIN\n    INSERT INTO [{database}].[{tableName}] ({insertColumns})\n    VALUES ({insertValues})\nEND\nGO";
    }

    public string GenerateInsertProcedure(string tableName, string database, IEnumerable<ColumnInfo> selectedColumns, string initials, string storyNumber, string description, string application)
    {
        var selectedCols = selectedColumns.Where(c => !c.IsIdentity).ToList();
        if (!selectedCols.Any())
        {
            return string.Empty;
        }

        var paramDeclarations = string.Join(",\n    ", selectedCols.Select(c => $"@{c.Name} {MapDataType(c)}"));
        var insertColumns = string.Join(", ", selectedCols.Select(c => $"[{c.Name}]"));
        var insertValues = string.Join(", ", selectedCols.Select(c => $"@{c.Name}"));

        var header = BuildProcedureHeader("Insert", $"{tableName}{DatabaseConstants.InsertProcedureSuffix}", $"Insert a {tableName} record", database, application, initials, storyNumber, description, selectedCols);
        return $"USE [{database}];\nGO\n\nCREATE OR ALTER PROCEDURE [{database}].[{DatabaseConstants.ProcedurePrefix}{tableName}{DatabaseConstants.InsertProcedureSuffix}]\n    {paramDeclarations}\nAS\n{header}\nBEGIN\n    INSERT INTO [{database}].[{tableName}] ({insertColumns})\n    VALUES ({insertValues})\nEND\nGO";
    }

    private string BuildProcedureHeader(string procedureType, string procedureName, string description, string database, string application, string initials, string storyNumber, string headerDescription, List<ColumnInfo>? paramColumns)
    {
        var fullProcedureName = $"{database}.{DatabaseConstants.ProcedurePrefix}{procedureName}";
        var testString = new StringBuilder();

        if (paramColumns != null && paramColumns.Any())
        {
            foreach (var column in paramColumns)
            {
                testString.AppendLine($"DECLARE @{column.Name} {MapDataType(column)} = {GetDefaultValue(column.DataType)};");
            }
            testString.Append($"EXEC {fullProcedureName}\n    ");
            testString.Append(string.Join(",\n    ", paramColumns.Select(c => $"@{c.Name} = @{c.Name}")));
            testString.Append(';');
        }
        else
        {
            testString.Append($"EXEC {fullProcedureName};");
        }

        return $@"/****************************************************************************************
PROCEDURE:   {fullProcedureName}
DESCRIPTION: {description}
APPLICATION: {application}
DATABASE:    {database}

TEST STRING:

{testString}

*******************************************************************************
REVISION HISTORY:
Initials        Date            Story Number    Description
=========================================================================================
{initials.PadRight(15)} {DateTime.Now:MM/dd/yyyy}      {storyNumber.PadRight(15)} {headerDescription}
*****************************************************************************************/";
    }

    private static string GetTableAlias(string tableName)
    {
        if (string.IsNullOrEmpty(tableName))
            return "T";

        var alias = new StringBuilder();

        // Add first character
        alias.Append(char.ToUpper(tableName[0]));

        // Add first letter of each uppercase letter (indicating a new word in PascalCase)
        for (int i = 1; i < tableName.Length; i++)
        {
            if (char.IsUpper(tableName[i]))
            {
                alias.Append(tableName[i]);
            }
        }

        // If we got a good multi-letter alias, use it
        if (alias.Length >= 2)
            return alias.ToString();

        // Fallback to first 2 characters if no uppercase letters found (e.g., "users" -> "US")
        return tableName.Length >= 2 ? tableName[..2].ToUpper() : tableName.ToUpper();
    }

    private static string MapDataType(ColumnInfo column)
    {
        var baseType = column.DataType.ToUpper();

        return baseType switch
        {
            "VARCHAR" or "NVARCHAR" or "CHAR" or "NCHAR" => column.MaxLength.HasValue && column.MaxLength.Value > 0
                ? $"{baseType}({(column.MaxLength.Value == -1 ? "MAX" : column.MaxLength.Value.ToString())})"
                : $"{baseType}(MAX)",
            "VARBINARY" or "BINARY" => column.MaxLength.HasValue && column.MaxLength.Value > 0
                ? $"{baseType}({(column.MaxLength.Value == -1 ? "MAX" : column.MaxLength.Value.ToString())})"
                : $"{baseType}(MAX)",
            "INT" => "INT",
            "BIGINT" => "BIGINT",
            "SMALLINT" => "SMALLINT",
            "TINYINT" => "TINYINT",
            "BIT" => "BIT",
            "DECIMAL" => "DECIMAL",
            "NUMERIC" => "DECIMAL",
            "MONEY" => "MONEY",
            "SMALLMONEY" => "SMALLMONEY",
            "FLOAT" => "FLOAT",
            "REAL" => "REAL",
            "DATETIME" => "DATETIME",
            "DATETIME2" => "DATETIME2",
            "SMALLDATETIME" => "SMALLDATETIME",
            "DATE" => "DATE",
            "TIME" => "TIME",
            "DATETIMEOFFSET" => "DATETIMEOFFSET",
            "TEXT" => "TEXT",
            "NTEXT" => "NTEXT",
            "IMAGE" => "IMAGE",
            "UNIQUEIDENTIFIER" => "UNIQUEIDENTIFIER",
            "XML" => "XML",
            _ => "NVARCHAR(MAX)"
        };
    }

    private static string MapDataType(string dataType)
    {
        return dataType.ToUpper() switch
        {
            "INT" => "INT",
            "BIGINT" => "BIGINT",
            "SMALLINT" => "SMALLINT",
            "TINYINT" => "TINYINT",
            "BIT" => "BIT",
            "DECIMAL" => "DECIMAL",
            "NUMERIC" => "DECIMAL",
            "MONEY" => "MONEY",
            "SMALLMONEY" => "SMALLMONEY",
            "FLOAT" => "FLOAT",
            "REAL" => "REAL",
            "DATETIME" => "DATETIME",
            "DATETIME2" => "DATETIME2",
            "SMALLDATETIME" => "SMALLDATETIME",
            "DATE" => "DATE",
            "TIME" => "TIME",
            "DATETIMEOFFSET" => "DATETIMEOFFSET",
            "CHAR" => "CHAR",
            "NCHAR" => "NCHAR",
            "VARCHAR" => "VARCHAR",
            "NVARCHAR" => "NVARCHAR",
            "TEXT" => "TEXT",
            "NTEXT" => "NTEXT",
            "BINARY" => "BINARY",
            "VARBINARY" => "VARBINARY",
            "IMAGE" => "IMAGE",
            "UNIQUEIDENTIFIER" => "UNIQUEIDENTIFIER",
            "XML" => "XML",
            _ => "NVARCHAR(MAX)"
        };
    }

    private static string GetDefaultValue(string dataType)
    {
        return dataType.ToUpper() switch
        {
            "INT" or "BIGINT" or "SMALLINT" or "TINYINT" => "0",
            "BIT" => "0",
            "DECIMAL" or "NUMERIC" or "MONEY" or "SMALLMONEY" => "0.00",
            "FLOAT" or "REAL" => "0.0",
            "DATETIME" or "DATETIME2" or "SMALLDATETIME" or "DATE" => "'2023-01-01'",
            "TIME" => "'00:00:00'",
            "DATETIMEOFFSET" => "'2023-01-01 00:00:00+00:00'",
            "CHAR" or "NCHAR" or "VARCHAR" or "NVARCHAR" => "'test'",
            "TEXT" or "NTEXT" => "'test'",
            "BINARY" or "VARBINARY" or "IMAGE" => "0x00",
            "UNIQUEIDENTIFIER" => "'00000000-0000-0000-0000-000000000000'",
            "XML" => "'<xml></xml>'",
            _ => "'default'"
        };
    }
}
