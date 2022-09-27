using OsEngine.Commands;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.MyEntity;
using OsEngine.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Direction = OsEngine.MyEntity.Direction;

namespace OsEngine.ViewModels
{
    public class MyRobotVM : BaseVM
    {
        public MyRobotVM()
        {
            ServerMaster.ServerCreateEvent += ServerMaster_ServerCreateEvent;
        }


        #region Поля =====================================================================================


        /// <summary>
        /// список портфелей 
        /// </summary>
        public ObservableCollection<string> StringPortfoios { get; set; } = new ObservableCollection<string>();

        Portfolio _portfolio;
        #endregion

        #region Свойства ================================================================================== 


        public string Header
        {
            get
            {
                if (SelectedSecurity !=null)
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

        /// <summary>
        /// Выбранная бумага
        /// </summary>
        public Security SelectedSecurity
        { 
            get => _selectedSecurity;
            set
            {
                _selectedSecurity = value;  
                OnPropertyChanged(nameof(SelectedSecurity));
                OnPropertyChanged(nameof(Header));
            }
        }
        private Security _selectedSecurity =null;

        public ServerType ServerType
        {
            get
            {
                if (Server ==null)
                {
                    return ServerType.None;
                }
                return Server.ServerType;
            }
        }
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
        private string _stringportfolio;


        /// <summary>
        /// точка страта работы робота (цена)
        /// </summary>
        public decimal StartPoint
        { 
            get => _startPoint;
            set
            {
                _startPoint = value;
                OnPropertyChanged(nameof(StartPoint));
            }
        }
        private decimal _startPoint;

        /// <summary>
        /// количество уровней 
        /// </summary>
        public int   CountLevels
        {
            get => _сountLevels;
            set
            {
                _сountLevels = value;
                OnPropertyChanged(nameof(CountLevels));
            }
        }
        private int _сountLevels;

        /// <summary>
        /// направление сделок StepType
        /// </summary>
        public Direction Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                OnPropertyChanged(nameof(Direction));
            }
        }
        private Direction _direction;


        /// <summary>
        ///  Lot
        /// </summary>
        public decimal Lot
        {
            get => _lot;
            set
            {
                _lot = value;
                OnPropertyChanged(nameof(Lot));
            }
        }
        private decimal _lot;

        /// <summary>
        /// тип расчета  
        /// </summary>
        public StepType StepType
        {
            get => _stepType;
            set
            {
                _stepType = value;
                OnPropertyChanged(nameof(StepType));
            }
        }
        private StepType _stepType;

        /// <summary>
        /// Шаг уровня 
        /// </summary>
        public decimal StepLevel
        {
            get => _stepLevel;
            set
            {
                _stepLevel = value;
                OnPropertyChanged(nameof(StepLevel));
            }
        }
        private decimal _stepLevel;

        /// <summary>
        /// Профит уровня 
        /// </summary>
        public decimal TakeLevel
        {
            get => _takeLevel;
            set
            {
                _takeLevel = value;
                OnPropertyChanged(nameof(TakeLevel));
            }
        }
        private decimal _takeLevel;

        /// <summary>
        /// количество активных уровней 
        /// </summary>
        public int MaxActiveLevel
        {
            get => _maxActiveLevel;
            set
            {
                _maxActiveLevel = value;
                OnPropertyChanged(nameof(MaxActiveLevel));
            }
        }
        private int _maxActiveLevel;

        /// <summary>
        /// всего позиций 
        /// </summary>
        public int AllPositionsCount
        {
            get => _allPositionsCount;
            set
            {
                _allPositionsCount = value;
                OnPropertyChanged(nameof(AllPositionsCount));
            }
        }
        private int _allPositionsCount;

        /// <summary>
        /// Средняя цена 
        /// </summary>
        public decimal PriceAverege
        {
            get => _priceAverege;
            set
            {
                _priceAverege = value;
                OnPropertyChanged(nameof(PriceAverege));
            }
        }
        private decimal _priceAverege;

        /// <summary>
        /// Комиссия  
        /// </summary>
        public decimal VarMargine
        {
            get => _varMargine;
            set
            {
                _varMargine = value;
                OnPropertyChanged(nameof(VarMargine));
            }
        }
        private decimal _varMargine;

        /// <summary>
        /// Прибыль 
        /// </summary>
        public decimal Accum
        {
            get => _accum;
            set
            {
                _accum = value;
                OnPropertyChanged(nameof(Accum));
            }
        }
        private decimal _accum;

        /// <summary>
        /// Итого  
        /// </summary>
        public decimal Total
        {
            get => _total;
            set
            {
                _total = value;
                OnPropertyChanged(nameof(Total));
            }
        }
        private decimal _total;

        public IServer Server
        {
            get => _server;
            set
            {
                _server = value;
                OnPropertyChanged(nameof(ServerType));

                StringPortfoios = GetStringPortfoios(_server);
                if (StringPortfoios != null || StringPortfoios.Count >0)
                {
                    StringPortfolio = StringPortfoios[0];
                } 
                OnPropertyChanged(nameof(StringPortfoios));
            } 
        }
        private IServer _server;

        #endregion

        #region Команды =====================================================================================


        private DelegateCommand _commandSelectSecurity;
        public DelegateCommand CommandSelectSecurity
        {
            get
            {
                if (_commandSelectSecurity == null)
                {
                    _commandSelectSecurity = new DelegateCommand(SelectSecurity);
                }
                return _commandSelectSecurity;
            }
        }

        #endregion

        #region Методы =====================================================================================

        private ObservableCollection<string> GetStringPortfoios(IServer server)
        {
            ObservableCollection<string> stringPortfoios = new ObservableCollection<string>();
            if (server == null)
            {
                return stringPortfoios;
            }
           
            foreach (Portfolio portf in server.Portfolios)
            {
                stringPortfoios.Add(portf.Number);
            }
            return stringPortfoios;  
        }

        private Portfolio GetPortfolio(string number)
        {
            if (Server == null)
            {
                foreach (Portfolio portf in Server.Portfolios)
                {
                    if (portf.Number == number)
                    {
                        return portf;
                    }
                }
            }
  
            return null;
        }

        /// <summary>
        /// выбрать бумагу
        /// </summary>
        void SelectSecurity (object o)
        {
            if (RobotWindowVM.ChengeEmitendWidow != null ) 
            {
                return;
            }
            RobotWindowVM.ChengeEmitendWidow = new ChengeEmitendWidow(this);
            RobotWindowVM.ChengeEmitendWidow.ShowDialog();
            RobotWindowVM.ChengeEmitendWidow = null; 

        } 
        /// <summary>
        /// Начать получать данные по бумге
        /// </summary> 
        private void StartSecuritiy(Security security)
        {
            if (security == null)
            {
                return;
            }
            Task.Run(() =>
            {
                while (true)
                {
                    var series = Server.StartThisSecurity(security.Name, new TimeFrameBuilder(), security.NameClass);
                    if (series != null)
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                }
            });
        }
         
        private void ServerMaster_ServerCreateEvent(IServer newserver)
        {
            if (newserver == Server)
            {
                return;
            }
            Server =newserver;
            Server.PortfoliosChangeEvent += Newserver_PortfoliosChangeEvent;
            Server.NeadToReconnectEvent += Newserver_NeadToReconnectEvent;
            Server.NewMarketDepthEvent += Newserver_NewMarketDepthEvent;
            Server.NewTradeEvent += Newserver_NewTradeEvent;
            Server.NewOrderIncomeEvent += Newserver_NewOrderIncomeEvent;
            Server.NewMyTradeEvent += Newserver_NewMyTradeEvent;
            Server.ConnectStatusChangeEvent += Newserver_ConnectStatusChangeEvent;
            Server.NewCandleIncomeEvent += _server_NewCandleIncomeEvent;
  
        }

        private void _server_NewCandleIncomeEvent(CandleSeries candleSeries)
        {
            candleSeries.СandleUpdeteEvent += CandleSeries_СandleUpdeteEvent;
        }

        private void CandleSeries_СandleUpdeteEvent(CandleSeries candle)
        {
          
        }

        private void Newserver_ConnectStatusChangeEvent(string conenect)
        {
            
        }

        private void Newserver_NewMyTradeEvent(Entity.MyTrade myTrade)
        {
            
        }

        private void Newserver_NewOrderIncomeEvent(Entity.Order order)
        {
           
        }

        private void Newserver_NewTradeEvent(List<Entity.Trade> trade)
        {
            
        }

        private void Newserver_NewMarketDepthEvent(Entity.MarketDepth marketDepth)
        {
            
        }

        private void Newserver_NeadToReconnectEvent()
        {
            
        }

        private void Newserver_PortfoliosChangeEvent(List<Portfolio> portfolios)
        {
            
        }


        #endregion
    }
}
