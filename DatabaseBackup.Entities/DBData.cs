using System.Collections.Generic;

namespace DatabaseBackup.Entities
{
    public class DBData
    {
        public Dictionary<string, string> NameValue { get; set; }
        public string TableName { get; set; }
        public string TableSchema { get; set; }

        public string GetCreationQuery()
        {
            return $"INSERT INTO [{this.TableSchema}].[{this.TableName}]({string.Join(", ", this.NameValue.Keys)}) VALUES ({string.Join(", ", this.NameValue.Values)})";
        }
    }
}