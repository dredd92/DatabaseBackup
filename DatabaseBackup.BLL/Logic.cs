using DatabaseBackup.ContractsBLL;
using DatabaseBackup.ContractsDAL;
using DatabaseBackup.DAL;
using System;
using System.Collections.Generic;
using System.IO;

namespace DatabaseBackup.BLL
{
    public class Logic : ILogic
    {
        private IDao dal = new DBDao();

        // Use when auth is sql security

        public void Backup(string address, string databaseName, string username, string password)
        {
            Path.Combine(Path.GetTempPath(), "DatabaseBackup");
            this.dal.Backup($"Server={address};Database={databaseName};User Id={username};Password={password}");
        }

        // Use when auth is Windows auth
        public void BackupLocalInstance(string address, string databaseName)
        {
            this.dal.Backup($@"Data Source={address};Initial Catalog=""{databaseName}"";Integrated Security=True");
        }

        public void Restore(System.DateTime date)
        {
            this.dal.Restore(date, @"Data Source=(localdb)\mssqllocaldb;Integrated Security=True");
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