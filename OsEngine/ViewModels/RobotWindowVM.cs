using OsEngine.Commands;
using OsEngine.Market;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.ViewModels
{
    public class RobotWindowVM : ViewModelBase  
    {
        public RobotWindowVM()
        {
  
        }
        #region  ================================ Свойства =====================================

        public ObservableCollection <MyRobotVM> Robots { get; set; } = new ObservableCollection<MyRobotVM>(); 

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
        private DelegateCommand comandAddRobot;
        public DelegateCommand ComandAddRobot
        {
            get
            {
                if (comandAddRobot == null)
                {
                    comandAddRobot = new DelegateCommand(AddTabRobot);
                }
                return comandAddRobot;
            } 
        }

        #endregion
        #region  ================================ Методы =====================================
        /// <summary>
        ///  подключение к серверу 
        /// </summary>
        void ServerConect(object o)
        {
            ServerMaster.ShowDialog(false);
        }
        /// <summary>
        ///  добавление робота 
        /// </summary>
        void AddTabRobot(object o)
        {
            Robots.Add(new MyRobotVM()
            {
                Header = "Tab" + (Robots.Count +1)
            });
             
        }

        #endregion

    }
}
