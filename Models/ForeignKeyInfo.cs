using CommunityToolkit.Mvvm.ComponentModel;

namespace GAG_Proc_Generator.Models;

public partial class ForeignKeyInfo : ObservableObject
{
    [ObservableProperty]
    private string foreignKeyName = string.Empty;

    [ObservableProperty]
    private string columnName = string.Empty;

    [ObservableProperty]
    private string referencedTable = string.Empty;

    [ObservableProperty]
    private string referencedColumn = string.Empty;

    [ObservableProperty]
    private bool isSelected;
}