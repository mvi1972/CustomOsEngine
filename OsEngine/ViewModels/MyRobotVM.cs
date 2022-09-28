using OsEngine.Commands;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.MyEntity;
using OsEngine.Views;
using System;
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


        Portfolio _portfolio;
        #endregion

        #region Свойства ================================================================================== 


        /// <summary>
        /// список портфелей 
        /// </summary>
        public ObservableCollection<string> StringPortfolios { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// колекция уровней 
        /// </summary>
        public ObservableCollection<Level> Levels { get; set; } = new ObservableCollection<Level>() ;
         
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
        public List<Direction> Directions { get; set; } = new List<Direction>()
        {
            Direction.BUY, Direction.BUY, Direction.BUYSELL
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
            StepType.PUNKT, StepType.PERCENT
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

        public IServer Server
        {
            get => _server;
            set
            {
                _server = value;
                OnPropertyChanged(nameof(ServerType));

                StringPortfolios = GetStringPortfolios(_server);
                if (StringPortfolios != null && StringPortfolios.Count >0)
                {
                    StringPortfolio = StringPortfolios[0];
                } 
                OnPropertyChanged(nameof(StringPortfolios));
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
        private DelegateCommand _сommandCalculate;
        public DelegateCommand CommandCalculate
        {
            get
            {
                if (_сommandCalculate == null)
                {
                    _сommandCalculate = new DelegateCommand(Calculate);
                }
                return _сommandCalculate;
            }
        }

        #endregion

        #region Методы =====================================================================================
        /// <summary>
        /// расчитывает уровни 
        /// </summary>
        void Calculate( object o)
        {
            ObservableCollection<Level> levels = new ObservableCollection<Level>();

            decimal currBuyPrice = StartPoint;
            decimal currSellPrice = StartPoint;

            if (CountLevels <=0)
            {
                return;
            }
            for (int i = 0; i < CountLevels; i++)
            {
                Level levelBuy = new Level() {Side = Side.Buy};
                Level levelSell = new Level() { Side = Side.Sell };

                if (StepType == StepType.PUNKT)
                {
                    currBuyPrice -= StepLevel * SelectedSecurity.PriceStep;
                    currSellPrice += StepLevel * SelectedSecurity.PriceStep;
                }
                else if (StepType == StepType.PERCENT)
                {
                    currBuyPrice -= StepLevel * currBuyPrice / 100;
                    currBuyPrice = Decimal.Round(currBuyPrice, SelectedSecurity.Decimals);

                    currSellPrice += StepLevel * currSellPrice / 100;
                    currSellPrice = Decimal.Round(currSellPrice, SelectedSecurity.Decimals);
                }
                levelSell.PriceLevel = currSellPrice;
                levelBuy.PriceLevel = currBuyPrice;

                if (Direction == Direction.BUY || Direction == Direction.BUYSELL)
                {
                    levels.Add(levelBuy);
                }
                if (Direction == Direction.SELL || Direction == Direction.BUYSELL)
                {
                    levels.Insert(0, levelSell);
                }
            }

            Levels = levels;
            OnPropertyChanged(nameof(Levels));
        }

        private ObservableCollection<string> GetStringPortfolios(IServer server)
        {
            ObservableCollection<string> stringPortfolios = new ObservableCollection<string>();
            if (server == null)
            {
                return stringPortfolios;
            }
            if (server.Portfolios == null)
            {
                return stringPortfolios;
            }
           
            foreach (Portfolio portf in server.Portfolios)
            {
                stringPortfolios.Add(portf.Number);
            }
            return stringPortfolios;  
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
