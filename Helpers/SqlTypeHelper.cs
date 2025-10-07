using System.Data;

namespace GAG_Proc_Generator.Helpers
{
    public static class SqlTypeHelper
    {
        public static string MapSqlTypeToCSharp(SqlDbType sqlType)
        {
            return sqlType switch
            {
                SqlDbType.BigInt => "long",
                SqlDbType.Int => "int",
                SqlDbType.SmallInt => "short",
                SqlDbType.TinyInt => "byte",
                SqlDbType.Bit => "bool",
                SqlDbType.Decimal => "decimal",
                SqlDbType.Money => "decimal",
                SqlDbType.SmallMoney => "decimal",
                SqlDbType.Float => "double",
                SqlDbType.Real => "float",
                SqlDbType.DateTime => "DateTime",
                SqlDbType.DateTime2 => "DateTime",
                SqlDbType.SmallDateTime => "DateTime",
                SqlDbType.Date => "DateTime",
                SqlDbType.Time => "TimeSpan",
                SqlDbType.DateTimeOffset => "DateTimeOffset",
                SqlDbType.Char => "string",
                SqlDbType.NChar => "string",
                SqlDbType.VarChar => "string",
                SqlDbType.NVarChar => "string",
                SqlDbType.Text => "string",
                SqlDbType.NText => "string",
                SqlDbType.Binary => "byte[]",
                SqlDbType.VarBinary => "byte[]",
                SqlDbType.Image => "byte[]",
                SqlDbType.UniqueIdentifier => "Guid",
                SqlDbType.Xml => "string",
                _ => "object"
            };
        }

        public static string MapCSharpToSql(string csharpType)
        {
            return csharpType.ToLower() switch
            {
                "long" => "BIGINT",
                "int" => "INT",
                "short" => "SMALLINT",
                "byte" => "TINYINT",
                "bool" => "BIT",
                "decimal" => "DECIMAL",
                "double" => "FLOAT",
                "float" => "REAL",
                "datetime" => "DATETIME",
                "timespan" => "TIME",
                "datetimeoffset" => "DATETIMEOFFSET",
                "string" => "NVARCHAR(MAX)",
                "byte[]" => "VARBINARY(MAX)",
                "guid" => "UNIQUEIDENTIFIER",
                _ => "NVARCHAR(MAX)"
            };
        }
    }
}