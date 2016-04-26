using System.Collections.Generic;
using System.Text;

namespace DatabaseBackup.Entities
{
    public class DBTable
    {
        public System.Collections.Generic.IEnumerable<DBColumn> Columns { get; set; }

        public IEnumerable<DBData> Data { get; set; }

        public string Name { get; set; }

        public string Schema { get; set; }

        public IEnumerable<DBTrigger> Triggers { get; set; }

        public string GetCreationQuery()
        {
            var result = new StringBuilder();
            result.AppendLine($"CREATE TABLE [{this.Schema}].[{this.Name}] (");

            foreach (var column in this.Columns)
            {
                switch (column.DataType)
                {
                    case "nvarchar":
                    case "varchar":
                    case "varbinary":
                        if (column.CharactersMaxLength == -1)
                        {
                            result.AppendLine($"\t[{column.Name}] {column.DataType}(MAX) {(column.IsNullable ? "NULL" : "NOT NULL")} {(column.Default == null ? string.Empty : "DEFAULT" + column.Default)},");
                            break;
                        }
                        else
                        {
                            result.AppendLine($"\t[{column.Name}] {column.DataType}({column.CharactersMaxLength}){(column.IsNullable ? "NULL" : "NOT NULL")} {(column.Default == null ? string.Empty : "DEFAULT" + column.Default)},");
                            break;
                        }
                    default:
                        result.AppendLine($"\t[{column.Name}] {column.DataType}{(column.CharactersMaxLength == -1 || column.DataType == "text" || column.DataType == "ntext" || column.DataType == "image" ? string.Empty : $"({column.CharactersMaxLength})")} {(column.IsNullable ? "NULL" : "NOT NULL")} {(column.Default == null ? string.Empty : "DEFAULT" + column.Default)},");
                        break;
                }

                //result.AppendLine($"\t[{column.Name}] {column.DataType}{(column.CharactersMaxLength == -1 || column.DataType == "text" || column.DataType == "ntext" ? string.Empty : $"({column.CharactersMaxLength})")} {(column.IsNullable ? "NULL" : "NOT NULL")} {(column.Default == null ? string.Empty : "DEFAULT" + column.Default)},");
            }

            result.AppendLine(")");
            return result.ToString();
        }
    }
}