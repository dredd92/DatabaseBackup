using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBackup.Entities
{
    public class DBSchema
    {
        public string Name { get; set; }

        public string GetCreationQuery()
        {
            return $"CREATE SCHEMA {Name}";
        }
    }
}