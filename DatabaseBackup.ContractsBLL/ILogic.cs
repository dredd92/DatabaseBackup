namespace DatabaseBackup.ContractsBLL
{
    public interface ILogic
    {
        void Backup(string address, string databaseName, string username, string password);

        void BackupLocalInstance(string address, string databaseName);

        System.Collections.Generic.IEnumerable<string> ShowDatabases(string address, string username, string password);

        System.Collections.Generic.IEnumerable<string> ShowDatabasesLocalInstance(string address);

        void Restore(System.DateTime date);
    }
}