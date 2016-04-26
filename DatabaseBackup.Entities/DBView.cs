using System.Collections.Generic;

namespace DatabaseBackup.Entities
{
    public class DBView
    {
        public string Definition { get; set; }

        public string Name { get; set; }

        public string Schema { get; set; }

        public IEnumerable<DBTrigger> Triggers { get; set; }

        public string GetCreationQuery()
        {
            return Definition;
        }
    }
}