using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBackup.Entities
{
    public class DBPrimaryKeyConstraint : DBConstraint
    {
        public override string GetCreationQuery()
        {
            return $"ALTER TABLE {this.TableName} ADD CONSTRAINT {this.Name} PRIMARY KEY ({string.Join(", ", this.Columns)})";
        }
    }
}