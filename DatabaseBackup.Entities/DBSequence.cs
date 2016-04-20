using System;

namespace DatabaseBackup.Entities
{
    public class DBSequence
    {
        public string DataType { get; set; }
        public long Increment { get; set; }
        public bool IsCached { get; set; }
        public long MaxValue { get; set; }
        public long MinValue { get; set; }
        public string Name { get; set; }
        public string Schema { get; set; }
        public long StartValue { get; set; }

        public string GetCreationQuery()
        {
            return $"CREATE SEQUENCE [{this.Schema}].[{this.Name}]{Environment.NewLine}AS [{this.DataType}]{Environment.NewLine}START WITH {this.StartValue.ToString()}{Environment.NewLine}INCREMENT BY {this.Increment.ToString()}{Environment.NewLine}MINVALUE {this.MinValue.ToString()}{Environment.NewLine}MAXVALUE {this.MaxValue.ToString()}{Environment.NewLine}{(IsCached ? "CACHE" : "NO CACHE")}";
        }
    }
}