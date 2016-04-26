using System.Collections.Generic;
using DatabaseBackup.ContractsBLL;
using DatabaseBackup.ContractsDAL;
using DatabaseBackup.DAL;

namespace DatabaseBackup.BLL
{
    public class Logic : ILogic
    {
        private IDao dal = new DBDao();

        // Use when auth is sql security

        public void Backup(string pathToFile, string address, string databaseName, string username, string password)
        {
            this.dal.Backup($@"Data Source={address};Database={databaseName};User Id={username};Password={password}", pathToFile);
        }

        // Use when auth is Windows auth
        public void BackupLocalInstance(string pathToFile, string address, string databaseName)
        {
            this.dal.Backup($@"Data Source={address};Initial Catalog=""{databaseName}"";Integrated Security=True", pathToFile);
        }

        public void Restore(string pathToFile, string address, string username, string password)
        {
            this.dal.Restore(pathToFile, $@"Data Source={address};Database=master;User Id={username};Password={password}");
        }

        public void RestoreLocalInstance(string pathToFile, string address)
        {
            this.dal.Restore(pathToFile, $@"Data Source={address};Initial Catalog=""master"";Integrated Security=True");
        }

        // Use when auth is Sql security
        public IEnumerable<string> ShowDatabases(string address, string username, string password = "")
        {
            return password == "" ? this.dal.ShowDatabases($"Server={address};User Id=myUsername;") : this.dal.ShowDatabases($"Server={address};User Id=myUsername;Password=myPassword");
        }

        // Use when auth is Windows auth
        public IEnumerable<string> ShowDatabasesLocalInstance(string address)
        {
            return this.dal.ShowDatabases($@"Data Source={address};Integrated Security=True");
        }
    }
}