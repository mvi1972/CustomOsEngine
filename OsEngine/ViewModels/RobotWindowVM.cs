﻿ using OsEngine.Charts.CandleChart.Indicators;
using OsEngine.Commands;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.MyEntity;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.Serialization;
using static OsEngine.ViewModels.GridRobotVM;
using System.Runtime.Serialization.Json;

namespace OsEngine.ViewModels
{
    [DataContract]  
    public class RobotWindowVM : BaseVM
    {
        public RobotWindowVM()
        {
            ServerMaster.ServerCreateEvent += ServerMaster_ServerCreateEvent;

            // поток для записи логирования 
            Task.Run(() =>
            {
                RecordLog();
            });

            LoadHeaderBot();

            ServerMaster.ActivateAutoConnection();     

           
        }

        #region  ================================ Свойства =====================================

        /// <summary>
        /// название статегии 
        /// </summary>
        public NameStrat NameStrat
        {
            get => _nameStrat;
            set
            {
                _nameStrat = value;
                OnPropertyChanged(nameof(NameStrat));
            }
        }
        private NameStrat _nameStrat;

        /// <summary>
        /// список типов стратегий
        /// </summary>
        public List<NameStrat> NameStrategies { get; set; } = new List<NameStrat>()
        {
            NameStrat.GRID, NameStrat.BREAKDOWN, NameStrat.NONE
        };
        /// <summary>
        /// колекция созданых роботов
        /// </summary> 
        public ObservableCollection<IRobotVM> Robots { get; set; } = new ObservableCollection<IRobotVM>();
        /// <summary>
        /// выбранный робот
        /// </summary>
        public IRobotVM SelectedRobot
        {
            get => _selectedRobot;
            set
            {
                _selectedRobot = value;
                OnPropertyChanged(nameof(SelectedRobot));
            }
        }
        private IRobotVM _selectedRobot;

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


        CultureInfo CultureInfo = new CultureInfo("ru-RU");

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
        /// в событии создания нового сервера // подписались на новый ордер
        /// </summary>
        private void ServerMaster_ServerCreateEvent(Market.Servers.IServer server)
        {
            // подписались на новый ордер
            server.NewOrderIncomeEvent += Server_NewOrderIncomeEvent;
            // изменились портфели
            server.PortfoliosChangeEvent += Server_PortfoliosChangeEvent;
           
        }

        private void Server_PortfoliosChangeEvent(List<Portfolio> portfolios)
         {
            //GetNameSecuretiClass();
        }

        private void GetBalansSecur()
        {
            List<Portfolio> portfolios = new List<Portfolio>();
            //if (Server.Portfolios != null)
            //{
            //    portfolios = Server.Portfolios;
            //}
            //if (portfolios.Count > 0 && portfolios != null
            //    && _selectedSecurity != null)
            //{
            //    int count = portfolios[0].GetPositionOnBoard().Count;
            //    string nam = SelectedSecurity.Name;
            //    string suf = "_BOTH";
            //    string SecurName = nam + suf;
            //    for (int i = 0; i < count; i++)
            //    {
            //        string seсurCode = portfolios[0].GetPositionOnBoard()[i].SecurityNameCode;
            //        if (seсurCode == SecurName)
            //        {
            //            decimal d = portfolios[0].GetPositionOnBoard()[i].ValueCurrent;
            //            SelectSecurBalans = d; // отправка значения в свойство
            //        }
            //    }
            //}

            //decimal balans = portfolios[0].GetPositionOnBoard()[0].Find(pos =>
            //    pos.SecurityNameCode == _securName).ValueCurrent;
            //    return balans;

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
            // колеция ордеров по бумаге, ключ NumberMarket
            ConcurrentDictionary<string, Order> ordNam = Orders[order.SecurityNameCode];

            foreach (var numMark in ordNam)
            {
                Order numOrd = numMark.Value;
                RobotWindowVM.SendStrTextDb(" номер ордера в Orders = " + numOrd.NumberMarket);
                RobotWindowVM.SendStrTextDb(" статус ордера в Orders = " + numOrd.State);
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
            AddTab("", NameStrat);
        }

        void AddTab (string name, NameStrat strat)
        {
            if (name !="")
            {
                if (strat == NameStrat.GRID)
                {
                    Robots.Add(new GridRobotVM(name, Robots.Count));
                }
                if (strat == NameStrat.BREAKDOWN)
                {
                    Robots.Add(new RobotBreakVM(name, Robots.Count));
                }
                
                //Robots.Last().Header = name;
            }
            else
            {
                if (strat == NameStrat.GRID)
                {
                    Robots.Add(new GridRobotVM(Robots.Count));
                    Robots.Last().Header = "Tab " + (Robots.Count + 1);
                }
                if (strat == NameStrat.BREAKDOWN)
                {
                    Robots.Add(new RobotBreakVM(Robots.Count));
                    Robots.Last().Header = "Tab " + (Robots.Count + 1);
                }
 
            }
            Robots.Last().OnSelectedSecurity += RobotWindowVM_OnSelectedSecurity; // подписываемся на создание новой вкладки робота
        }

        private void RobotWindowVM_OnSelectedSecurity(string name)
        {
            SaveHeaderBot();
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
                    SaveHeaderBot();
                }
            }
        }

        /// <summary>
        /// конструктор отправки строки в лог
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
        /// конструктор отправки строки в лог с стартегией
        /// </summary>
        public static void Log(string name, string strat, string str)
        {
            MessageForLog mess = new MessageForLog()
            {
                Name = name,
                Strategy = strat,   
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
        private void SaveHeaderBot()
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
                    if (SelectedRobot == null)
                    {
                        writer.WriteLine("none tab");
                    }
                    else writer.WriteLine(SelectedRobot.Header);

                    writer.WriteLine(SelectedRobot.NameStrat);

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
        private void LoadHeaderBot()
        {
            if (!Directory.Exists(@"Parametrs"))
            {
                return;
            }
            string strTabs = "";
            string header = "";
            string strStrat = "";
            try
            {
                using (StreamReader reader = new StreamReader(@"Parametrs\param.txt"))
                {
                    strTabs = reader.ReadLine();
                    header = reader.ReadLine();
                    strStrat = reader.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Log("App", " Ошибка выгрузки параметров = " + ex.Message);
            }
            string[] tabs = strTabs.Split(';');
            //string[] strat = strStrat.Split(';');
            foreach (string tab in tabs)
            {
                if (tab != "")
                {
                    AddTab(tab, NameStrat);
                    if (Robots.Last().Header == header) 
                    {
                        if (NameStrat == NameStrat.GRID)
                        {
                            SelectedRobot = (GridRobotVM)Robots.Last();
                        }
                        if (NameStrat == NameStrat.BREAKDOWN)
                        {
                            SelectedRobot = (RobotBreakVM)Robots.Last();
                        }
                    }
                }
            }

        }

        /// <summary>
        /// отправить строку в дебаг
        /// </summary>
        public static void SendStrTextDb(string text, string text2 = null)
        {
            string str = text + " \n" + text2 + "\n";
            Debug.WriteLine(str);
        }

        #endregion
    }
}