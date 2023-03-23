using Newtonsoft.Json;
using OkonkwoOandaV20.TradeLibrary.DataTypes.Pricing;
using OsEngine.Commands;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.MyEntity;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Lifetime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Action = OsEngine.MyEntity.Action;
using Level = OsEngine.MyEntity.Level;
using Direction = OsEngine.MyEntity.Direction;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Collections.Concurrent;
using ControlzEx.Theming;
using OsEngine.Market.Servers.Binance.Futures;
using System.Windows.Controls.Primitives;

namespace OsEngine.ViewModels
{
    
    public class GridRobotVM : BaseVM, IRobotVM
    {
        /// <summary>
        /// конструктор для созданого и сохранеенного робота
        /// </summary>
        public GridRobotVM(string header, int numberTab)
        {
            string[]str = header.Split('=');
            NumberTab = numberTab;
            Header = str[0];    
            LoadParamsBot(header);
            DesirializerDictionaryOrders();
            ServerMaster.ServerCreateEvent += ServerMaster_ServerCreateEvent;
            
         }
        /// <summary>
        /// конструктор для нового робота 
        /// </summary>
        public GridRobotVM(int numberTab)
        {
            NumberTab = numberTab;
        }

        #region Свойства ================================================================================== 


        /// <summary>
        /// словарь  ордеров на бирже ТЕСТ 
        /// </summary>
        public ConcurrentDictionary<string, Order> DictionaryOrdersActiv = new ConcurrentDictionary<string, Order>();

        /// <summary>
        /// список портфелей 
        /// </summary>
        public ObservableCollection<string> StringPortfolios { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// колекция уровней 
        /// </summary>
        public ObservableCollection<Level> Levels { get; set; } = new ObservableCollection<Level>() ;

        //string str = "Levels колекция  = " + Levels.Count;
        //Debug.WriteLine(str);

        /// <summary>
         /// заголовок робота 
         /// </summary>
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

        public NameStrat NameStrat
        { 
            get => _nameStrat;
            set
            {
                _nameStrat=value;
                OnPropertyChanged(nameof(NameStrat));   
            }
        }
        private NameStrat _nameStrat = NameStrat.GRID;

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
                    if (SelectedSecurity == null)
                    {
                        StartSecuritiy(SelectedSecurity);
                    }
                }
            }
        }
        private Security _selectedSecurity =null;

        /// <summary>
        /// базовый класс пары
        /// </summary>
        public Security NameClass
        {
            get => _nameClass;
            set
            {
                _nameClass = value;
                OnPropertyChanged(nameof(NameClass));
                OnPropertyChanged(nameof(Header));
                if (NameClass != null)
                {
                    // string klass = SelectedSecurity.NameClass;
                    OnSelectedSecurity?.Invoke(SelectedSecurity.NameClass);
                }
            }
        }
        private Security _nameClass = null;

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

        /// <summary>
        /// название портфеля (счета)
        /// </summary>
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
        /// Объем выбраной бумаги на бирже
        /// </summary>
        public decimal SelectSecurBalans
        {
            get => _selectSecurBalans;
            set
            {
                _selectSecurBalans = value;
                OnPropertyChanged(nameof(SelectSecurBalans));
            }
        }
        private decimal _selectSecurBalans;
   
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
        /// свойство список направления сделок
        /// </summary>
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
        /// <summary>
        /// список типов расчета шага 
        /// </summary>
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
        public decimal AllPositionsCount
        {
            get => _allPositionsCount;
            set
            {
                _allPositionsCount = value;
                OnPropertyChanged(nameof(AllPositionsCount));
            }
        }
        private decimal _allPositionsCount;

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
                if (IsRun)
                {
                    IsReadOnly = true;
                    IsEnabled = false;
                }
                else
                {
                    IsReadOnly = false;
                    IsEnabled = true;   
                }
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
        /// <summary>
        /// 
        /// </summary>
        public bool IsChekCurrency
        {
            get => _isChekCurrency;
            set
            {
                _isChekCurrency = value;
                OnPropertyChanged(nameof(IsChekCurrency));
            }
        }
        private bool _isChekCurrency;

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set
            {
                _isReadOnly = value;
                OnPropertyChanged(nameof(IsReadOnly));
            }
        }
        private bool _isReadOnly;
        /// <summary>
        /// отключение возможности редактирования комбобокса направления сделки
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        private bool _isEnabled;

        /// <summary>
        /// расчетная точка стопов для шота
        /// </summary>
        public decimal StopShort
        {
            get => _stopShort;
            set
            {
                _stopShort = value;
                OnPropertyChanged(nameof(StopShort));
            }
        }
        private decimal _stopShort=0;

        /// <summary>
        /// расчетная точка стопов для лонга
        /// </summary>
        public decimal StopLong
        {
            get => _stopLong;
            set
            {
                _stopLong = value;
                OnPropertyChanged(nameof(StopLong));
            }
        }
        private decimal _stopLong=0;

        #endregion

        #region Поля =======================================================================================

        //List<Order> listForSave;

        decimal _bestBid;
        decimal _bestAsk;

        /// <summary>
        /// количество  портфелей
        /// </summary>
        int _portfoliosCount = 0;

        Portfolio _portfolio;

        public List<Side> Sides { get; set; } = new List<Side>()
        {
            Side.Buy,
            Side.Sell,
        };

        public int NumberTab=0;

        CultureInfo CultureInfo = new CultureInfo("ru-RU");
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

        private DelegateCommand commandClosePositions;
        public DelegateCommand CommandClosePositions
        {
            get
            {
                if (commandClosePositions ==null)
                {
                    commandClosePositions = new DelegateCommand(ClosePosition);
                }
                return commandClosePositions;
            }
        }

        private DelegateCommand commandAddRow;
        public DelegateCommand CommandAddRow
        {
            get
            {
                if (commandAddRow == null)
                {
                    commandAddRow = new DelegateCommand(AddRow);
                }
                return commandAddRow;
            }
        }
        private DelegateCommand commandTestApi;
        public DelegateCommand CommandTestApi
        {
            get
            {
                if (commandTestApi == null)
                {
                    commandTestApi = new DelegateCommand(TestApi);
                }
                return commandTestApi;
            }
        }
        

        #endregion

        #region все Методы =====================================================================================

        #region ===== логика ======


        /// <summary>
        /// расчитывает уровни (цены открвтия и профитов)
        /// </summary>
        void Calculate(object o)
        {
            
            decimal volume = 0;
            decimal stepTake = 0;

            foreach (Level level in Levels)
            {
                volume += Math.Abs(level.Volume);
            }
            if (volume > 0)
            {
                MessageBoxResult result = MessageBox.Show(" Есть открытые позиции! \n Всеравно пресчитать? ", " ВНИМАНИЕ !!! ",
                    MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            ObservableCollection<Level> levels = new ObservableCollection<Level>();

            decimal currBuyPrice = StartPoint;
            decimal currSellPrice = StartPoint;

            if (CountLevels <= 0 || SelectedSecurity == null)
            {
                return;
            }
            RobotWindowVM.Log(Header, " \n\n Пересчитываем уровни  ");
            for (int i = 0; i < CountLevels; i++)
            {
                Level levelBuy = new Level() { Side = Side.Buy };
                Level levelSell = new Level() { Side = Side.Sell };

                if (StepType == StepType.PUNKT)
                {
                    currBuyPrice -= StepLevel * SelectedSecurity.PriceStep;
                    currSellPrice += StepLevel * SelectedSecurity.PriceStep;

                    stepTake = TakeLevel * SelectedSecurity.PriceStep;
                }
                else if (StepType == StepType.PERCENT)
                {
                    currBuyPrice -= StepLevel * currBuyPrice / 100;
                    currBuyPrice = Decimal.Round(currBuyPrice, SelectedSecurity.Decimals);

                    currSellPrice += StepLevel * currSellPrice / 100;
                    currSellPrice = Decimal.Round(currSellPrice, SelectedSecurity.Decimals);

                    stepTake = TakeLevel * currBuyPrice / 100;
                    stepTake = Decimal.Round(stepTake, SelectedSecurity.Decimals);

                }
                levelSell.PriceLevel = currSellPrice;
                levelBuy.PriceLevel = currBuyPrice;

                if (Direction == Direction.BUY || Direction == Direction.BUYSELL)
                {
                    levelBuy.TakePrice = levelBuy.PriceLevel + stepTake;
                    levels.Add(levelBuy);
                }
                if (Direction == Direction.SELL || Direction == Direction.BUYSELL)
                {
                    levelSell.TakePrice = levelSell.PriceLevel - stepTake;
                    levels.Insert(0, levelSell);
                }
                RobotWindowVM.Log(Header, "Уровень =  " + levels.Last().GetStringForSave());
            }
            Levels = levels;
            OnPropertyChanged(nameof(Levels));

            CalculateStop();

            SaveParamsBot();
        }

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
                    IsChekCurrency = true;
                }
                else IsChekCurrency = false;
            }
        }

        /// <summary>
        /// Начать получать данные по бумге
        /// </summary> 
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
                        // DesirializerLevels();
                        SaveParamsBot();
                        break;
                    }
                    Thread.Sleep(1000);
                }
            });
        }

        /// <summary>
        /// запускаем сервер
        /// </summary>
        void StartServer(string servType)
        {
            if (servType == "" || servType == "null")
            {
                return;
            }
            ServerType type = ServerType.None;
            if (Enum.TryParse(servType, out type))
            {
                ServerType = type;
                ServerMaster.SetNeedServer(type);
                // LoadParamsBot(Header);
            }
        }

        /// <summary>
        /// добавить строку уровня
        /// </summary>
        private void AddRow(object o)
        {
            if (IsRun)
            {
                MessageBox.Show("Перейдите в режим редактирования!  ");
            }
            Levels.Add(new Level());
            //DesirializerDictionaryOrders();
        }
        private void TestApi(object o)
        {
            GetOrderStatusOnBoard();
        }

        /// <summary>
        /// закрываем все позиции 
        /// </summary>
        private void ClosePosition(object o)
        {
            MessageBoxResult resut = MessageBox.Show("Закрыть все позиции по " + Header, " Уверен? ", MessageBoxButton.YesNo);
            if (resut == MessageBoxResult.Yes)
            {
                IsRun = false;
                foreach (Level level in Levels)
                {
                    level.CancelAllOrders(Server, GetStringForSave);

                    LevelTradeLogicClose(level, Action.CLOSE);
                }
            }
        }

        /// <summary>
        /// проверка стопа
        /// </summary>
        private void ExaminationStop()
        {
            if (SelectSecurBalans == 0) return;

            if (StopLong != 0 && Price != 0)
            {
                if (Price < StopLong && Direction == Direction.BUY ||
                    Price < StopLong && Direction == Direction.BUYSELL)
                {
                    IsRun = false;
                    StopLong = 0;
                    string str = " Сработал СТОП лонга \n IsRun = false , сохранились \n ";
                    Debug.WriteLine(str);
                    foreach (Level level in Levels)
                    {
                        level.CancelAllOrders(Server, GetStringForSave);
                        string str3 = "level long = " + level.PriceLevel;
                        Debug.WriteLine(str3);

                        string str2 = "Всего уровней = " + Levels.Count;
                        Debug.WriteLine(str2);

                        LevelTradeLogicClose(level, Action.STOP);
                        RobotWindowVM.Log(Header, " Сработал СТОП ЛОНГ ");
                        StopLong = 0;
                    }
                    return;
                }
            }
            if (StopShort != 0 && Price != 0)
            {
                if (Price > StopShort && Direction == Direction.SELL ||
                    Price > StopShort && Direction == Direction.BUYSELL)
                {
                    IsRun = false;
                    SaveParamsBot();
                    string str = " Сработал СТОП, IsRun = false , сохранились \n ";
                    Debug.WriteLine(str);
                    StopShort = 0;
                    foreach (Level level in Levels)
                    {
                        level.CancelAllOrders(Server, GetStringForSave);

                        RobotWindowVM.Log(Header, "ExaminationStop \n " + " Сработал СТОП ШОРТА ");
                        string str4 = "level Short = " + level.PriceLevel;
                        Debug.WriteLine(str4);

                        string str2 = "Всего уровней = " + Levels.Count;
                        Debug.WriteLine(str2);

                        LevelTradeLogicClose(level, Action.STOP);
                        StopShort = 0;
                    }
                }
            }
        }

        /// <summary>
        /// после трейда или ррдера с бижжи по бумаге гоняет по уровням  логику отправки ореров на открытип и закрытие 
        /// </summary>
        private void TradeLogic()
        {
            foreach (Level level in Levels)
            {
                LevelTradeLogicOpen(level);

                LevelTradeLogicClose(level, Action.TAKE);
            }
        }

        /// <summary>
        /// расчитать цену стопов
        /// </summary>
        private void CalculateStop()
        {
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
            StopLong = StartPoint - stepLevel * (CountLevels + 1);
            StopShort = StartPoint + stepLevel * (CountLevels + 1);
        }

        /// <summary>
        /// логика отправки  лимит ордера на открытия на уровнях
        /// </summary>
        private void LevelTradeLogicOpen(Level level)
        {
            if (IsRun == false || SelectedSecurity == null)
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
                if ((level.Side == Side.Buy && level.PriceLevel >= borderDown)
                   || (level.Side == Side.Sell && level.PriceLevel <= borderUp))
                {
                    decimal lot = CalcWorkLot(Lot, level.PriceLevel);

                    decimal worklot = lot - Math.Abs(level.Volume) - level.LimitVolume;
                    RobotWindowVM.Log(Header, "LevelTradeLogicOpen ");
                    RobotWindowVM.Log(Header, " Уровень = " + level.GetStringForSave());
                    RobotWindowVM.Log(Header, "Рабочий лот =  " + worklot);
                    RobotWindowVM.Log(Header, "IsChekCurrency =  " + IsChekCurrency);

                    level.PassVolume = false;

                    if (IsChekCurrency && Lot > 6 || !IsChekCurrency)
                    {
                        if (worklot == 0)
                        {
                            level.PassVolume = true;
                            LevelTradeLogicOpen(level);
                            RobotWindowVM.Log(Header, " worklot = 0 ретурн ");
                            return;
                        }
                        Order order = SendLimitOrder(SelectedSecurity, level.PriceLevel, worklot, level.Side);
                        if (order != null)
                        {
                            level.OrdersForOpen.Add(order);

                            RobotWindowVM.Log(Header, " Отправляем лимитку в level.OrdersForOpen " + GetStringForSave(order));
                            Thread.Sleep(10);
                        }
                        else
                        {
                            level.PassVolume = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// логика отправки ордеров на закрытия на уровнях
        /// </summary>
        private void LevelTradeLogicClose(Level level, Action action)
        {
            decimal stepLevel = 0;
            if (action == Action.STOP)
            {
                Side side = Side.None;

                if (level.Volume > 0)
                {
                    side = Side.Sell;
                }
                else if (level.Volume < 0)
                {
                    side = Side.Buy;
                }

                decimal worklot = Math.Abs(level.Volume);// -level.TakeVolume
                if (IsChekCurrency && worklot * level.PriceLevel > 6 || !IsChekCurrency)
                {
                    Order order = SendMarketOrder(SelectedSecurity, Price, worklot, side);
                    if (order != null)
                    {
                        if (order.State != OrderStateType.Activ ||
                            order.State != OrderStateType.Patrial ||
                            order.State != OrderStateType.Pending)
                        {
                            level.PassVolume = false;
                            level.OrdersForClose.Add(order);
                            RobotWindowVM.Log(Header, "Отправлен Маркет ордер на закрытие \n  " + GetStringForSave(order));
                        }
                    }
                    else level.PassVolume = true;
                }
                else if (worklot * level.PriceLevel <= 6 && worklot != 0)
                {
                    RobotWindowVM.Log(Header, "ВНИМАНИЕ объём МАРКЕТ ордера <= 6 $ \n ордер не отправлен\n" +
                        "action == Action.STOP \n " +
                        " worklot  =  " + worklot);
                    level.PassVolume = true;
                    LevelTradeLogicClose(level, Action.STOP);
                    return;
                }
            }

            if (action == Action.CLOSE)
            {
                decimal price = 0;
                Side side = Side.None;

                if (level.Volume > 0)
                {
                    price = _bestAsk;

                    side = Side.Sell;
                }
                else if (level.Volume < 0)
                {
                    price = _bestBid;

                    side = Side.Buy;
                }
                level.PassTake = false;

                decimal worklot = Math.Abs(level.Volume) - level.TakeVolume;
                if (IsChekCurrency && worklot * level.PriceLevel > 6 || !IsChekCurrency)
                {
                    Order order = SendLimitOrder(SelectedSecurity, price, worklot, side);
                    if (order != null)
                    {
                        if (price == 0)
                        {
                            level.PassTake = true;
                            LevelTradeLogicClose(level, Action.CLOSE);
                            return;
                        }

                        if (order.State != OrderStateType.Activ ||
                            order.State != OrderStateType.Patrial ||
                            order.State != OrderStateType.Pending)
                        {
                            level.PassVolume = false;
                            level.OrdersForClose.Add(order);
                            RobotWindowVM.Log(Header,
                            "Отправляем  Лимит ордер на закрытие по цене посленего трейда "
                             + GetStringForSave(order));
                        }
                    }
                    else
                    {
                        level.PassVolume = true;
                    }
                }
                else if (worklot * level.PriceLevel <= 6 && worklot != 0)
                {
                    RobotWindowVM.Log(Header, "ВНИМАНИЕ ордер <= 6 $ \n " +
                        "action == Action.CLOSE ордер не отправлен \n" +
                        " worklot  =  " + worklot);
                    level.PassVolume = true;
                    LevelTradeLogicClose(level, Action.CLOSE);
                    return;
                }
            }

            if (IsRun == false || SelectedSecurity == null && action == Action.TAKE)
            {
                return;
            }

            // выбрана бумага и робот включен следует логика выставления тейков 

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
                        if (action == Action.TAKE)
                        {
                            price = level.TakePrice;
                        }
                        side = Side.Sell;
                    }
                    else if (level.Volume < 0)
                    {
                        if (action == Action.TAKE)
                        {
                            price = level.TakePrice;
                        }
                        side = Side.Buy;
                    }
                    level.PassTake = false;

                    RobotWindowVM.Log(Header, "Уровень = " + level.GetStringForSave());

                    decimal worklot = Math.Abs(level.Volume) - level.TakeVolume;
                    RobotWindowVM.Log(Header, "Рабочий лот =  " + worklot);
                    RobotWindowVM.Log(Header, "IsChekCurrency =  " + IsChekCurrency);

                    if (IsChekCurrency && worklot * level.PriceLevel > 6 || !IsChekCurrency)
                    {
                        Order order = SendLimitOrder(SelectedSecurity, price, worklot, side);
                        if (order != null)
                        {
                            level.OrdersForClose.Add(order);
                            RobotWindowVM.Log(Header, "Отправляем Тэйк ордер =  " + GetStringForSave(order));
                        }
                        else
                        {
                            level.PassTake = true;
                        }
                    }
                    else if (worklot * level.PriceLevel <= 6 && worklot != 0)
                    {
                        RobotWindowVM.Log(Header, "ВНИМАНИЕ action ТЭЙК ордер меньше 6 $ не отрпавлен \n" +
                            " worklot  =  " + worklot);
                        level.PassTake = true;
                        LevelTradeLogicClose(level, Action.TAKE); ;
                    }
                }
            }
        }

        /// <summary>
        /// раcчет позиций 
        /// </summary>
        private void CalculateMargin()
        {
            if (Levels.Count == 0 || SelectedSecurity == null) return;

            decimal volume = 0;
            decimal accum = 0;
            decimal margine = 0;
            decimal averagePrice = 0;

            foreach (Level level in Levels)
            {
                if (level.Volume != 0)
                {
                    averagePrice = (level.OpenPrice * level.Volume + averagePrice * volume)
                        / (level.Volume + volume);

                    level.Margin = (Price - level.OpenPrice) * level.Volume * SelectedSecurity.Lot;
                }
                volume += level.Volume;
                accum += level.Accum;
                margine += level.Margin;
            }

            AllPositionsCount = volume;
            PriceAverege = averagePrice;
            Accum = accum;
            VarMargine = margine;
            Total = Accum + VarMargine;
            PriceAverege = averagePrice;
        }

        private decimal CalcWorkLot(decimal lot, decimal price)
        {
            decimal workLot = lot;
            if (IsChekCurrency)
            {
                workLot = lot / price;
            }
            workLot = decimal.Round(workLot, SelectedSecurity.DecimalsVolume);

            return workLot;
        }

        /// <summary>
        ///  отправить лимитный оредер на биржу 
        /// </summary>
        private Order SendLimitOrder(Security sec, decimal prise, decimal volume, Side side)
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
            RobotWindowVM.Log(Header, "SendLimitOrder\n " + " отправляем лимитку на биржу\n" + GetStringForSave(order));
            RobotWindowVM.SendStrTextDb(" SendLimitOrder " + order.NumberUser);
            Server.ExecuteOrder(order);


            return order;
        }

        /// <summary>
        ///  отправить Маркетный оредер на биржу 
        /// </summary>
        private Order SendMarketOrder(Security sec, decimal prise, decimal volume, Side side)
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
                TypeOrder = OrderPriceType.Market,
                NumberUser = NumberGen.GetNumberOrder(StartProgram.IsOsTrader),
                SecurityNameCode = sec.Name,
                SecurityClassCode = sec.NameClass,
            };
            RobotWindowVM.Log(Header, "SendMarketOrder\n " + " отправляем маркет на биржу\n" + GetStringForSave(order));
            RobotWindowVM.SendStrTextDb(" SendMarketOrder " + order.NumberUser);
            Server.ExecuteOrder(order);

            return order;
        }

        private void StartStop(object o)
        {

            RobotWindowVM.Log(Header, " \n\n StartStop = " + !IsRun);
            Thread.Sleep(300);

            SaveParamsBot();

            IsRun = !IsRun;

            if (IsRun)
            {

                foreach (Level level in Levels)
                {
                    level.SetVolumeStart();
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
                Thread.Sleep(3000);
                bool flag = true;
                foreach (Level level in Levels)
                {
                    if (flag && level.LimitVolume != 0
                        || level.TakeVolume != 0)
                    {
                        flag = false;
                        break;
                    }
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
            _server.NewBidAscIncomeEvent += _server_NewBidAscIncomeEvent;
            _server.ConnectStatusChangeEvent += _server_ConnectStatusChangeEvent;

            RobotWindowVM.Log(Header, " Подключаемся к серверу = " + _server.ServerType);
        }

        #endregion

        #region  ===== сервисные ======

        /// <summary>
        /// запросить статус ордеров на бирже
        /// </summary>
        private void GetOrderStatusOnBoard()
        {
            List<Order> odersInLev =  GetOrdersInLevels();
            for (int i = 0; i < odersInLev.Count; i++)
            {
                Order ord = odersInLev[i];
                Server.GetStatus(ord);
            }            
        }
        /// <summary>
        /// Взять ордера на уровнях
        /// </summary>
        /// <param name="namSecur"> название бумаги </param>
        /// <returns> спсиок ордеров на уровнях </returns>
        private List<Order> GetOrdersInLevels()
        {
            List<Order> ordersInLevels = new List<Order>();
            foreach (Level level in Levels)
            {
                for (int i = 0; i < level.OrdersForOpen.Count; i++)
                {
                    Order order = level.OrdersForOpen[i];
                    ordersInLevels.Add(order);
                }
                for (int i = 0; i < level.OrdersForClose.Count; i++)
                {
                    Order order = level.OrdersForClose[i];
                    ordersInLevels.Add(order);
                }
            }            
            return ordersInLevels;
        }

        ///<summary>
        /// взять текущий объем на бирже выбаной  бумаги
        /// </summary>
        private void GetBalansSecur()
        {
            List<Portfolio> portfolios = new List<Portfolio>();
            if (Server.Portfolios != null)
            {
                portfolios = Server.Portfolios;
            }
            if (portfolios.Count > 0 && portfolios != null
                && _selectedSecurity != null)
            {
                int count = portfolios[0].GetPositionOnBoard().Count;
                string nam = SelectedSecurity.Name;
                string suf = "_BOTH";
                string SecurName = nam + suf;
                for (int i = 0; i < count; i++)
                {
                    string seсurCode = portfolios[0].GetPositionOnBoard()[i].SecurityNameCode;
                    if (seсurCode == SecurName)
                    {
                        decimal d = portfolios[0].GetPositionOnBoard()[i].ValueCurrent;
                        SelectSecurBalans = d; // отправка значения в свойство
                    }
                }
            }

            //decimal balans = portfolios[0].GetPositionOnBoard()[0].Find(pos =>
            //    pos.SecurityNameCode == _securName).ValueCurrent;
            //    return balans;

        }

        /// <summary>
        ///  формируем строку для сохранения ордера
        /// </summary>
        private string GetStringForSave(Order order)
        {
            string str = "";
            str += "ордер = \n";
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

        /// <summary>
        ///  формируем строку для сохранения моих трейдов 
        /// </summary>
        private string GetStringForSave(MyTrade myTrade)
        {
            string str = "";
            str += "мой трейд = \n";
            str += myTrade.SecurityNameCode + " | ";
            str += myTrade.Side + " | ";
            str += "Объем = " + myTrade.Volume + " | ";
            str += "Цена = " + myTrade.Price + " | ";
            str += "NumberOrderParent = " + myTrade.NumberOrderParent + " | ";
            str += "NumberTrade = " + myTrade.NumberTrade + " | ";

            return str;
        }

        /// <summary>
        /// сохранение параметров робота
        /// </summary>
        private void SaveParamsBot()
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
                    writer.WriteLine(ServerType);
                    writer.WriteLine(StringPortfolio);

                    writer.WriteLine(StopShort);
                    writer.WriteLine(StartPoint);
                    writer.WriteLine(StopLong);

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

                    writer.WriteLine(IsChekCurrency);
                    writer.WriteLine(IsRun);

                    writer.Close();
                    RobotWindowVM.Log(Header, "SaveParamsBot  \n cохраненили  параметры ");
                }
            }
            catch (Exception ex)
            {
                RobotWindowVM.Log(Header, " Ошибка сохранения параметров = " + ex.Message);
            }
        }
        /// <summary>
        /// загрузка во вкладку параметров из файла сохрана
        /// </summary>
        private void LoadParamsBot(string name)
        {
            if (!Directory.Exists(@"Parametrs\Tabs"))
            {
                return;
            }
            RobotWindowVM.Log(Header, " LoadParamsBot \n загрузили параметры ");
            string servType = "";
            try
            {
                using (StreamReader reader = new StreamReader(@"Parametrs\Tabs\param_" + name + ".txt"))
                {
                    Header = reader.ReadLine(); // загружаем заголовок
                    servType = reader.ReadLine(); // загружаем название сервера
                    StringPortfolio = reader.ReadLine();  // загружаем бумагу 

                    StopShort = GetDecimalForString(reader.ReadLine());
                    StartPoint = GetDecimalForString(reader.ReadLine());
                    StopLong = GetDecimalForString(reader.ReadLine());

                    CountLevels = (int)GetDecimalForString(reader.ReadLine());

                    Direction direct = Direction.BUY;
                    if (Enum.TryParse(reader.ReadLine(), out direct))
                    {
                        Direction = direct;
                    }

                    Lot = GetDecimalForString(reader.ReadLine());

                    StepType step = StepType.PUNKT;
                    if (Enum.TryParse(reader.ReadLine(), out step))
                    {
                        StepType = step;
                    }

                    StepLevel = GetDecimalForString(reader.ReadLine());
                    TakeLevel = GetDecimalForString(reader.ReadLine());
                    MaxActiveLevel = (int)GetDecimalForString(reader.ReadLine());
                    PriceAverege = GetDecimalForString(reader.ReadLine());
                    Accum = GetDecimalForString(reader.ReadLine());

                    Levels = JsonConvert.DeserializeAnonymousType(reader.ReadLine(), new ObservableCollection<Level>());

                    bool check = false;
                    if (bool.TryParse(reader.ReadLine(), out check))
                    {
                        IsChekCurrency = check;
                    }
                    bool run = false;
                    if (bool.TryParse(reader.ReadLine(), out run))
                    {
                        IsRun = run;
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                RobotWindowVM.Log(Header, " Ошибка выгрузки параметров = " + ex.Message);
            }
            StartServer(servType);

        }
        /// <summary>
        ///  преобразует строку из файла сохранения в децимал 
        /// </summary>
        private decimal GetDecimalForString(string str)
        {
            decimal value = 0;
            decimal.TryParse(str, out value);
            return value;
        }
        /// <summary>
        /// является ли уровень активным 
        /// </summary>
        private bool ActiveLevelAre()
        {
            if (Levels == null || Levels.Count == 0) return false;
            foreach (Level level in Levels)
            {
                if (level.StatusLevel == PositionStatus.OPENING ||
                    level.StatusLevel == PositionStatus.OPEN ||
                    level.StatusLevel == PositionStatus.CLOSING)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// сериализация словаря  ордеров
        /// </summary>
        public void SerializerDictionaryOrders()
        {
            DataContractJsonSerializer DictionaryOrdersSerialazer 
              = new DataContractJsonSerializer(typeof(ConcurrentDictionary<string, ConcurrentDictionary<string, Order>>));
            using (var file = new FileStream("DictionaryAllOrders.json", FileMode.OpenOrCreate))
            {
                DictionaryOrdersSerialazer.WriteObject(file, RobotWindowVM.Orders);
            }
        }

        /// <summary>
        ///  десериализация словаря ордеров 
        /// </summary>
        public void DesirializerDictionaryOrders()
        {
            return;
            DataContractJsonSerializer DictionaryOrdersSerialazer 
                = new DataContractJsonSerializer(typeof(ConcurrentDictionary<string, ConcurrentDictionary<string, Order>>));
            if (!File.Exists("DictionaryAllOrders.json"))
            {
                return;
            }
            using (var file = new FileStream("DictionaryAllOrders.json", FileMode.Open))
            {
                ConcurrentDictionary<string, ConcurrentDictionary<string, Order>> DictionaryOrders 
                 = DictionaryOrdersSerialazer.ReadObject(file) as ConcurrentDictionary<string, ConcurrentDictionary<string, Order>>;
                if (DictionaryOrders != null)
                {
                    RobotWindowVM.Orders = DictionaryOrders;
                }
            }
        }
  
        /// <summary>
        /// берет названия кошельков (бирж)
        /// </summary>
        public ObservableCollection<string> GetStringPortfolios(IServer server)
        {
            ObservableCollection<string> stringPortfolios = new ObservableCollection<string>();
            if (server == null)
            {
                RobotWindowVM.Log(Header, "GetStringPortfolios server == null ");
                return stringPortfolios;
            }
            if (server.Portfolios == null)
            {
                return stringPortfolios;
            }

            foreach (Portfolio portf in server.Portfolios)
            {
                RobotWindowVM.Log(Header, "GetStringPortfolios  портфель =  " + portf.Number);
                stringPortfolios.Add(portf.Number);
            }
            return stringPortfolios;
        }

        /// <summary>
        /// берет номер портфеля  
        /// </summary>
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
        #endregion

        #region ======= события сервера =====

        private void _server_ConnectStatusChangeEvent(string status)
        {
            DesirializerDictionaryOrders();
            
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
            _server.NewBidAscIncomeEvent -= _server_NewBidAscIncomeEvent;
            _server.ConnectStatusChangeEvent -= _server_ConnectStatusChangeEvent;

            RobotWindowVM.Log(Header, " Отключаем от сервера = " + _server.ServerType);
        }

        /// <summary>
        /// изменились лучшие цены 
        /// </summary>
        private void _server_NewBidAscIncomeEvent(decimal bid, decimal ask, Security namesecur)
        {
            _bestBid = 0;
            _bestAsk = 0;
            if (namesecur.Name == SelectedSecurity.Name)
            {
                _bestAsk = ask;
                _bestBid = bid;
            }
        }

        private void Server_NewTradeEvent(List<Trade> trades)
        {
            if (trades != null
                && trades[0].SecurityNameCode == SelectedSecurity.Name)
            {
                Trade trade = trades.Last();

                Price = trade.Price;

                CalculateMargin();
                ExaminationStop();

                if (trade.Time.Second % 5 == 0)
                {
                    // TradeLogic();
                }
            }
            
        }

        private void Server_NewCandleIncomeEvent(CandleSeries candle)
        {

        }

        /// <summary>
        /// пришел ответ с биржи по ордеру 
        /// </summary>
        private void Server_NewOrderIncomeEvent(Order order)
        {
            if (order == null || _portfolio == null || SelectedSecurity == null) return;
            if (order.SecurityNameCode == SelectedSecurity.Name
                && order.ServerType == Server.ServerType) // 
            {
                //TradeLogic();
                if (ActiveLevelAre())
                {
                    SerializerDictionaryOrders();
                    RobotWindowVM.SendStrTextDb(" SerializerDictionaryOrders ");
                    // SerializerLevel();
                }

                //  дальше запись в лог ответа с биржи по ордеру  и уровню 
                bool rec = true;
                if (order.State == OrderStateType.Activ
                    && order.TimeCallBack.AddSeconds(15) < Server.ServerTime)
                {
                    rec = false;
                }
                if (rec)
                {
                    RobotWindowVM.Log(Header, "NewOrderIncomeEvent = " + GetStringForSave(order));
                }
                if (order.NumberMarket != "")
                {
                    foreach (Level level in Levels)
                    {
                        bool newOrderBool = level.NewOrder(order);

                        if (newOrderBool && rec)
                        {
                            RobotWindowVM.Log(Header, "Уровень = " + level.GetStringForSave());
                        }
                    }
                }

                SaveParamsBot();
                //Get();
            }
        }

        /// <summary>
        ///  пришел мой трейд перевыставляем ордера по уровням
        /// </summary>
        private void Server_NewMyTradeEvent(MyTrade myTrade)
        {
            if (myTrade == null || myTrade.SecurityNameCode != SelectedSecurity.Name)
            {
                return; // нашей бумаги нет
            }
            if (ActiveLevelAre())
            {
                SerializerDictionaryOrders();
            }


            foreach (Level level in Levels)
            {
                bool newTrade = level.AddMyTrade(myTrade, SelectedSecurity.Lot);
                if (newTrade)
                {
                    RobotWindowVM.SendStrTextDb(" Trade ордера " + myTrade.NumberOrderParent + "\n " +
                                                    "NumberTrade " + myTrade.NumberTrade);

                    RobotWindowVM.Log(Header, "MyTrade =  " + GetStringForSave(myTrade));
                    if (myTrade.Side == level.Side)
                    {
                        LevelTradeLogicClose(level, Action.TAKE);
                    }
                    else
                    {
                        LevelTradeLogicOpen(level);
                    }
                    RobotWindowVM.Log(Header, " MyTrade \n Уровень  =  " + level.GetStringForSave());
                    SaveParamsBot();
                }
            }
        }
        /// <summary>
        /// Сервер мастер создан сервер
        /// </summary>
        private void ServerMaster_ServerCreateEvent(IServer server)
        {
            if (server.ServerType == ServerType)
            {
                Server = server;
               
            }
        }

        /// <summary>
        /// список подключенных бумаг на сервере изменися 
        /// </summary>
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

        /// <summary>
        /// изменлся портфель на сервере
        /// </summary>
        private void _server_PortfoliosChangeEvent(List<Portfolio> portfolios)
        {
            GetBalansSecur();// запросить объем монеты на бирже 
            if (portfolios == null || _portfoliosCount >= portfolios.Count) // нет новых портфелей 
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
        #endregion

        #endregion ===========================================================================================

        #region ============================================События============================================

        public delegate void selectedSecurity(string name);
        public event selectedSecurity OnSelectedSecurity;

        #endregion

        #region == ЗАГОТОВКИ =====

        /// <summary>
        /// сериализация словарz активных ордеров
        /// </summary>
        public void SerializerDictionaryActivOrders()
        {
            DataContractJsonSerializer DictionaryOrdersSerialazer = new DataContractJsonSerializer(typeof(ConcurrentDictionary<string, Order>));
            using (var file = new FileStream("DictionaryActivOrders.json", FileMode.OpenOrCreate))
            {
                DictionaryOrdersSerialazer.WriteObject(file, DictionaryOrdersActiv);
            }
        }
        /// <summary>
        ///  десериализация словаря ордеров 
        /// </summary>
        public void DesirializerDictionaryActivOrders()
        {
            DataContractJsonSerializer DictionaryOrdersSerialazer = new DataContractJsonSerializer(typeof(ConcurrentDictionary<string, Order>));
            using (var file = new FileStream("DictionaryActivOrders.json", FileMode.Open))
            {
                ConcurrentDictionary<string, Order> LoadDictionaryOrdersActiv = new ConcurrentDictionary<string, Order>();
                ConcurrentDictionary<string, Order> DictionaryOrdersActiv = DictionaryOrdersSerialazer.ReadObject(file) as ConcurrentDictionary<string, Order>;
                if (DictionaryOrdersActiv != null)
                {
                    foreach (var order in DictionaryOrdersActiv)
                    {
                        LoadDictionaryOrdersActiv.AddOrUpdate(order.Key, order.Value, (key, value) => value = order.Value);
                    }
                }
            }
        }


        /// <summary>
        /// сохраяие уровни в файл 
        /// </summary>
        public void SerializerLevel()
        {
            if (!Directory.Exists(@"Parametrs\Tabs"))
            {
                Directory.CreateDirectory(@"Parametrs\Tabs");
            }

            DataContractJsonSerializer LevelSerialazer = new DataContractJsonSerializer(typeof(ObservableCollection<Level>));

            using (var file = new FileStream(@"Parametrs\Tabs\levels_" + Header + "=" + NumberTab + ".json", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                LevelSerialazer.WriteObject(file, Levels);
                RobotWindowVM.SendStrTextDb(" Serializer Levels ");
            }
        }
        /// <summary>
        /// загружаеи из фала сохраненные уровни 
        /// </summary>
        public void DesirializerLevels()
        {
            if (!File.Exists(@"Parametrs\Tabs\levels_" + Header + "=" + NumberTab + ".json"))
            {
                RobotWindowVM.Log(Header, " DesirializerLevels \n нет файла levels_.json ");
                return;
            }

            DataContractJsonSerializer LevelsDsSerialazer = new DataContractJsonSerializer(typeof(ObservableCollection<Level>));
            using (var file = new FileStream(@"Parametrs\Tabs\levels_" + Header + "=" + NumberTab + ".json", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                ObservableCollection<Level> LevelsDeseriolazer = LevelsDsSerialazer.ReadObject(file) as ObservableCollection<Level>;
                if (LevelsDeseriolazer != null)
                {
                    Levels = LevelsDeseriolazer;
                    RobotWindowVM.Log(Header, " DesirializerLevels \n загрузили уровни из levels_.json ");
                }
            }
        }

        //RobotWindowVM.SendStrTextDb(" NewOrderIncomeEvent " + order.NumberMarket, " NumberUser " + order.NumberUser.ToString() + "\n"
        //             + " NewOrder Status " + order.State + "\n"
        //             + " DictionaryOrdersActiv count " + DictionaryOrdersActiv.Count);
        //if (order.State == OrderStateType.Activ)
        //{
        //    DictionaryOrdersActiv.AddOrUpdate(order.NumberMarket, order, (key, value) => value = order);

        //    RobotWindowVM.SendStrTextDb(" NewOrderIncomeEvent " + order.NumberMarket, " NumberUser " + order.NumberUser.ToString() + "\n"
        //                                + " Add Activ order \n" );
        //    //SerializerDictionaryOrders();
        //    RobotWindowVM.SendStrTextDb(" SerializerDictionaryOrders ");
        //}



        #endregion

        ////#region ============================= Реализация интерфейса INotifyPropertyChanged =======================

        //public event PropertyChangedEventHandler PropertyChanged;

        //public virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        //}

        //public virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
        //{
        //    if (Equals(field, value)) return false;
        //    field = value;
        //    OnPropertyChanged(PropertyName);
        //    return true;
        //}
        //#endregion
    }
}
