using DatabaseBackup.Dal.Interfaces;

namespace DatabaseBackup.Dal
{
    public class DataAccessLayer : IDataAccessLayer
    {
        private WorkingDatabase workingDatabase;

        public void BackupDatabase()
        {
            workingDatabase.Backup();
        }

        public void ConnectToDatabase(string connectionString)
        {
            workingDatabase = new WorkingDatabase(connectionString);
        }

        public void RestoreDatabase(string backupFileName)
        {
            string sqlDump = GetBackupFileContent(backupFileName);
            workingDatabase.Restore(sqlDump);
        }

        private string GetBackupFileContent(string filename)
        {
            // open and read file
            return "CREATE table1";
        }
    }
}