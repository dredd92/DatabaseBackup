using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DatabaseBackup.Entities
{
    public class DBTable
    {
        public System.Collections.Generic.IEnumerable<DBColumn> Columns { get; set; }
        public IEnumerable<DBConstraint> Constraints { get; set; }
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
                result.AppendLine($"\t[{column.Name}] {column.DataType}{(column.CharactersMaxLength == -1 ? string.Empty : $"({column.CharactersMaxLength})")} {(column.IsNullable ? "NULL" : "NOT NULL")} {(column.Default == null ? string.Empty : "DEFAULT" + column.Default)},");
            }

            result.AppendLine(");");
            return result.ToString();
        }
    }
}