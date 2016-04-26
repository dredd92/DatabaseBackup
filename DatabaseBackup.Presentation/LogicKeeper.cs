using DatabaseBackup.BLL;
using DatabaseBackup.ContractsBLL;

namespace DatabaseBackup.Presentation
{
    internal static class LogicKeeper
    {
        public static ILogic Logic = new Logic();
    }
}