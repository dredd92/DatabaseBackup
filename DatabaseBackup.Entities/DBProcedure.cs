namespace DatabaseBackup.Entities
{
    public class DBProcedure
    {
        public string Definition { get; set; }

        public string GetCreationQuery()
        {
            return this.Definition;
        }
    }
}