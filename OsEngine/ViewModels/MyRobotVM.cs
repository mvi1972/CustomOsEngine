﻿using Newtonsoft.Json;
using OsEngine.Commands;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.Market.Servers.GateIo.Futures.Response;
using OsEngine.MyEntity;
using OsEngine.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Direction = OsEngine.MyEntity.Direction;

namespace OsEngine.ViewModels
{
    public class MyRobotVM : BaseVM
    {
        public MyRobotVM(string header)
        {
            Header = header;    
            Load();
            ServerMaster.ServerCreateEvent += ServerMaster_ServerCreateEvent;
        }


        public MyRobotVM()
        {
            
        }

        #region Поля =====================================================================================
        /// <summary>
        /// количество  портфелей
        /// </summary>
        int _portfoliosCount = 0;    

        Portfolio _portfolio;

        private List<MyTrade> _myTrades = new List<MyTrade>();


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
                    OnSelectedSecurity?.Invoke(SelectedSecurity.Name); 
                }
            }
        }
        private Security _selectedSecurity =null;

        public ServerType ServerType
        {
            get
            {
                if (Server == null)
                {
                    return _serverType;
                }
                return Server.ServerType;
            }
            set
            {
                if (value != null && value!= _serverType)
                {
                    _serverType = value;
                }

            }
        }
        ServerType _serverType = ServerType.None;
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
        private string _stringportfolio ="";

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
            foreach (Level level in Levels)
            {
                LevelTradeLogicOpen(level);

                LevelTradeLogicClose(level);
            }
        }


        private void LevelTradeLogicOpen(Level level)
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

            if (level.PassVolume
                  && level.PriceLevel != 0
                  && Math.Abs(level.Volume) + level.LimitVolume < Lot)
            {
                if (level.Side == Side.Sell
                    && level.PriceLevel <= borderUp)
                {

                }
                else if (level.Side == Side.Buy
                       && level.PriceLevel >= borderDown)
                {
                    decimal worklot = Lot - Math.Abs(level.Volume) - level.LimitVolume;
                    RobotWindowVM.Log(Header, " Уровень = " + level.GetStringForSave());

                    level.PassVolume = false;
                    Order order = SendOrder(SelectedSecurity, level.PriceLevel, worklot, Side.Buy);
                    level.OrdersForOpen.Add(order); 

                    RobotWindowVM.Log(Header, " Отправляем лимитку  " + GetStringForSave(order));
                }
            }
        }

        private void LevelTradeLogicClose( Level level)
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

            if (level.PassTake && level.PriceLevel != 0)
            {
                if (level.Volume != 0 && level.TakeVolume != Math.Abs(level.Volume))
                {

                    stepLevel = 0;
                    decimal price = 0;
                    Side side = Side.None;

                    if (StepType == StepType.PUNKT)
                    {
                        stepLevel = TakeLevel * SelectedSecurity.PriceStep;
                    }
                    else if (StepType == StepType.PERCENT)
                    {
                        stepLevel = TakeLevel * Price / 100;
                        stepLevel = Decimal.Round(stepLevel, SelectedSecurity.Decimals);
                    }
                    if (level.Volume > 0)
                    {
                        price = level.PriceLevel + stepLevel;
                        side = Side.Sell;
                    }
                    else if (level.Volume < 0)
                    {
                        price = level.PriceLevel - stepLevel;
                        side = Side.Buy;
                    }
                    level.PassTake = false;

                    RobotWindowVM.Log(Header, "Уровень = " + level.GetStringForSave());

                    Order order = SendOrder(SelectedSecurity, price, Math.Abs(level.Volume), side);
                    if (order != null)
                    {
                        level.OrdersForClose.Add(order);
                        RobotWindowVM.Log(Header, "Отправляем Тэйк ордер =  " + GetStringForSave(order));
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
            RobotWindowVM.Log( Header, " \n\n StartStop = " + !IsRun);
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
                    level.CancelAllOrders(Server, GetStringForSave);
                }
            }
        }

        /// <summary>
        ///  подключиться к серверу
        /// </summary>
        private void SubscribeToServer()
        {
            _server.NewMyTradeEvent += Server_NewMyTradeEvent;
            _server.NewOrderIncomeEvent += Server_NewOrderIncomeEvent;
            _server.NewCandleIncomeEvent += Server_NewCandleIncomeEvent;
            _server.NewTradeEvent += Server_NewTradeEvent;
            _server.SecuritiesChangeEvent += _server_SecuritiesChangeEvent;
            _server.PortfoliosChangeEvent += _server_PortfoliosChangeEvent;

            RobotWindowVM.Log( Header, " Подключаемся к серверу = " + _server.ServerType);
        }

        /// <summary>
        ///  отключиться от сервера 
        /// </summary>
        private void UnSubscribeToServer()
        {
            _server.NewMyTradeEvent -= Server_NewMyTradeEvent;
            _server.NewOrderIncomeEvent -= Server_NewOrderIncomeEvent;
            _server.NewCandleIncomeEvent -= Server_NewCandleIncomeEvent;
            _server.NewTradeEvent -= Server_NewTradeEvent;
            _server.SecuritiesChangeEvent -= _server_SecuritiesChangeEvent;
            _server.PortfoliosChangeEvent -= _server_PortfoliosChangeEvent;

            RobotWindowVM.Log( Header, " Отключаемся от сервера = " + _server.ServerType);
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
                && order.ServerType == Server.ServerType
                && order.PortfolioNumber == _portfolio.Number) // 
            {
                bool rec =true;
                if (order.State == OrderStateType.Activ
                    && order.TimeCallBack.AddSeconds(15) < Server.ServerTime) 
                {
                    rec = false;
                }
                if (rec)
                {
                    RobotWindowVM.Log(Header, "NewOrderIncomeEvent = " + GetStringForSave(order));
                }
                if (order.NumberMarket !="")
                {
                    foreach (Level level in Levels)
                    {
                        bool newOrderBool= level.NewOrder(order);

                        if (newOrderBool && rec)
                        {
                            RobotWindowVM.Log(Header, "Уровень = " + level.GetStringForSave());
                        }
                    }
                }
                Save();
            }
        }

        private void Server_NewMyTradeEvent(MyTrade myTrade)
        {
            if (myTrade == null || myTrade.SecurityNameCode != SelectedSecurity.Name) 
            {
                return; // нашей бумаги нет
            }
            foreach (MyTrade trade in _myTrades)
            {
                if (trade.NumberTrade == myTrade.NumberTrade)
                {
                    return ;
                }
            }
            _myTrades.Add(myTrade);

            RobotWindowVM.Log( Header, "MyTrade =  " + GetStringForSave(myTrade));

            foreach (Level level in Levels)
            {
                 level.AddMyTrade(myTrade);
            }
            Save();
        }

        /// <summary>
        /// расчитывает уровни 
        /// </summary>
        void Calculate( object o)
        {
            ObservableCollection<Level> levels = new ObservableCollection<Level>();

            decimal currBuyPrice = StartPoint;
            decimal currSellPrice = StartPoint;

            if (CountLevels <=0 || SelectedSecurity == null )
            {
                return;
            }
            RobotWindowVM.Log( Header, " \n\n Calculate " );
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
                RobotWindowVM.Log( Header, "Уровень =  " + levels.Last().GetStringForSave());
            }
            Levels = levels;
            OnPropertyChanged(nameof(Levels));

            Save();
        }

        private void _server_PortfoliosChangeEvent(List<Portfolio> portfolios)
        {
            if (portfolios == null 
                || _portfoliosCount >= portfolios.Count) // нет новых портфелей 
            {
                return; 
            }
            _portfoliosCount = portfolios.Count;

            StringPortfolios = GetStringPortfolios(_server); // грузим портфели

            OnPropertyChanged(nameof(StringPortfolios));

            if (StringPortfolios != null && StringPortfolios.Count > 0)
            {
                if (StringPortfolio == "")
                {
                    StringPortfolio = StringPortfolios[0];
                }
                for (int i = 0; i < portfolios.Count; i++)
                {
                    if (portfolios[i].Number == StringPortfolio)
                    {
                        _portfolio = portfolios[i];
                    }
                }
            }
            OnPropertyChanged(nameof(StringPortfolios));
        }

        public ObservableCollection<string> GetStringPortfolios(IServer server)
        {
            ObservableCollection<string> stringPortfolios = new ObservableCollection<string>();
            if (server == null)
            {
                RobotWindowVM.Log( Header, "GetStringPortfolios server == null ");
                return stringPortfolios;
            }
            if (server.Portfolios == null)
            {
                return stringPortfolios;
            }
           
            foreach (Portfolio portf in server.Portfolios)
            {
                RobotWindowVM.Log( Header, "GetStringPortfolios  портфель =  " + portf.Number);
                stringPortfolios.Add(portf.Number);
            }
            return stringPortfolios;  
        }

        private Portfolio GetPortfolio(string number)
        {
            if (Server != null && Server.Portfolios != null)
            {
                foreach (Portfolio portf in Server.Portfolios)
                {
                    if (portf.Number == number)
                    {
                        RobotWindowVM.Log( Header, " Выбран портфель =  " + portf.Number);
                        return portf;
                    }
                }
            }

            RobotWindowVM.Log( Header, "GetStringPortfolios  портфель = null ");
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
                RobotWindowVM.Log( Header, "StartSecuritiy  security = null ");
                return;
            }
            Task.Run(() =>
            {
                while (true)
                {
                    var series = Server.StartThisSecurity(security.Name, new TimeFrameBuilder(), security.NameClass);
                    if (series != null)
                    {
                        RobotWindowVM.Log( Header, "StartSecuritiy  security = " + series.Security.Name);
                        Save();
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

        /// <summary>
        /// сохранение параметров для вкладок
        /// </summary>
        private void Save()
        {
            if (!Directory.Exists(@"Parametrs\Tabs"))
            {
                Directory.CreateDirectory(@"Parametrs\Tabs");
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(@"Parametrs\Tabs\param_" + Header + ".txt", false))
                {
                    writer.WriteLine(Header);
                    writer.WriteLine(ServerType);
                    writer.WriteLine(StringPortfolio);
                    writer.WriteLine(StartPoint);
                    writer.WriteLine(CountLevels);
                    writer.WriteLine(Direction);
                    writer.WriteLine(Lot);
                    writer.WriteLine(StepType);
                    writer.WriteLine(StepLevel);
                    writer.WriteLine(TakeLevel);
                    writer.WriteLine(MaxActiveLevel);
                    writer.WriteLine(PriceAverege);
                    writer.WriteLine(Accum);

                    writer.WriteLine(JsonConvert.SerializeObject(Levels));

                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                RobotWindowVM.Log( Header, " Ошибка сохранения параметров = " + ex.Message);

            }
        }
        /// <summary>
        /// загрузка во вкладку параметров 
        /// </summary>
        private void Load()
        {
            if (!Directory.Exists(@"Parametrs\Tabs"))
            {
                return;
            }
            string servType ="" ;
            try
            {
                using (StreamReader reader = new StreamReader(@"Parametrs\Tabs\param_" + Header + ".txt"))
                {
                    Header = reader.ReadLine(); // загружаем заголовок
                    servType = reader.ReadLine(); // загружаем название сервера
                    StringPortfolio = reader.ReadLine();  // загружаем бумагу 
                    StartPoint = GetDecimalForString(reader.ReadLine());
                    CountLevels = (int)GetDecimalForString(reader.ReadLine());

                    Direction direct = Direction.BUY;
                    if (Enum.TryParse(reader.ReadLine(), out direct))
                    {
                        Direction = direct;
                    }

                    Lot = GetDecimalForString(reader.ReadLine());

                    StepType step = StepType.PUNKT;
                    if (Enum.TryParse(reader.ReadLine(), out direct))
                    {
                        StepType = step;
                    }

                    StepLevel = GetDecimalForString(reader.ReadLine());
                    TakeLevel = GetDecimalForString(reader.ReadLine());
                    MaxActiveLevel = (int)GetDecimalForString(reader.ReadLine());
                    PriceAverege = GetDecimalForString(reader.ReadLine());
                    Accum = GetDecimalForString(reader.ReadLine());

                    Levels = JsonConvert.DeserializeAnonymousType(reader.ReadLine(), new ObservableCollection<Level>());

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                RobotWindowVM.Log( Header, " Ошибка выгрузки параметров = " + ex.Message);
            }
            StartServer(servType);

        }
        /// <summary>
        ///  преобразует строку из файла сохранения в дефимал 
        /// </summary>
        private decimal GetDecimalForString(string str)
        {
            decimal value = 0;
            decimal.TryParse(str, out value);
            return value;   
        }

        void StartServer (string servType)
        {
            if (servType == "" || servType == "null")
            {
                return;
            }
            ServerType type = ServerType.None;
            if (Enum.TryParse (servType, out type))
            {
                ServerType = type;  
                ServerMaster.SetNeedServer(type);
            }
        }


        private void ServerMaster_ServerCreateEvent(IServer server)
        {
            if (server.ServerType == ServerType)
            {
                Server = server;
            }
        }

        private void _server_SecuritiesChangeEvent(List<Security> securities)
        {
            for (int i = 0; i < securities.Count; i++)
            {
                if (securities[i].Name == Header)
                {
                    SelectedSecurity = securities[i];   
                    StartSecuritiy(securities[i]);
                    break;
                }
            }
        }


        #endregion

        #region ============================================События============================================

        public delegate void selectedSecurity(string name);
        public event selectedSecurity OnSelectedSecurity;
        #endregion

    }
}
