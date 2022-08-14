using OsEngine.Commands;
using OsEngine.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.ViewModels
{
    public class MyRobotVM : ViewModelBase
    {
        #region Поля =====================================================================================

        private DelegateCommand comandServerConect;

        #endregion

        #region Команды =====================================================================================

        public DelegateCommand ComandServerConect
        {
            get
            {
                if (comandServerConect == null)
                {
                    comandServerConect = new DelegateCommand(ServerConect);
                }
                return comandServerConect;
            }
        }
        #endregion

        #region Методы =====================================================================================
        void ServerConect (object o)
        {
            ServerMaster.ShowDialog(false);
        }

        #endregion
    }
}
