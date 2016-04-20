namespace DatabaseBackup.Entities
{
    public class DBFunction
    {
        public string Definition { get; set; }

        public string GetCreationQuery()
        {
            return Definition;
        }
    }
}