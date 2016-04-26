using DatabaseBackup.BLL;
using DatabaseBackup.ContractsBLL;

namespace DatabaseBackup.Presentation
{
    public static class LogicKeeper
    {
        public static ILogic Logic = new Logic();
    }
}