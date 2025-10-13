using CommunityToolkit.Mvvm.ComponentModel;

namespace GAG_Proc_Generator.Models;

public partial class ColumnInfo : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string dataType = string.Empty;

    [ObservableProperty]
    private bool isNullable;

    [ObservableProperty]
    private bool isPrimaryKey;

    [ObservableProperty]
    private bool isSelected;
}