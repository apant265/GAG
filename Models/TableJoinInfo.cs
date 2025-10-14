using CommunityToolkit.Mvvm.ComponentModel;

namespace GAG_Proc_Generator.Models;

public partial class TableJoinInfo : ObservableObject
{
    [ObservableProperty]
    private string joinTable = string.Empty;

    [ObservableProperty]
    private string joinColumn = string.Empty;

    [ObservableProperty]
    private string baseTableColumn = string.Empty;

    [ObservableProperty]
    private string joinType = "INNER JOIN";

    [ObservableProperty]
    private bool isSelected = true;

    public string DisplayText => $"{JoinType} {JoinTable} ON {JoinTable}.{JoinColumn} = BaseTable.{BaseTableColumn}";
}
