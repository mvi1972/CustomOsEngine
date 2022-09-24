using OsEngine.Commands;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.MyEntity;
using OsEngine.Views;
using System.Collections.Generic;
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

        // List<IServer> _servers = new List<IServer>();
        IServer _server;

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
                //  StartSecuritiy(_security);
            }
        }
        private Security _selectedSecurity =null;

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
        public List<Direction> Directions { get; set; } = new List<Direction>()
        {
            Direction.BUY, Direction.SELL, Direction.BUYSELL
        };

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
        public List<StepType> StepTypes { get; set; } = new List<StepType>()
        {
           StepType.PERCENT, StepType.PUNKT
        };

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

        #endregion

        #region Команды =====================================================================================

        // SelectSecurity
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
        /// Насать получать данные по бумге
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
                    var series = _server.StartThisSecurity(security.Name, new TimeFrameBuilder(), security.NameClass);
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
            if (newserver == _server)
            {
                return;
            }
            _server =newserver;
            _server.PortfoliosChangeEvent += Newserver_PortfoliosChangeEvent;
            _server.NeadToReconnectEvent += Newserver_NeadToReconnectEvent;
            _server.NewMarketDepthEvent += Newserver_NewMarketDepthEvent;
            _server.NewTradeEvent += Newserver_NewTradeEvent;
            _server.NewOrderIncomeEvent += Newserver_NewOrderIncomeEvent;
            _server.NewMyTradeEvent += Newserver_NewMyTradeEvent;
            _server.ConnectStatusChangeEvent += Newserver_ConnectStatusChangeEvent;
            _server.NewCandleIncomeEvent += _server_NewCandleIncomeEvent;
  
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
