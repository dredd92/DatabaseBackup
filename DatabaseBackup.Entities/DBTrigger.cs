namespace DatabaseBackup.Entities
{
    public class DBTrigger
    {
        public string Definition { get; set; }
        public string Name { get; set; }

        public string GetCreationQuery()
        {
            return this.Definition;
        }
    }
}