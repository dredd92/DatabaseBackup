using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBackup.Entities
{
    public class DBCheckConstraint : DBConstraint
    {
        public string CheckClause { get; set; }

        public override string GetCreationQuery()
        {
            return $"ALTER TABLE [{this.TableSchema}].[{this.TableName}] ADD CONSTRAINT {this.Name} CHECK {this.CheckClause}";
        }
    }
}
