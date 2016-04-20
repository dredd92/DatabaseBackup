using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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