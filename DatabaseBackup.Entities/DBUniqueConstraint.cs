using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseBackup.Entities
{
    public class DBUniqueConstraint : DBConstraint
    {
        public override string GetCreationQuery()
        {
            return $"ALTER TABLE [{this.TableSchema}].[{this.TableName}] ADD CONSTRAINT {this.Name} UNIQUE ({string.Join(", ", this.Columns)})";
        }
    }
}