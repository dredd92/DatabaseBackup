using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBackup.Entities
{
    public class DBSynonym
    {
        public string Catalogue { get; set; }
        public string Name { get; set; }
        public string ObjectName { get; set; }
        public string Schema { get; set; }

        public string GetCreationQuery()
        {
            return $"CREATE SYNONYM [{this.Schema}].[{this.Name}] FOR [{this.Catalogue}].[{this.Schema}].[{this.ObjectName}]";
        }
    }
}