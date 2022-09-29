using OsEngine.Commands;
using OsEngine.Market;
using OsEngine.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OsEngine.ViewModels
{
    public class RobotWindowVM : BaseVM  
    {
        public RobotWindowVM()
        {
            Task.Run(() =>
            {
                RecordLog();
            });
  
        }
        #region  ================================ Свойства =====================================

        public ObservableCollection <MyRobotVM> Robots { get; set; } = new ObservableCollection<MyRobotVM>();



        #endregion
        #region  ================================ Поля =====================================

        /// <summary>
        /// коллекция  для логов из разных потоков 
        /// </summary>
        private static ConcurrentQueue<string> _logMessges = new ConcurrentQueue<string>();    

        /// <summary>
        /// окно выбора инструмента
        /// </summary>
        public static ChengeEmitendWidow ChengeEmitendWidow = null;

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
        private DelegateCommand comandDeleteRobot;
        public DelegateCommand ComandDeleteRobot
        {
            get
            {
                if (comandDeleteRobot == null)
                {
                    comandDeleteRobot = new DelegateCommand(DeleteTabRobot);
                }
                return comandDeleteRobot;
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
        /// <summary>
        /// Удаление вкладки робота
        /// </summary>
        void DeleteTabRobot(object obj)
        {
            string header= (string)obj;

            MyRobotVM delRobot =null;

            foreach (var robot in Robots)
            {
                if (robot.Header == header )
                {
                    delRobot = robot;
                    break;
                }
            }
            if (delRobot != null)
            {
                MessageBoxResult res= MessageBox.Show("Удалить вкладку " + header + "?", header, MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    Robots.Remove(delRobot);
                }    
            }
        }

        /// <summary>
        /// Очередь логирования
        /// </summary>
        public static void Log(string str)
        {
            _logMessges.Enqueue(str);
        }
        /// <summary>
        /// Запись логa 
        /// </summary>
        private static void RecordLog()
        {
            if (!Directory.Exists (@"Log"))
            {
                Directory.CreateDirectory(@"Log");
            }
            while (MainWindow.ProccesIsWorked)
            {
                string str;

                if (_logMessges.TryDequeue (out str))
                {
                    string name = "Log"+ DateTime.Now.ToShortDateString() + ".txt";

                    using (StreamWriter writer = new StreamWriter(@"Log\" + name, true))
                    {
                        writer.WriteLine(str);
                        writer.Close();
                    }
                }
                Thread.Sleep(10);
            }
        }

        #endregion

    }
}
