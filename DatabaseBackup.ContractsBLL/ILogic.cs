namespace DatabaseBackup.ContractsBLL
{
    public interface ILogic
    {
        void Backup(string pathToFile, string address, string databaseName, string username, string password);

        void BackupLocalInstance(string pathToFile, string address, string databaseName);

        void Restore(string pathToFile, string address, string username, string password);

        void RestoreLocalInstance(string pathToFile, string address);

        System.Collections.Generic.IEnumerable<string> ShowDatabases(string address, string username, string password);

        System.Collections.Generic.IEnumerable<string> ShowDatabasesLocalInstance(string address);
    }
}