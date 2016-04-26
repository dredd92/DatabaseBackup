using DatabaseBackup.BLL;
using DatabaseBackup.ContractsBLL;

namespace DatabaseBackup.ConsoleApp
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            ILogic logic = new Logic();
            //logic.BackupLocalInstance(@"(localdb)\mssqllocaldb", "AdventureWorks2012");
        }
    }
}