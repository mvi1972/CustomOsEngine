using OsEngine.Commands;
using OsEngine.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.ViewModels
{
    public class RobotWindowVM : ViewModelBase  
    {
        #region  ================================ Свойства =====================================

        #endregion
        #region  ================================ Поля =====================================

        #endregion
        #region  ================================ Команды =====================================

        private DelegateCommand comandServerConect;
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
        #region  ================================ Методы =====================================
        void ServerConect(object o)
        {
            ServerMaster.ShowDialog(false);
        }

        #endregion

    }
}
