using DatabaseBackup.BLL;
using DatabaseBackup.ContractsBLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseBackup.Presentation
{
    public static class LogicKeeper
    {
        public static ILogic logic = new Logic();
    }
}