using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GAG_Proc_Generator.Constants;
using GAG_Proc_Generator.Models;
using GAG_Proc_Generator.Services;
using GAG_Proc_Generator.Views;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using DialogResult = System.Windows.Forms.DialogResult;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace GAG_Proc_Generator.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DatabaseService databaseService;
    private readonly SqlGenerationService sqlGenerationService;

    [ObservableProperty]
    private string tableName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ColumnInfo> columns = new();

    [ObservableProperty]
    private ObservableCollection<ForeignKeyInfo> foreignKeys = new();

    [ObservableProperty]
    private ObservableCollection<TableJoinInfo> additionalJoins = new();

    [ObservableProperty]
    private string generatedSql = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> tables = new();

    [ObservableProperty]
    private string outputFolder = DatabaseConstants.DefaultOutputFolder;

    [ObservableProperty]
    private string initials = string.Empty;

    [ObservableProperty]
    private string storyNumber = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string application = DatabaseConstants.DefaultApplication;

    [ObservableProperty]
    private string database = DatabaseConstants.DefaultDatabase;

    [ObservableProperty]
    private string connectionString = "Server=yourserver.database.windows.net;Database=YourDatabase;Authentication=Active Directory Integrated;";

    [ObservableProperty]
    private bool isConnected;

    [ObservableProperty]
    private bool isTableLoaded;

    [ObservableProperty]
    private bool isHeaderInfoComplete;

    public MainViewModel()
    {
        databaseService = new DatabaseService();
        sqlGenerationService = new SqlGenerationService();
    }

    partial void OnInitialsChanged(string value) => UpdateHeaderInfoComplete();
    partial void OnStoryNumberChanged(string value) => UpdateHeaderInfoComplete();
    partial void OnDescriptionChanged(string value) => UpdateHeaderInfoComplete();

    private void UpdateHeaderInfoComplete()
    {
        IsHeaderInfoComplete = !string.IsNullOrWhiteSpace(Initials) &&
                               !string.IsNullOrWhiteSpace(StoryNumber) &&
                               !string.IsNullOrWhiteSpace(Description);
    }

    [RelayCommand]
    private async Task ConfigureJoinsAsync()
    {
        if (!IsTableLoaded)
        {
            MessageBox.Show(
                "Please load a table first.",
                MessageStrings.TitleWarning,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            var dialog = new Views.JoinConfigurationDialog
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            await dialog.ViewModel.InitializeAsync(
                ConnectionString,
                TableName,
                Columns,
                AdditionalJoins);

            if (dialog.ShowDialog() == true)
            {
                AdditionalJoins.Clear();
                foreach (var join in dialog.ViewModel.ConfiguredJoins)
                {
                    AdditionalJoins.Add(join);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error configuring joins: {ex.Message}",
                MessageStrings.TitleError,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task LoadTableAsync()
    {
        if (string.IsNullOrWhiteSpace(TableName) || string.IsNullOrEmpty(ConnectionString))
        {
            MessageBox.Show(
                MessageStrings.ConnectToDbFirst,
                MessageStrings.TitleWarning,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        Columns.Clear();
        ForeignKeys.Clear();
        IsTableLoaded = false;

        try
        {
            var columns = await databaseService.GetColumnsAsync(ConnectionString, TableName);
            var foreignKeys = await databaseService.GetForeignKeysAsync(ConnectionString, TableName);

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                foreach (var column in columns)
                {
                    Columns.Add(column);
                }

                foreach (var fk in foreignKeys)
                {
                    ForeignKeys.Add(fk);
                }

                IsTableLoaded = Columns.Count > 0;
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to load table: {ex.Message}",
                MessageStrings.TitleError,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void GenerateGetAll()
    {
        var sql = sqlGenerationService.GenerateGetAllProcedure(
            TableName,
            Database,
            Columns,
            ForeignKeys.Where(fk => fk.IsSelected),
            AdditionalJoins,
            Initials,
            StoryNumber,
            Description,
            Application);

        GeneratedSql = sql;
        SaveToFile($"{DatabaseConstants.ProcedurePrefix}{TableName}{DatabaseConstants.GetAllProcedureSuffix}{DatabaseConstants.SqlFileExtension}", sql);
    }

    [RelayCommand]
    private void GenerateGetByAttributes()
    {
        var selectedCols = Columns.Where(c => c.IsSelected).ToList();
        if (!selectedCols.Any())
        {
            MessageBox.Show(
                MessageStrings.SelectColumnsForFiltering,
                MessageStrings.TitleWarning,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var sql = sqlGenerationService.GenerateGetByAttributesProcedure(
            TableName,
            Database,
            Columns,
            selectedCols,
            ForeignKeys.Where(fk => fk.IsSelected),
            AdditionalJoins,
            Initials,
            StoryNumber,
            Description,
            Application);

        if (!string.IsNullOrEmpty(sql))
        {
            GeneratedSql = sql;
            var procedureSuffix = selectedCols.Count == 1 ? $"_GetBy{selectedCols[0].Name}" : DatabaseConstants.GetByAttributesProcedureSuffix;
            SaveToFile($"{DatabaseConstants.ProcedurePrefix}{TableName}{procedureSuffix}{DatabaseConstants.SqlFileExtension}", sql);
        }
    }

    [RelayCommand]
    private void GenerateDelete()
    {
        var selectedCols = Columns.Where(c => c.IsSelected).ToList();
        if (!selectedCols.Any())
        {
            MessageBox.Show(
                MessageStrings.SelectColumnsForDeletion,
                MessageStrings.TitleWarning,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var sql = sqlGenerationService.GenerateDeleteProcedure(
            TableName,
            Database,
            selectedCols,
            Initials,
            StoryNumber,
            Description,
            Application);

        if (!string.IsNullOrEmpty(sql))
        {
            GeneratedSql = sql;
            SaveToFile($"{DatabaseConstants.ProcedurePrefix}{TableName}{DatabaseConstants.DeleteProcedureSuffix}{DatabaseConstants.SqlFileExtension}", sql);
        }
    }

    [RelayCommand]
    private void GenerateSave()
    {
        var selectedCols = Columns.Where(c => c.IsSelected).ToList();
        if (!selectedCols.Any())
        {
            MessageBox.Show(
                MessageStrings.SelectColumnsToSave,
                MessageStrings.TitleWarning,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var sql = sqlGenerationService.GenerateSaveProcedure(
            TableName,
            Database,
            Columns,
            selectedCols,
            Initials,
            StoryNumber,
            Description,
            Application);

        if (!string.IsNullOrEmpty(sql))
        {
            GeneratedSql = sql;
            SaveToFile($"{DatabaseConstants.ProcedurePrefix}{TableName}{DatabaseConstants.SaveProcedureSuffix}{DatabaseConstants.SqlFileExtension}", sql);
        }
    }

    [RelayCommand]
    private void GenerateInsert()
    {
        var selectedCols = Columns.Where(c => c.IsSelected).ToList();
        if (!selectedCols.Any())
        {
            MessageBox.Show(
                "Please select columns to insert.",
                MessageStrings.TitleWarning,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var sql = sqlGenerationService.GenerateInsertProcedure(
            TableName,
            Database,
            selectedCols,
            Initials,
            StoryNumber,
            Description,
            Application);

        if (!string.IsNullOrEmpty(sql))
        {
            GeneratedSql = sql;
            SaveToFile($"{DatabaseConstants.ProcedurePrefix}{TableName}{DatabaseConstants.InsertProcedureSuffix}{DatabaseConstants.SqlFileExtension}", sql);
        }
    }

    [RelayCommand]
    private void GenerateAllProcs()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select folder for generated procedures",
            SelectedPath = OutputFolder
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;

        OutputFolder = dialog.SelectedPath;
        var dir = Path.Combine(OutputFolder, TableName);
        Directory.CreateDirectory(dir);

        var getAllSql = sqlGenerationService.GenerateGetAllProcedure(
            TableName, Database, Columns, ForeignKeys.Where(fk => fk.IsSelected),
            AdditionalJoins, Initials, StoryNumber, Description, Application);
        File.WriteAllText(Path.Combine(dir, $"{DatabaseConstants.ProcedurePrefix}{TableName}{DatabaseConstants.GetAllProcedureSuffix}{DatabaseConstants.SqlFileExtension}"), getAllSql);

        var getBySql = sqlGenerationService.GenerateGetByAttributesProcedure(
            TableName, Database, Columns, Columns.Where(c => c.IsSelected),
            ForeignKeys.Where(fk => fk.IsSelected), AdditionalJoins, Initials, StoryNumber, Description, Application);
        File.WriteAllText(Path.Combine(dir, $"{DatabaseConstants.ProcedurePrefix}{TableName}{DatabaseConstants.GetByAttributesProcedureSuffix}{DatabaseConstants.SqlFileExtension}"), getBySql);

        var insertSql = sqlGenerationService.GenerateInsertProcedure(
            TableName, Database, Columns.Where(c => c.IsSelected),
            Initials, StoryNumber, Description, Application);
        File.WriteAllText(Path.Combine(dir, $"{DatabaseConstants.ProcedurePrefix}{TableName}{DatabaseConstants.InsertProcedureSuffix}{DatabaseConstants.SqlFileExtension}"), insertSql);

        var deleteSql = sqlGenerationService.GenerateDeleteProcedure(
            TableName, Database, Columns.Where(c => c.IsSelected),
            Initials, StoryNumber, Description, Application);
        File.WriteAllText(Path.Combine(dir, $"{DatabaseConstants.ProcedurePrefix}{TableName}{DatabaseConstants.DeleteProcedureSuffix}{DatabaseConstants.SqlFileExtension}"), deleteSql);

        var saveSql = sqlGenerationService.GenerateSaveProcedure(
            TableName, Database, Columns, Columns.Where(c => c.IsSelected),
            Initials, StoryNumber, Description, Application);
        File.WriteAllText(Path.Combine(dir, $"{DatabaseConstants.ProcedurePrefix}{TableName}{DatabaseConstants.SaveProcedureSuffix}{DatabaseConstants.SqlFileExtension}"), saveSql);

        MessageBox.Show(
            MessageStrings.AllProcsGenerated,
            MessageStrings.TitleSuccess,
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void SaveToFile(string fileName, string sql)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select folder for generated procedures",
            SelectedPath = OutputFolder
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;

        OutputFolder = dialog.SelectedPath;
        var dir = Path.Combine(OutputFolder, TableName);
        Directory.CreateDirectory(dir);
        var filePath = Path.Combine(dir, fileName);
        File.WriteAllText(filePath, sql);

        MessageBox.Show(
            $"Procedure saved to: {filePath}",
            MessageStrings.TitleFileSaved,
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    [RelayCommand]
    private async Task OpenConnectionDialogAsync()
    {
        try
        {
            var dialog = new DatabaseConnectionDialog
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            if (dialog.ShowDialog() != true) return;

            ConnectionString = dialog.ConnectionString;
            await LoadTablesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error opening connection dialog: {ex.Message}",
                MessageStrings.TitleError,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task LoadTablesAsync()
    {
        try
        {
            var tables = await databaseService.GetTablesAsync(ConnectionString);

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Tables.Clear();

                foreach (var table in tables)
                {
                    Tables.Add(table);
                }

                IsConnected = Tables.Count > 0;
            });
        }
        catch (Exception ex)
        {
            IsConnected = false;
            if (Tables.Count == 0)
            {
                MessageBox.Show(
                    $"Failed to load tables: {ex.Message}",
                    MessageStrings.TitleError,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
