namespace GAG_Proc_Generator.Models
{
    public class ForeignKeyInfo
    {
        public string ForeignKeyName { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public string ReferencedTable { get; set; } = string.Empty;
        public string ReferencedColumn { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}