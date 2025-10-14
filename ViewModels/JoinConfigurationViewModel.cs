using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GAG_Proc_Generator.Models;
using GAG_Proc_Generator.Services;

namespace GAG_Proc_Generator.ViewModels;

public partial class JoinConfigurationViewModel : ObservableObject
{
    private readonly DatabaseService databaseService;

    [ObservableProperty]
    private ObservableCollection<string> availableTables = new();

    [ObservableProperty]
    private ObservableCollection<ColumnInfo> baseTableColumns = new();

    [ObservableProperty]
    private ObservableCollection<ColumnInfo> selectedTableColumns = new();

    [ObservableProperty]
    private ObservableCollection<TableJoinInfo> configuredJoins = new();

    [ObservableProperty]
    private string selectedJoinTable = string.Empty;

    [ObservableProperty]
    private string selectedJoinColumn = string.Empty;

    [ObservableProperty]
    private string selectedBaseColumn = string.Empty;

    [ObservableProperty]
    private string selectedJoinType = "INNER JOIN";

    [ObservableProperty]
    private string baseTableName = string.Empty;

    [ObservableProperty]
    private string connectionString = string.Empty;

    public ObservableCollection<string> JoinTypes { get; } = new()
    {
        "INNER JOIN",
        "LEFT JOIN",
        "RIGHT JOIN",
        "FULL OUTER JOIN"
    };

    public JoinConfigurationViewModel()
    {
        databaseService = new DatabaseService();
    }

    partial void OnSelectedJoinTableChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(ConnectionString))
        {
            _ = LoadSelectedTableColumnsAsync();
        }
    }

    public async Task InitializeAsync(string connectionString, string baseTableName, IEnumerable<ColumnInfo> baseColumns, IEnumerable<TableJoinInfo> existingJoins)
    {
        ConnectionString = connectionString;
        BaseTableName = baseTableName;

        BaseTableColumns.Clear();
        foreach (var column in baseColumns)
        {
            BaseTableColumns.Add(column);
        }

        ConfiguredJoins.Clear();
        foreach (var join in existingJoins)
        {
            ConfiguredJoins.Add(join);
        }

        await LoadAvailableTablesAsync();
    }

    private async Task LoadAvailableTablesAsync()
    {
        try
        {
            var tables = await databaseService.GetTablesAsync(ConnectionString);
            AvailableTables.Clear();
            foreach (var table in tables.Where(t => t != BaseTableName))
            {
                AvailableTables.Add(table);
            }
        }
        catch
        {
            // Handle error silently or log
        }
    }

    private async Task LoadSelectedTableColumnsAsync()
    {
        try
        {
            var columns = await databaseService.GetColumnsAsync(ConnectionString, SelectedJoinTable);
            SelectedTableColumns.Clear();
            foreach (var column in columns)
            {
                SelectedTableColumns.Add(column);
            }
        }
        catch
        {
            // Handle error silently or log
        }
    }

    [RelayCommand]
    private void AddJoin()
    {
        if (string.IsNullOrWhiteSpace(SelectedJoinTable) ||
            string.IsNullOrWhiteSpace(SelectedJoinColumn) ||
            string.IsNullOrWhiteSpace(SelectedBaseColumn))
        {
            return;
        }

        var newJoin = new TableJoinInfo
        {
            JoinTable = SelectedJoinTable,
            JoinColumn = SelectedJoinColumn,
            BaseTableColumn = SelectedBaseColumn,
            JoinType = SelectedJoinType,
            IsSelected = true
        };

        ConfiguredJoins.Add(newJoin);

        SelectedJoinTable = string.Empty;
        SelectedJoinColumn = string.Empty;
        SelectedBaseColumn = string.Empty;
        SelectedJoinType = "INNER JOIN";
        SelectedTableColumns.Clear();
    }

    [RelayCommand]
    private void RemoveJoin(TableJoinInfo join)
    {
        ConfiguredJoins.Remove(join);
    }

    [RelayCommand]
    private void MoveJoinUp(TableJoinInfo join)
    {
        var index = ConfiguredJoins.IndexOf(join);
        if (index > 0)
        {
            ConfiguredJoins.Move(index, index - 1);
        }
    }

    [RelayCommand]
    private void MoveJoinDown(TableJoinInfo join)
    {
        var index = ConfiguredJoins.IndexOf(join);
        if (index < ConfiguredJoins.Count - 1)
        {
            ConfiguredJoins.Move(index, index + 1);
        }
    }
}
