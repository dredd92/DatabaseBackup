using System.Collections.Generic;

namespace DatabaseBackup.Entities
{
    public abstract class DBConstraint
    {
        public List<string> Columns { get; set; }

        public string Name { get; set; }

        public string TableName { get; set; }

        public string TableSchema { get; set; }

        public abstract string GetCreationQuery();
    }
}