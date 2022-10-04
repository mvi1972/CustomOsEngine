using OsEngine.Commands;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.MyEntity;
using OsEngine.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Direction = OsEngine.MyEntity.Direction;

namespace OsEngine.ViewModels
{
    public class MyRobotVM : BaseVM
    {
        public MyRobotVM()
        {
            
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
                if (SelectedSecurity != null )
                {
                    StartSecuritiy(SelectedSecurity); // запуск бумаги 
                }
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
        /// Рыночная цена бумаги 
        /// </summary>
        public decimal Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged(nameof(Price));
            }
        }
        private decimal _price;

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

        /// <summary>
        /// вкл\выкл
        /// </summary>
        public bool IsRun
        {
            get => _isRun;
            set
            {
                _isRun = value;
                OnPropertyChanged(nameof(IsRun));
             }
        }
        private bool _isRun;

        public IServer Server
        {
            get => _server;
            set
            {
                if (Server != null)
                {
                    UnSubscribeToServer();
                    _server =null;
                }
                _server = value;
                OnPropertyChanged(nameof(ServerType));
                SubscribeToServer(); // подключаемя к бир
                StringPortfolios = GetStringPortfolios(_server); // грузим портфели
                if (StringPortfolios != null && StringPortfolios.Count >0)
                {
                    StringPortfolio = StringPortfolios[0];
                } 
                OnPropertyChanged(nameof(StringPortfolios));
            } 
        }
        private IServer _server = null;

        #endregion

        #region Команды =====================================================================================

        private DelegateCommand _commandStartStop;
        public DelegateCommand CommandStartStop
        {
            get
            {
                if (_commandStartStop == null)
                {
                    _commandStartStop = new DelegateCommand(StartStop);
                }
                return _commandStartStop;
            }
        }
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

        private void TradeLogic()
        {
            if (IsRun == false
                || SelectedSecurity == null)
            {
                return;
            }

            decimal stepLevel = 0;
            if (StepType == StepType.PUNKT)
            {
                stepLevel = StepLevel * SelectedSecurity.PriceStep;
            }
            else if (StepType == StepType.PERCENT)
            {
                stepLevel = StepLevel * Price / 100;
                stepLevel = Decimal.Round(stepLevel, SelectedSecurity.Decimals);
            }

            decimal borderUp = Price + stepLevel * MaxActiveLevel;
            decimal borderDown = Price - stepLevel * MaxActiveLevel;

            foreach (Level level in Levels)
            {
                if (level.PassVolume
                    && level.PriceLevel !=0
                    && Math.Abs(level.Volume) + level.OrderVolume  < Lot)
                {
                    if (level.Side == Side.Sell
                        && level.PriceLevel <= borderUp)
                    {

                    }
                    else if (level.Side == Side.Buy
                           && level.PriceLevel >= borderDown)
                    {
                        decimal worklot = Lot - Math.Abs(level.Volume) + level.OrderVolume;
                        RobotWindowVM.Log(" Уровень = " + level.GetStringForSave());

                        level.PassVolume = false;
                        level.Order = SendOrder(SelectedSecurity, level.PriceLevel, worklot, Side.Buy);

                        RobotWindowVM.Log(" Отправляем лимитку  " + GetStringForSave(level.Order));
                    }
                }
            }
        }
        /// <summary>
        ///  отправить оредер на биржу 
        /// </summary>
        private Order SendOrder(Security sec, decimal prise, decimal volume, Side side)
        {
            if (string.IsNullOrEmpty(StringPortfolio))
            {
                // сообщение в лог  сделать 
                MessageBox.Show(" еще нет портфеля ");
                return null;
            }
            Order order = new Order()
            {
                Price = prise,  
                Volume = volume,    
                Side = side, 
                PortfolioNumber = StringPortfolio,
                TypeOrder = OrderPriceType.Limit,
                NumberUser = NumberGen.GetNumberOrder(StartProgram.IsOsTrader),
                SecurityNameCode = sec.Name,
                SecurityClassCode = sec.NameClass,
            };     
            Server.ExecuteOrder(order);
            return order;
        }
 
        private void StartStop( object o)
        {
            RobotWindowVM.Log(" \n\n StartStop = " + !IsRun);
            Thread.Sleep(300);

            IsRun = !IsRun;

            if (IsRun)
            {
                foreach (Level level in Levels)
                {
                    level.PassVolume = true;
                    level.PassTake = true;
                }
                TradeLogic();
            }
            else
            {
                foreach (Level level in Levels)
                {
                    if (level.Order != null
                        && level.Order.State == OrderStateType.Activ
                        || level.Order.State == OrderStateType.Patrial
                        || level.Order.State == OrderStateType.Pending)
                    {
                        Server.CancelOrder(level.Order);
                        RobotWindowVM.Log(" Снимаем лимитки на сервере " + GetStringForSave(level.Order));
                    }
                }
                foreach (Level level in Levels)
                {
                    if (level.LimitTake != null
                        && level.Order.State == OrderStateType.Activ
                        || level.Order.State == OrderStateType.Patrial
                        || level.Order.State == OrderStateType.Pending)
                    {
                        Server.CancelOrder(level.LimitTake);
                        RobotWindowVM.Log(" Снимаем тэйк на сервере " + GetStringForSave(level.LimitTake));
                    }
                }
            }
        }

        /// <summary>
        ///  подключиться к серверу
        /// </summary>
        private void SubscribeToServer()
        {
            if (Server != null)
            {
                UnSubscribeToServer();
            }
            Server.NewMyTradeEvent += Server_NewMyTradeEvent;
            Server.NewOrderIncomeEvent += Server_NewOrderIncomeEvent;
            Server.NewCandleIncomeEvent += Server_NewCandleIncomeEvent;
            Server.NewTradeEvent += Server_NewTradeEvent;
            RobotWindowVM.Log(" Подключаемся к серверу = " + _server.ServerType);
        }
        /// <summary>
        ///  отключиться от сервера 
        /// </summary>
        private void UnSubscribeToServer()
        {
            Server.NewMyTradeEvent -= Server_NewMyTradeEvent;
            Server.NewOrderIncomeEvent -= Server_NewOrderIncomeEvent;
            Server.NewCandleIncomeEvent -= Server_NewCandleIncomeEvent;
            Server.NewTradeEvent -= Server_NewTradeEvent;

            RobotWindowVM.Log(" Отключаемся от сервера = " + _server.ServerType);
        }

        private void Server_NewTradeEvent(List<Trade> trades)
        {
            if (trades != null
                && trades[0].SecurityNameCode == SelectedSecurity.Name)
            {
                Price = trades.Last().Price;
            }
        }

        private void Server_NewCandleIncomeEvent(CandleSeries candle)
        {
            
        }

        private void Server_NewOrderIncomeEvent(Order order)
        {
            if (order.SecurityNameCode == SelectedSecurity.Name
                && order.ServerType == Server.ServerType) // && order.PortfolioNumber == _portfolio.Number
            {
                RobotWindowVM.Log("NewOrderIncomeEvent = " + GetStringForSave(order));

                foreach (Level level in Levels)
                {
                    if (level.Order != null)
                    {
                        if (level.Order.NumberUser == order.NumberUser
                           || level.Order.NumberMarket == order.NumberMarket)
                        {
                            RobotWindowVM.Log("лимитный ордер " );

                            level.Order.NumberMarket = order.NumberMarket;
                            level.Order.State = order.State;
                            level.Order.Volume = order.Volume;

                            level.PassVolume = true;

                            RobotWindowVM.Log("Уровень = " + level.GetStringForSave());
                        }
                    }
                }
            }
        }

        private void Server_NewMyTradeEvent(MyTrade myTrade)
        {
            if (myTrade == null || myTrade.SecurityNameCode != SelectedSecurity.Name) 
            {
                return; // нашей бумаги нет
            }

            RobotWindowVM.Log("MyTrade =  " + GetStringForSave(myTrade));

            foreach (Level level in Levels)
            {
                if (level.Order != null && level.Order.NumberMarket == myTrade.NumberOrderParent)
                {
                    level.AddMyTrade(myTrade);
                    RobotWindowVM.Log("MyTrade = Limit");
                }
                if (level.LimitTake != null && level.LimitTake.NumberMarket == myTrade.NumberOrderParent)
                {
                    level.AddMyTrade(myTrade);
                    RobotWindowVM.Log("MyTrade = Take");
                }
            }
        }

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
            RobotWindowVM.Log(" \n\n Calculate " );
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
                RobotWindowVM.Log("Уровень =  " + levels.Last().GetStringForSave());
            }
            Levels = levels;
            OnPropertyChanged(nameof(Levels));
        }

        private ObservableCollection<string> GetStringPortfolios(IServer server)
        {
            ObservableCollection<string> stringPortfolios = new ObservableCollection<string>();
            if (server == null)
            {
                RobotWindowVM.Log("GetStringPortfolios server == null ");
                return stringPortfolios;
            }
            if (server.Portfolios == null)
            {
                return stringPortfolios;
            }
           
            foreach (Portfolio portf in server.Portfolios)
            {
                RobotWindowVM.Log("GetStringPortfolios  портфель =  " + portf.Number);
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
                        RobotWindowVM.Log("GetStringPortfolios  портфель =  " + portf.Number);
                        return portf;
                    }
                }
            }

            RobotWindowVM.Log("GetStringPortfolios  портфель = null ");
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
                RobotWindowVM.Log("StartSecuritiy  security = null ");
                return;
            }
            Task.Run(() =>
            {
                while (true)
                {
                    var series = Server.StartThisSecurity(security.Name, new TimeFrameBuilder(), security.NameClass);
                    if (series != null)
                    {
                        RobotWindowVM.Log("StartSecuritiy  security = " + series.Security.Name);
                        break;
                    }
                    Thread.Sleep(1000);
                }
            });
        }

        /// <summary>
        ///  формируем строку для ордера
        /// </summary>
        private string GetStringForSave(Order order)
        {
            string str = "";

            str += order.SecurityNameCode + " | ";
            str += order.PortfolioNumber + " | ";
            str += order.TimeCreate + " | ";
            str += order.State + " | ";
            str += order.Side + " | ";
            str += "Объем = " + order.Volume + " | ";
            str += "Цена = " + order.Price + " | ";
            str += "Мой Номер = " + order.NumberUser + " | ";
            str += "Номер биржи = " + order.NumberMarket + " | ";
            //str += order.SecurityNameCode + " | ";

            return str;
        }

   
        private string GetStringForSave(MyTrade myTrade)
        {
            string str = "";

            str += myTrade.SecurityNameCode + " | ";
            str += myTrade.Side + " | ";
            str += "Объем = " + myTrade.Volume + " | ";
            str += "Цена = " + myTrade.Price + " | ";
            str += "NumberOrderParent = " + myTrade.NumberOrderParent + " | ";
            str += "NumberTrade = " + myTrade.NumberTrade + " | ";
  
            return str;
        }

        #endregion
    }
}
