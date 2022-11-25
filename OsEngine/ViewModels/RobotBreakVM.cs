using Newtonsoft.Json;
using OsEngine.Charts.CandleChart.Indicators;
using OsEngine.Commands;
using OsEngine.Entity;
using OsEngine.Logging;
using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.Market.Servers.GateIo.Futures.Response;
using OsEngine.Market.Servers.Transaq.TransaqEntity;
using OsEngine.MyEntity;
using OsEngine.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Action = OsEngine.MyEntity.Action;
using Direction = OsEngine.MyEntity.Direction;
using Security = OsEngine.Entity.Security;

namespace OsEngine.ViewModels
{
    public class RobotBreakVM : BaseVM, IRobotVM
    {
        #region ==============конструкторы=========================
        public RobotBreakVM(string header, int numberTab)
        {
            string[] str = header.Split('=');
            NumberTab = numberTab;
            Header = str[0];
            Load(header);
            ServerMaster.ServerCreateEvent += ServerMaster_ServerCreateEvent;
        }
        /// <summary>
        /// конструктор для нового робота 
        /// </summary>
        public RobotBreakVM(int numberTab)
        {
            NumberTab = numberTab;
        }
        #endregion
        #region ======================================поля================================
        public int NumberTab = 0;
        Portfolio _portfolio;

        public List<Side> Sides { get; set; } = new List<Side>()
        {
            Side.Buy,
            Side.Sell,
        };
        #endregion
        #region ==============================Свойтсва===========================================
        public string Header
        {
            get
            {
                if (SelectedSecurity != null)
                {
                    return SelectedSecurity.Name;
                }
                else
                {
                    return _header;
                }
            }
            set
            {
                _header = value;
                OnPropertyChanged(nameof(Header));
            }
        }
        private string _header;
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

        public string StringPortfolio
        {
            get => _stringportfolio;
            set
            {
                _stringportfolio = value;
                OnPropertyChanged(nameof(StringPortfolio));

                _portfolio = GetPortfolio(_stringportfolio);
            }
        }
        private string _stringportfolio = "";

        public IServer Server
        {
            get => _server;
            set
            {
                if (Server != null)
                {
                    //UnSubscribeToServer();
                    _server = null;
                }
                _server = value;
                OnPropertyChanged(nameof(ServerType));

                //SubscribeToServer(); // подключаемя к бир
            }
        }
        private IServer _server = null;

        public ObservableCollection<string> StringPortfolios { get; set; } = new ObservableCollection<string>();
 
 
        /// <summary>
        /// выбрать бумагу
        /// </summary>
        void SelectSecurity(object o)
        {
            if (RobotWindowVM.ChengeEmitendWidow != null)
            {
                return;
            }
            RobotWindowVM.ChengeEmitendWidow = new ChengeEmitendWidow(this);
            RobotWindowVM.ChengeEmitendWidow.ShowDialog();
            RobotWindowVM.ChengeEmitendWidow = null;
            if (_server != null)
            {
                if (_server.ServerType == ServerType.Binance
                    || _server.ServerType == ServerType.BinanceFutures)
                {
                   // IsChekCurrency = true;
                }
                //else IsChekCurrency = false;
            }
        }
        public Security SelectedSecurity
        {
            get => _selectedSecurity;
            set
            {
                _selectedSecurity = value;
                OnPropertyChanged(nameof(SelectedSecurity));
                OnPropertyChanged(nameof(Header));
                if (SelectedSecurity != null)
                {
                    StartSecuritiy(SelectedSecurity); // запуск бумаги 
                    OnSelectedSecurity?.Invoke(SelectedSecurity.Name);
                }
            }
        }
        private Security _selectedSecurity = null;
        public List<Direction> Directions { get ; set ; } = new List<Direction>()
        {
            Direction.BUY, Direction.SELL, Direction.BUYSELL
        };
        public List<StepType> StepTypes { get ; set; } = new List<StepType>()
        {
            StepType.PUNKT, StepType.PERCENT
        };
        #endregion
        #region ======================Методы ============================================

        private Portfolio GetPortfolio(string number)
        {
            if (Server != null && Server.Portfolios != null)
            {
                foreach (Portfolio portf in Server.Portfolios)
                {
                    if (portf.Number == number)
                    {
                        RobotWindowVM.Log(Header, " Выбран портфель =  " + portf.Number);
                        return portf;
                    }
                }
            }

            RobotWindowVM.Log(Header, "GetStringPortfolios  портфель = null ");
            return null;
        }
        /// <summary>
        /// загрузка во вкладку параметров 
        /// </summary>
        private void Load(string name)
        {
            if (!Directory.Exists(@"Parametrs\Tabs"))
            {
                return;
            }
            string servType = "";
            try
            {
                using (StreamReader reader = new StreamReader(@"Parametrs\Tabs\param_" + name + ".txt"))
                {
                    Header = reader.ReadLine(); // загружаем заголовок
                    servType = reader.ReadLine(); // загружаем название сервера
                    StringPortfolio = reader.ReadLine();  // загружаем бумагу 
                    //StartPoint = GetDecimalForString(reader.ReadLine());
                    //CountLevels = (int)GetDecimalForString(reader.ReadLine());

                    Direction direct = Direction.BUY;
                    if (Enum.TryParse(reader.ReadLine(), out direct))
                    {
                        //Direction = direct;
                    }

                    //Lot = GetDecimalForString(reader.ReadLine());

                    StepType step = StepType.PUNKT;
                    if (Enum.TryParse(reader.ReadLine(), out direct))
                    {
                        //StepType = step;
                    }

                    //StepLevel = GetDecimalForString(reader.ReadLine());
                    //TakeLevel = GetDecimalForString(reader.ReadLine());
                   // MaxActiveLevel = (int)GetDecimalForString(reader.ReadLine());
                    //PriceAverege = GetDecimalForString(reader.ReadLine());
                    //Accum = GetDecimalForString(reader.ReadLine());

                    //Levels = JsonConvert.DeserializeAnonymousType(reader.ReadLine(), new ObservableCollection<Level>());

                    bool check = false;
                    if (bool.TryParse(reader.ReadLine(), out check))
                    {
                        //IsChekCurrency = check;
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                RobotWindowVM.Log(Header, " Ошибка выгрузки параметров = " + ex.Message);
            }
            //StartServer(servType);

        }

        private void ServerMaster_ServerCreateEvent(IServer server)
        {
            //if (server.ServerType == ServerType)
            {
                //Server = server;
            }
        }
        private void StartSecuritiy(Security security)
        {
            if (security == null)
            {
                RobotWindowVM.Log(Header, "StartSecuritiy  security = null ");
                return;
            }
            Task.Run(() =>
            {
                while (true)
                {
                    var series = Server.StartThisSecurity(security.Name, new TimeFrameBuilder(), security.NameClass);
                    if (series != null)
                    {
                        RobotWindowVM.Log(Header, "StartSecuritiy  security = " + series.Security.Name);
                        Save();
                        break;
                    }
                    Thread.Sleep(1000);
                }
            });
        }
        private void Save()
        {
            if (!Directory.Exists(@"Parametrs\Tabs"))
            {
                Directory.CreateDirectory(@"Parametrs\Tabs");
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(@"Parametrs\Tabs\param_" + Header + "=" + NumberTab + ".txt", false))
                {
                    writer.WriteLine(Header);
                    //writer.WriteLine(ServerType);
                    writer.WriteLine(StringPortfolio);
                    //writer.WriteLine(StartPoint);
                    //writer.WriteLine(CountLevels);
                    //writer.WriteLine(Direction);
                    //writer.WriteLine(Lot);
                    //writer.WriteLine(StepType);
                    //writer.WriteLine(StepLevel);
                    //writer.WriteLine(TakeLevel);
                    //writer.WriteLine(MaxActiveLevel);
                    //writer.WriteLine(PriceAverege);
                    //writer.WriteLine(Accum);

                    //writer.WriteLine(JsonConvert.SerializeObject(Levels));

                    //writer.WriteLine(IsChekCurrency);

                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                RobotWindowVM.Log(Header, " Ошибка сохранения параметров = " + ex.Message);

            }
        }
        #endregion


        public event GridRobotVM.selectedSecurity OnSelectedSecurity;
    }
}
