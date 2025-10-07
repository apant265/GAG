using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using GAG_Proc_Generator.Models;
using GAG_Proc_Generator.Views;
using Microsoft.Data.SqlClient;

namespace GAG_Proc_Generator.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _tableName = string.Empty;
        private ObservableCollection<ColumnInfo> _columns = new();
        private ObservableCollection<ForeignKeyInfo> _foreignKeys = new();
        private string _generatedSql = string.Empty;
        private ObservableCollection<string> _tables = new();
        private string _outputFolder = "GeneratedProcs";

        public string TableName
        {
            get => _tableName;
            set
            {
                _tableName = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ColumnInfo> Columns
        {
            get => _columns;
            set
            {
                _columns = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ForeignKeyInfo> ForeignKeys
        {
            get => _foreignKeys;
            set
            {
                _foreignKeys = value;
                OnPropertyChanged();
            }
        }

        public string GeneratedSql
        {
            get => _generatedSql;
            set
            {
                _generatedSql = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Tables
        {
            get => _tables;
            set
            {
                _tables = value;
                OnPropertyChanged();
            }
        }

        public string OutputFolder
        {
            get => _outputFolder;
            set
            {
                _outputFolder = value;
                OnPropertyChanged();
            }
        }

        private string _initials = string.Empty;
        private string _storyNumber = string.Empty;
        private string _description = string.Empty;
        private string _application = "GlobalValues.API";
        private string _database = "GlobalValues";

        public string Initials
        {
            get => _initials;
            set
            {
                _initials = value;
                OnPropertyChanged();
            }
        }

        public string StoryNumber
        {
            get => _storyNumber;
            set
            {
                _storyNumber = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public string Application
        {
            get => _application;
            set
            {
                _application = value;
                OnPropertyChanged();
            }
        }

        public string Database
        {
            get => _database;
            set
            {
                _database = value;
                OnPropertyChanged();
            }
        }

        public string ConnectionString { get; set; } = "Server=yourserver.database.windows.net;Database=YourDatabase;Authentication=Active Directory Integrated;";

        public ICommand LoadTableCommand => new RelayCommand(LoadTable);
        public ICommand GenerateGetAllCommand => new RelayCommand(GenerateGetAll);
        public ICommand GenerateGetByAttributesCommand => new RelayCommand(GenerateGetByAttributes);
        public ICommand GenerateDeleteCommand => new RelayCommand(GenerateDelete);
        public ICommand GenerateSaveCommand => new RelayCommand(GenerateSave);
        public ICommand OpenConnectionDialogCommand => new RelayCommand(OpenConnectionDialog);
        public ICommand GenerateAllProcsCommand => new RelayCommand(GenerateAllProcs);

        private void LoadTable()
        {
            if (string.IsNullOrWhiteSpace(TableName) || string.IsNullOrEmpty(ConnectionString))
            {
                System.Windows.MessageBox.Show("Please connect to database first.", "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }
            Columns.Clear();
            ForeignKeys.Clear();

            try
            {
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();

                var schema = connection.GetSchema("Columns", new[] { null, null, TableName });
                var primaryKeys = GetPrimaryKeys(connection, TableName);
                var foreignKeys = GetForeignKeys(connection, TableName);

                foreach (DataRow row in schema.Rows)
                {
                    var column = new ColumnInfo
                    {
                        Name = row["COLUMN_NAME"].ToString()!,
                        DataType = row["DATA_TYPE"].ToString()!,
                        IsNullable = row["IS_NULLABLE"].ToString() == "YES",
                        IsPrimaryKey = primaryKeys.Contains(row["COLUMN_NAME"].ToString()!)
                    };
                    Columns.Add(column);
                }

                foreach (var fk in foreignKeys)
                {
                    ForeignKeys.Add(fk);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to load table: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private List<string> GetPrimaryKeys(SqlConnection connection, string tableName)
        {
            var keys = new List<string>();
            using var command = new SqlCommand(
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = @TableName AND CONSTRAINT_NAME LIKE 'PK_%'",
                connection);
            command.Parameters.AddWithValue("@TableName", tableName);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                keys.Add(reader.GetString(0));
            }
            return keys;
        }

        private List<ForeignKeyInfo> GetForeignKeys(SqlConnection connection, string tableName)
        {
            var fks = new List<ForeignKeyInfo>();
            using var command = new SqlCommand(
                @"SELECT
                    fk.name AS ForeignKeyName,
                    c.name AS ColumnName,
                    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
                    rc.name AS ReferencedColumn
                FROM sys.foreign_keys fk
                INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                INNER JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
                INNER JOIN sys.columns rc ON fkc.referenced_object_id = rc.object_id AND fkc.referenced_column_id = rc.column_id
                WHERE OBJECT_NAME(fk.parent_object_id) = @TableName",
                connection);
            command.Parameters.AddWithValue("@TableName", tableName);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                fks.Add(new ForeignKeyInfo
                {
                    ForeignKeyName = reader.GetString(0),
                    ColumnName = reader.GetString(1),
                    ReferencedTable = reader.GetString(2),
                    ReferencedColumn = reader.GetString(3)
                });
            }
            return fks;
        }

        private void GenerateGetAll()
        {
            string sql = GenerateGetAllSql();
            GeneratedSql = sql;
            SaveToFile($"usp_{TableName}_GetAll.sql", sql);
        }

        private void GenerateGetByAttributes()
        {
            string sql = GenerateGetByAttributesSql();
            if (!string.IsNullOrEmpty(sql))
            {
                GeneratedSql = sql;
                SaveToFile($"usp_{TableName}_GetByAttributes.sql", sql);
            }
        }

        private void GenerateDelete()
        {
            string sql = GenerateDeleteSql();
            if (!string.IsNullOrEmpty(sql))
            {
                GeneratedSql = sql;
                SaveToFile($"usp_{TableName}_Delete.sql", sql);
            }
        }

        private void GenerateSave()
        {
            string sql = GenerateSaveSql();
            if (!string.IsNullOrEmpty(sql))
            {
                GeneratedSql = sql;
                SaveToFile($"usp_{TableName}_Save.sql", sql);
            }
        }

        private void GenerateAllProcs()
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = "Select folder for generated procedures";
            dialog.SelectedPath = OutputFolder;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                OutputFolder = dialog.SelectedPath;
                string dir = Path.Combine(OutputFolder, TableName);
                Directory.CreateDirectory(dir);
                string getAllSql = GenerateGetAllSql();
                File.WriteAllText(Path.Combine(dir, $"usp_{TableName}_GetAll.sql"), getAllSql);
                string getBySql = GenerateGetByAttributesSql();
                File.WriteAllText(Path.Combine(dir, $"usp_{TableName}_GetByAttributes.sql"), getBySql);
                string deleteSql = GenerateDeleteSql();
                File.WriteAllText(Path.Combine(dir, $"usp_{TableName}_Delete.sql"), deleteSql);
                string saveSql = GenerateSaveSql();
                File.WriteAllText(Path.Combine(dir, $"usp_{TableName}_Save.sql"), saveSql);
                System.Windows.MessageBox.Show("All procedures generated and saved.", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        private string GenerateGetAllSql()
        {
            var selectedFKs = ForeignKeys.Where(fk => fk.IsSelected).ToList();
            string tableAlias = TableName.Length >= 2 ? TableName.Substring(0, 2).ToUpper() : TableName.ToUpper();
            string selectList = string.Join(",\n        ", Columns.Select(c => $"{tableAlias}.{c.Name}"));
            List<string> joinClauses = new();
            Dictionary<string, string> tableAliases = new() { { TableName, tableAlias } };
            foreach (var fk in selectedFKs)
            {
                string fkAlias = fk.ReferencedTable.Length >= 2 ? fk.ReferencedTable.Substring(0, 2).ToUpper() : fk.ReferencedTable.ToUpper();
                if (!tableAliases.ContainsKey(fk.ReferencedTable)) tableAliases[fk.ReferencedTable] = fkAlias;
                joinClauses.Add($"    JOIN {Database}.{fk.ReferencedTable} AS {fkAlias} ON {fkAlias}.{fk.ReferencedColumn} = {tableAlias}.{fk.ColumnName}");
                selectList += $",\n        {fkAlias}.Label AS {fk.ReferencedTable}";
                if (fk.ReferencedTable == "GeographyElement")
                {
                    string gtAlias = "GT";
                    joinClauses.Add($"    JOIN {Database}.GeographyType AS {gtAlias} ON {gtAlias}.GeographyTypeID = {fkAlias}.GeographyTypeID");
                    selectList += $",\n        {fkAlias}.GeographyTypeID,\n        {gtAlias}.Label AS GeographyType";
                }
            }
            string fromClause = $"{Database}.{TableName} AS {tableAlias}";
            if (joinClauses.Any())
            {
                fromClause += "\n" + string.Join("\n", joinClauses);
            }
            string header = BuildHeader("GetAll", $"{TableName}_GetAll", $"Get all {TableName} records");
            string sql = $"USE [{Database}];\nGO\n\nCREATE OR ALTER PROCEDURE [{Database}].[usp{TableName}_GetAll]\nAS\n{header}\nBEGIN\n    SELECT\n        {selectList}\n    FROM {fromClause}\nEND\nGO";
            return sql;
        }

        private string GenerateGetByAttributesSql()
        {
            var selectedCols = Columns.Where(c => c.IsSelected).ToList();
            if (!selectedCols.Any())
            {
                System.Windows.MessageBox.Show("Please select columns for filtering.", "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return "";
            }
            var selectedFKs = ForeignKeys.Where(fk => fk.IsSelected).ToList();
            string tableAlias = TableName.Length >= 2 ? TableName.Substring(0, 2).ToUpper() : TableName.ToUpper();
            string selectList = string.Join(",\n        ", Columns.Select(c => $"{tableAlias}.{c.Name}"));
            List<string> joinClauses = new();
            Dictionary<string, string> tableAliases = new() { { TableName, tableAlias } };
            foreach (var fk in selectedFKs)
            {
                string fkAlias = fk.ReferencedTable.Length >= 2 ? fk.ReferencedTable.Substring(0, 2).ToUpper() : fk.ReferencedTable.ToUpper();
                if (!tableAliases.ContainsKey(fk.ReferencedTable)) tableAliases[fk.ReferencedTable] = fkAlias;
                joinClauses.Add($"    JOIN {Database}.{fk.ReferencedTable} AS {fkAlias} ON {fkAlias}.{fk.ReferencedColumn} = {tableAlias}.{fk.ColumnName}");
                selectList += $",\n        {fkAlias}.Label AS {fk.ReferencedTable}";
                if (fk.ReferencedTable == "GeographyElement")
                {
                    string gtAlias = "GT";
                    joinClauses.Add($"    JOIN {Database}.GeographyType AS {gtAlias} ON {gtAlias}.GeographyTypeID = {fkAlias}.GeographyTypeID");
                    selectList += $",\n        {fkAlias}.GeographyTypeID,\n        {gtAlias}.Label AS GeographyType";
                }
            }
            string fromClause = $"{Database}.{TableName} AS {tableAlias}";
            if (joinClauses.Any())
            {
                fromClause += "\n" + string.Join("\n", joinClauses);
            }
            string whereClause = string.Join(" AND\n        ", selectedCols.Select(c => $"{tableAlias}.[{c.Name}] = @{c.Name}"));
            string paramDeclarations = string.Join("\n    ", selectedCols.Select(c => $"@{c.Name} {MapType(c.DataType)} {(c.IsNullable ? "" : "NOT NULL")}"));
            string header = BuildHeader("GetByAttributes", $"{TableName}_GetByAttributes", $"Get {TableName} records by attributes", selectedCols);
            string sql = $"USE [{Database}];\nGO\n\nCREATE OR ALTER PROCEDURE [{Database}].[usp{TableName}_GetByAttributes]\n    {paramDeclarations}\nAS\n{header}\nBEGIN\n    SELECT\n        {selectList}\n    FROM {fromClause}\n    WHERE {whereClause}\nEND\nGO";
            return sql;
        }

        private string GenerateDeleteSql()
        {
            var selectedCols = Columns.Where(c => c.IsSelected).ToList();
            if (!selectedCols.Any())
            {
                System.Windows.MessageBox.Show("Please select columns for deletion criteria.", "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return "";
            }
            string whereClause = string.Join(" AND\n        ", selectedCols.Select(c => $"[{c.Name}] = @{c.Name}"));
            string paramDeclarations = string.Join("\n    ", selectedCols.Select(c => $"@{c.Name} {MapType(c.DataType)} {(c.IsNullable ? "" : "NOT NULL")}"));
            string header = BuildHeader("Delete", $"{TableName}_Delete", $"Delete {TableName} records", selectedCols);
            string sql = $"USE [{Database}];\nGO\n\nCREATE OR ALTER PROCEDURE [{Database}].[usp{TableName}_Delete]\n    {paramDeclarations}\nAS\n{header}\nBEGIN\n    DELETE FROM [{Database}].[{TableName}]\n    WHERE {whereClause}\nEND\nGO";
            return sql;
        }

        private string GenerateSaveSql()
        {
            var selectedCols = Columns.Where(c => c.IsSelected).ToList();
            if (!selectedCols.Any())
            {
                System.Windows.MessageBox.Show("Please select columns to save.", "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return "";
            }
            var pkCols = Columns.Where(c => c.IsPrimaryKey).ToList();
            string paramDeclarations = string.Join("\n    ", selectedCols.Select(c => $"@{c.Name} {MapType(c.DataType)} {(c.IsNullable ? "" : "NOT NULL")}"));
            string insertColumns = string.Join(", ", selectedCols.Select(c => $"[{c.Name}]"));
            string insertValues = string.Join(", ", selectedCols.Select(c => $"@{c.Name}"));
            string updateSet = string.Join(",\n        ", selectedCols.Select(c => $"[{c.Name}] = @{c.Name}"));
            string wherePk = string.Join(" AND ", pkCols.Select(c => $"[{c.Name}] = @{c.Name}"));
            string header = BuildHeader("Save", $"{TableName}_Save", $"Save a {TableName} record", selectedCols);
            string sql;
            if (pkCols.Any())
            {
                sql = $"USE [{Database}];\nGO\n\nCREATE OR ALTER PROCEDURE [{Database}].[usp{TableName}_Save]\n    {paramDeclarations}\nAS\n{header}\nBEGIN\n    IF EXISTS (SELECT 1 FROM [{Database}].[{TableName}] WHERE {wherePk})\n    BEGIN\n        UPDATE [{Database}].[{TableName}]\n        SET\n            {updateSet}\n        WHERE {wherePk}\n    END\n    ELSE\n    BEGIN\n        INSERT INTO [{Database}].[{TableName}] ({insertColumns})\n        VALUES ({insertValues})\n    END\nEND\nGO";
            }
            else
            {
                sql = $"USE [{Database}];\nGO\n\nCREATE OR ALTER PROCEDURE [{Database}].[usp{TableName}_Save]\n    {paramDeclarations}\nAS\n{header}\nBEGIN\n    INSERT INTO [{Database}].[{TableName}] ({insertColumns})\n    VALUES ({insertValues})\nEND\nGO";
            }
            return sql;
        }

        private void SaveToFile(string procedureName, string sql)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = "Select folder for generated procedures";
            dialog.SelectedPath = OutputFolder;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                OutputFolder = dialog.SelectedPath;
                string dir = Path.Combine(OutputFolder, TableName);
                Directory.CreateDirectory(dir);
                File.WriteAllText($"{dir}/{procedureName}", sql);
                System.Windows.MessageBox.Show($"Procedure saved to: {dir}\\{procedureName}", "File Saved", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        private void OpenConnectionDialog()
        {
            try
            {
                var dialog = new Views.DatabaseConnectionDialog();
                dialog.Owner = System.Windows.Application.Current.MainWindow;
                if (dialog.ShowDialog() == true)
                {
                    ConnectionString = dialog.ConnectionString;
                    LoadTables();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error opening connection dialog: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void LoadTables()
        {
            try
            {
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();
                var command = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME", connection);
                var reader = command.ExecuteReader();
                Tables.Clear();
                while (reader.Read())
                {
                    Tables.Add(reader.GetString(0));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                if (Tables.Count == 0)
                {
                    System.Windows.MessageBox.Show($"Failed to load tables: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private string BuildHeader(string procedureType, string procedureName, string description, List<ColumnInfo> paramColumns = null)
        {
            string fullProcedureName = $"{Database}.usp{procedureName}";
            string testString = "";
            if (paramColumns != null && paramColumns.Any())
            {
                string declares = string.Join("\n", paramColumns.Select(c => $"DECLARE @{c.Name} {MapType(c.DataType)} = {GetDefaultValue(c.DataType)};"));
                string execParams = string.Join(",\n    ", paramColumns.Select(c => $"@{c.Name} = @{c.Name}"));
                testString = $"{declares}\nEXEC {fullProcedureName}\n    {execParams};";
            }
            else
            {
                testString = $"EXEC {fullProcedureName};";
            }
            string header = $@"/****************************************************************************************
PROCEDURE:   {fullProcedureName}
DESCRIPTION: {description}
APPLICATION: {Application}
DATABASE:    {Database}

TEST STRING:

{testString}

*******************************************************************************
REVISION HISTORY:
Initials  	Date            Story Number	Description
=========================================================================================
{Initials.PadRight(10)} {DateTime.Now.ToString("MM/dd/yyyy")} {StoryNumber.PadRight(15)} {Description}
*****************************************************************************************/";
            return header;
        }

        private string GetDefaultValue(string dataType)
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

        private string MapType(string dataType)
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();

        public event EventHandler? CanExecuteChanged;
    }
}