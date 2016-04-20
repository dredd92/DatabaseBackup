using System.Collections.Generic;

namespace DatabaseBackup.ContractsDAL
{
    public interface IDao
    {
        void Backup(string conString);

        void Restore(System.DateTime date, string conString);

        IEnumerable<string> ShowDatabases(string conString);
    }
}