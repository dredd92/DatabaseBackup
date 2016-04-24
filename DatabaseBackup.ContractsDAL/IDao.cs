using System.Collections.Generic;

namespace DatabaseBackup.ContractsDAL
{
    public interface IDao
    {
        void Backup(string conString, string pathToFile);

        void Restore(string pathToFile, string conString);

        IEnumerable<string> ShowDatabases(string conString);
    }
}