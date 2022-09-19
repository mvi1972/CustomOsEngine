using OsEngine.Commands;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.MyEntity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static protobuf.ws.TradesRequest;
using Direction = OsEngine.MyEntity.Direction;

namespace OsEngine.ViewModels
{
    public class MyRobotVM : ViewModelBase
    {
        public MyRobotVM()
        {
            ServerMaster.ServerCreateEvent += ServerMaster_ServerCreateEvent;
        }

 
        #region Поля =====================================================================================

        // List<IServer> _servers = new List<IServer>();
        IServer _server;

        List<Security> _securities = new List<Security>();

        private Security _security;

        #endregion

        #region Свойства ================================================================================== 

        public string Header
        {
            get => _selectedSecurity;
            set
            {
                _selectedSecurity = value;
                OnPropertyChanged(nameof(Header));
            }
        }
        private string _header;

        public ObservableCollection <string> ListSecurities { get; set; } =  new ObservableCollection<string>();
        /// <summary>
        /// Выбранная бумага
        /// </summary>
        public string SelectedSecurity
        { 
            get => _selectedSecurity;
            set
            {
                _selectedSecurity = value;  
                OnPropertyChanged(nameof(SelectedSecurity));
                _security = GetSecuritiesForName(_selectedSecurity);
                StartSecuritiy(_security);
            }
        }
        private string _selectedSecurity = "";

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

        #region Методы =====================================================================================

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
            /*
            foreach (IServer server in _servers)
            {
                if (newserver == server)
                {
                    return;
                }
            } 
            _servers.Add(newserver);
            newserver.PortfoliosChangeEvent += Newserver_PortfoliosChangeEvent;
            newserver.SecuritiesChangeEvent += Newserver_SecuritiesChangeEvent;
            newserver.NeadToReconnectEvent += Newserver_NeadToReconnectEvent;
            newserver.NewMarketDepthEvent += Newserver_NewMarketDepthEvent;
            newserver.NewTradeEvent += Newserver_NewTradeEvent;
            newserver.NewOrderIncomeEvent += Newserver_NewOrderIncomeEvent;
            newserver.NewMyTradeEvent += Newserver_NewMyTradeEvent;
            newserver.ConnectStatusChangeEvent += Newserver_ConnectStatusChangeEvent;
            */
            if (newserver == _server)
            {
                return;
            }
            _server =newserver;
            _server.PortfoliosChangeEvent += Newserver_PortfoliosChangeEvent;
            _server.SecuritiesChangeEvent += Newserver_SecuritiesChangeEvent;
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
        private Security GetSecuritiesForName(string name)
        {
            for (int i = 0; i < _securities.Count; i++)
            {
                if (_securities[i].Name == name)
                {
                    return _securities[i];
                }
            }
            return null;
        }
        private void Newserver_SecuritiesChangeEvent(List<Security> securities)
        {
            ObservableCollection<string> listsecurities = new ObservableCollection<string>();
            for (int i = 0; i < securities.Count; i++)
            {
                listsecurities.Add(securities[i].Name);
            }

            ListSecurities = listsecurities;
            OnPropertyChanged(nameof(ListSecurities));
            _securities = securities;
        }

        private void Newserver_PortfoliosChangeEvent(List<Portfolio> portfolios)
        {
            
        }

        void ServerConect (object o)
        {
            ServerMaster.ShowDialog(false);
        }

        #endregion
    }
}
