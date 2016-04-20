namespace DatabaseBackup.Bll.Interfaces
{
    public interface IBackupLogic
    {
        void Backup();

        void Connect(string connectionString);

        void Restore(string backupFileName);
    }
}