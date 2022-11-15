 using OsEngine.Charts.CandleChart.Indicators;
using OsEngine.Commands;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.MyEntity;
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
            ServerMaster.ServerCreateEvent += ServerMaster_ServerCreateEvent;

            Task.Run(() =>
            {
                RecordLog();
            });

            Load();
               
            ServerMaster.ActivateAutoConnection();
        }


        #region  ================================ Свойства =====================================
        /// <summary>
        /// колекция созданых роботов
        /// </summary> 
        public ObservableCollection<IRobotVM> Robots { get; set; } = new ObservableCollection<IRobotVM>();
        /// <summary>
        /// выбранный робот
        /// </summary>
        public RobotVM SelectedRobot
        {
            get => _selectedRobot;
            set
            {
                _selectedRobot = value;
                OnPropertyChanged(nameof(SelectedRobot));
            }
        }
        private RobotVM _selectedRobot; 

        #endregion
        #region  ================================ Поля =====================================

        /// <summary>
        /// коллекция  для логов из разных потоков 
        /// </summary>
        private static ConcurrentQueue<MessageForLog> _logMessges = new ConcurrentQueue<MessageForLog>();

        /// <summary>
        /// поле окна выбора инструмента
        /// </summary>
        public static ChengeEmitendWidow ChengeEmitendWidow = null;
        /// <summary>
        /// многопоточный словарь для ордеров
        /// </summary>
        public static ConcurrentDictionary<string, ConcurrentDictionary<string, Order>> 
            Orders = new ConcurrentDictionary<string, ConcurrentDictionary<string, Order>>();   

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
        /// событие создания нового сервера 
        /// </summary>
        private void ServerMaster_ServerCreateEvent(Market.Servers.IServer server)
        {
            server.NewOrderIncomeEvent += Server_NewOrderIncomeEvent;
        }
        /// <summary>
        /// добвляет или обновляет пришедшие ордера с биржы в словарь ордеров на компе
        /// </summary>
        private void Server_NewOrderIncomeEvent(Order order)
        {
            ConcurrentDictionary<string, Order> numberOrders = null;
            if (Orders.TryGetValue(order.SecurityNameCode, out numberOrders))
            {
                numberOrders.AddOrUpdate(order.NumberMarket, order, (key, value) => value= order);
            }
            else
            {
                numberOrders = new ConcurrentDictionary<string, Order>();
                numberOrders.AddOrUpdate(order.NumberMarket, order, (key, value) => value = order);

                Orders.AddOrUpdate(order.SecurityNameCode, numberOrders, (key, value) => value = numberOrders);
            }
        }

        /// <summary>
        ///  подключение к серверу 
        /// </summary>
        void ServerConect(object o)
        {
            ServerMaster.ShowDialog(false);
        }
        /// <summary>
        ///  добавление робота на вкладку 
        /// </summary>
        void AddTabRobot(object o)
        {
            AddTab("");
        }

        void AddTab (string name)
        {
            if (name !="")
            {
                Robots.Add(new RobotVM(name, Robots.Count));
                //Robots.Last().Header = name;
            }
            else
            {
                Robots.Add(new RobotVM(Robots.Count));
                Robots.Last().Header = "Tab " + (Robots.Count + 1);
            }
            Robots.Last().OnSelectedSecurity += RobotWindowVM_OnSelectedSecurity; // подписываемся на создание новой вкладки робота
        }

        private void RobotWindowVM_OnSelectedSecurity(string name)
        {
            Save();
        }

        /// <summary>
        /// Удаление вкладки робота
        /// </summary>
        void DeleteTabRobot(object obj)
        {
            if (SelectedRobot != null)
            {
                MessageBoxResult res = MessageBox.Show("Удалить вкладку " + SelectedRobot.Header + "?", SelectedRobot.Header, MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    Robots.Remove(SelectedRobot);
                    Save();
                }
            }
        }

        /// <summary>
        /// отправка строки в лог
        /// </summary>
        public static void Log(string name, string str)
        {
            MessageForLog mess = new MessageForLog()
            {
                Name = name,
                Message = str
            };
            _logMessges.Enqueue(mess);
        }
        /// <summary>
        /// Запись логa 
        /// </summary>
        private static void RecordLog()
        {
            if (!Directory.Exists(@"Log"))
            {
                Directory.CreateDirectory(@"Log");
            }
            while (MainWindow.ProccesIsWorked)
            {
                MessageForLog mess;

                if (_logMessges.TryDequeue(out mess))
                {
                    string name = "Log" + mess.Name + "_"+ DateTime.Now.ToShortDateString() + ".txt";

                    using (StreamWriter writer = new StreamWriter(@"Log\" + name, true))
                    {
                        writer.WriteLine(mess.Message);
                        writer.Close();
                    }
                }
                Thread.Sleep(10);
            }
        }
        /// <summary>
        /// сохранение заголовка и бумаги последнего выбраного робота
        /// </summary>
        private void Save()
        {
            if (!Directory.Exists(@"Parametrs"))
            {
                Directory.CreateDirectory(@"Parametrs");
            }

            string str = "";

            for (int i=0; i< Robots.Count; i++)
            {
                str += Robots[i].Header  + "=" + i + ";";
            }
            try
            {
                using ( StreamWriter writer = new StreamWriter(@"Parametrs\param.txt"))
                {
                    writer.WriteLine(str);
                    writer.WriteLine(SelectedRobot.Header);

                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Log("App", " Ошибка сохранения параметров = " + ex.Message);
            }
        }
        /// <summary>
        /// загрузка в робота параметров 
        /// </summary>
        private void Load()
        {
            if (!Directory.Exists(@"Parametrs"))
            {
                return;
            }
            string strTabs = "";
            string header = "";
            try
            {
                using (StreamReader reader = new StreamReader(@"Parametrs\param.txt"))
                {
                    strTabs = reader.ReadLine();
                    header = reader.ReadLine(); 
                }
            }
            catch (Exception ex)
            {
                Log("App", " Ошибка выгрузки параметров = " + ex.Message);
            }
            string[] tabs = strTabs.Split(';');
            foreach (string tab in tabs)
            {
                if (tab != "")
                {
                    AddTab(tab);
                    if (Robots.Last().Header == header) 
                    {
                        SelectedRobot = (RobotVM)Robots.Last();
                    }
                }
            }    

            #endregion
        }
    }
}