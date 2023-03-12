using OsEngine.Entity;
using OsEngine.Market.Servers;
using OsEngine.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OsEngine.MyEntity
{
    [DataContract]   
    public class Level : BaseVM
    {
        #region ======================================Свойства======================================
        [DataMember]
        /// <summary>
        /// цена уровня
        /// </summary>
        public decimal PriceLevel
        {
            get => _priceLevel;

            set
            {
                _priceLevel = value;
                OnPropertyChanged(nameof(PriceLevel));
            }
        }
        public decimal _priceLevel = 0;
        /// <summary>
        /// направлние сделок на уровне
        /// </summary>
        [DataMember]
        public Side Side
        {
            get => _side;

            set
            {
                _side = value;
                OnPropertyChanged(nameof(Side));
            }
        }
        public Side _side;

        /// <summary>
        /// Статус сделок на уровне
        /// </summary>
        [DataMember]
        public PositionStatus StatusLevel
        {
            get => _statusLevel;

            set
            {
                _statusLevel = value;
                OnPropertyChanged(nameof(StatusLevel));
            }
        }
        private PositionStatus _statusLevel;
        
        /// <summary>
        /// реалькая цена открытой позиции
        /// </summary>
        [DataMember]
        public decimal OpenPrice
        {
            get => _openPrice;

            set
            {
                _openPrice = value;
                OnPropertyChanged(nameof(OpenPrice));
            }
        }
        public decimal _openPrice = 0;
        /// <summary>
        ///  цена закрытия позиции (прибыли)
        /// </summary>
        [DataMember]
        public decimal TakePrice
        {
            get => _takePrice;

            set
            {
                _takePrice = value;
                Change();
            }
        }
        public decimal _takePrice = 0;

        [DataMember]
        /// <summary>
        /// объем позиции
        /// </summary>
        public decimal Volume
        {
            get => _volume;

            set
            {
                _volume = value;
                Change(); 
            }
        }
        public decimal _volume = 0;
        [DataMember]
        public decimal Margin
        {
            get => _margine;

            set
            {
                _margine = value;
                Change();
            }
        }
        public decimal _margine = 0;
        [DataMember]
        public decimal Accum
        {
            get => _accum;

            set
            {
                _accum = value;
                Change();
                
            }
        }
        public decimal _accum = 0;
        [DataMember]
        /// <summary>
        /// объем ордера открытия поз
        /// <summary>
        public decimal LimitVolume
        {
            get => _limitVolume;
            set
            {
                _limitVolume = value;
                Change();
            }
        }
        private decimal _limitVolume;
        [DataMember]
        /// <summary>
        /// Обем ордера закрытия поз
        /// </summary>
        public decimal TakeVolume
        {
            get => _takeVolume;
            set
            {
                _takeVolume = value;
                Change();
            }
        }
        private decimal _takeVolume;
        [DataMember]
        /// <summary>
        /// разрешение открыть позицию        
        /// </summary>
        public bool PassVolume
        {
            get => _passVolume;

            set
            {
                _passVolume = value;
                Change();
            }
        }
        public bool _passVolume = true;
        [DataMember]
        /// <summary>
        /// разрешение выставить тейк     
        /// </summary>
        public bool PassTake
        {
            get => _passTake;

            set
            {
                _passTake = value;
                Change();
            }
        }
        public bool _passTake = true;
        #endregion

        #region ======================================Поля===========================================

        CultureInfo CultureInfo = new CultureInfo("ru-RU");

        /// <summary>
        ///  список лимиток на закрытие
        /// </summary>
        [DataMember]
        public List<Order> OrdersForClose = new List<Order>();

        /// <summary>
        /// лимитки на открытие позиций 
        /// </summary>
        [DataMember]
        public List<Order> OrdersForOpen = new List<Order>();

        /// <summary>
        ///  список моих трейдов принадлежащих уровню
        /// </summary>
        [DataMember]
        private List<MyTrade> _myTrades = new List<MyTrade>();

        private decimal _calcVolume = 0;

        #endregion

        #region ======================================Методы====================================
       
        //public void GetCounOrdersForOpen()
        //{
        //    int count = 0;
        //    count = OrdersForOpen.Count;
        //    if (count>0)
        //    {
        //        decimal orderprice = OrdersForOpen[OrdersForOpen.Count - 1].Price;
        //        string str = "ордеров на открытие = " + count + "\n"
        //            + " цена последнего = " + orderprice;
        //        Debug.WriteLine(str);
        //    }  
        //}

        public void SetVolumeStart()
        {
            _calcVolume = Volume;

            if (Volume == 0)
            {
                if (LimitVolume == 0)
                {
                    ClearOrders(ref OrdersForOpen);
                }
                if (TakeVolume == 0)
                {
                    ClearOrders(ref OrdersForClose);
                }
            }
        }


        /// <summary>
        /// принадлежит ли ордер уровню 
        /// </summary>
        public bool NewOrder(Order newOrder)
        {
            foreach (Order order in OrdersForOpen)
            {
                if (order.NumberMarket == newOrder.NumberMarket)
                {
                    CalculateOrders();
                    StatusLevel = PositionStatus.OPEN;

                    return true;
                }
            }
            foreach (Order order in OrdersForClose)
            {
                if (order.NumberMarket == newOrder.NumberMarket)
                {
                    CalculateOrders();
                    StatusLevel = PositionStatus.DONE;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// проверка объема исполненых ордеров на уровне + смена статусов сделки уровня
        /// </summary>
        private void CalculateOrders()
        {
            decimal activeVolume = 0;
            decimal volumeExecute = 0;

            decimal activeTake = 0;

            bool passLimit = true;
            bool passTake = true;

            foreach (Order order in OrdersForOpen)
            {
                volumeExecute += order.VolumeExecute;
                if (order.State == OrderStateType.Activ
                    || order.State == OrderStateType.Patrial)
                {
                    activeVolume += order.Volume - order.VolumeExecute;
                    StatusLevel = PositionStatus.OPENING;
                }
                else if (order.State == OrderStateType.Pending
                        || order.State == OrderStateType.None)
                {
                    passLimit = false;
                    StatusLevel = PositionStatus.NONE;
                }
            }

            foreach (Order order in OrdersForClose)
            {
                volumeExecute -= order.VolumeExecute;
                if (order.State == OrderStateType.Activ
                    || order.State == OrderStateType.Patrial)
                {
                    activeTake += order.Volume - order.VolumeExecute;
                    StatusLevel = PositionStatus.CLOSING;
                }
                else if (order.State == OrderStateType.Pending
                        || order.State == OrderStateType.None)
                {
                    passTake = false;
                    StatusLevel = PositionStatus.NONE;
                }
            }

            Volume = volumeExecute;

            if (Side == Side.Sell)
            {
                Volume *= -1;
            }

            LimitVolume = activeVolume;
            TakeVolume = activeTake;
            PassVolume = passLimit;
            PassTake= passTake;
        }
        private void ClearOrders(ref List<Order> orders)
        {
            List<Order> newOrders = new List<Order>();
            foreach (Order order in orders)
            {
                if (order!= null
                    && order.State != OrderStateType.Cancel
                    && order.State != OrderStateType.Done)
                {
                    newOrders.Add(order);
                    RobotWindowVM.SendStrTextDb(" Удалили ордера Cancel и Done " );
                }
            }
            orders = newOrders;  
        }

        /// <summary>
        /// список свойств для обновления в интефейсе после изменений
        /// </summary>
        private void Change()
        {          
            OnPropertyChanged(nameof(Volume));
            OnPropertyChanged(nameof(OpenPrice));
            OnPropertyChanged(nameof(LimitVolume));
            OnPropertyChanged(nameof(PassTake));
            OnPropertyChanged(nameof(TakeVolume));
            OnPropertyChanged(nameof(PassVolume));
            OnPropertyChanged(nameof(TakePrice));
            OnPropertyChanged(nameof(Side));
            OnPropertyChanged(nameof(PriceLevel));
            OnPropertyChanged(nameof(Margin));
            OnPropertyChanged(nameof(Accum));
            OnPropertyChanged(nameof(StatusLevel));
        }

        /// <summary>
        ///  формируем строку для сохранения
        /// </summary>
        public string GetStringForSave()
        {
            string str = "";
            str += "Уровень = \n";
            str += "Volume = " + Volume.ToString(CultureInfo) + " | ";
            str += "PriceLevel = " + PriceLevel.ToString(CultureInfo) + " | ";
            str += "OpenPrice = " + OpenPrice.ToString(CultureInfo) + " | ";
            str += Side + " | ";
            str += "PassVolume = " + PassVolume.ToString(CultureInfo) + " | ";
            str += "PassTake = " + PassTake.ToString(CultureInfo) + " | ";
            str += "LimitVolume = " + LimitVolume.ToString(CultureInfo) + " | ";
            str += "TakeVolume = " + TakeVolume.ToString(CultureInfo) + " | ";
            str += "TacePrice = " + TakePrice.ToString(CultureInfo) + " | ";
       
            return str;
        }

        /// <summary>
        /// отозвать все ордера с биржи
        /// </summary>
        public void CancelAllOrders(IServer server, DelegateGetStringForSave getStringForSave)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    string namsec = "";
                    if (OrdersForOpen.Count != 0) // эта конструкция что бы взять имя бумаги для отправки в лог 
                    {                        
                        Order order = OrdersForOpen[0];
                        namsec = order.SecurityNameCode;

                        RobotWindowVM.Log(order.SecurityNameCode, "ВКЛЮЧЕН поток для отзыва ордеров на открытие \n");
                    }
                    if (OrdersForClose.Count != 0)
                    {
                        Order order = OrdersForClose[0];
                        namsec = order.SecurityNameCode;

                        RobotWindowVM.Log(order.SecurityNameCode, "ВКЛЮЧЕН поток отзыва ордеров на закрытие \n");
                    }
                    CanselCloseOrders(server, getStringForSave);
                    CanselOpenOrders(server, getStringForSave);

                    string str = "ВКЛЮЧЕН поток для отзыва ордеров \n";
                    Debug.WriteLine(str);

                    Thread.Sleep(2000);
                    if (LimitVolume == 0 && TakeVolume == 0)
                    {
                        string str2 = "Поток для отзыва ордеров ОТКЛЮЧЕН \n";
                        Debug.WriteLine(str2);
                        if (namsec != "")
                        {
                            RobotWindowVM.Log(namsec, "Поток для отзыва ордеров ОТКЛЮЧЕН \n");
                        }
                        break;
                    }
                }
            });
        }

        /// <summary>
        /// отозвать ордера на открытие с биржи
        /// </summary>
        private void CanselOpenOrders(IServer server, DelegateGetStringForSave getStringForSave)
        {
            foreach (Order order in OrdersForOpen)
            {
                RobotWindowVM.SendStrTextDb("OrdersForOpen[0].NumberMarket " + OrdersForOpen[0].NumberMarket.ToString());
                if (order != null
                       && order.State == OrderStateType.Activ
                        || order.State == OrderStateType.Patrial
                        || order.State == OrderStateType.Pending)
                {
                    server.CancelOrder(order);
                  
                    RobotWindowVM.Log(order.SecurityNameCode, " Снимаем лимитку на открытие с биржи \n" + getStringForSave(order));
                    Thread.Sleep(30); 
                }
            }
        }

        /// <summary>
        /// отозвать ордера на закрытие с биржи
        /// </summary>
        private void CanselCloseOrders(IServer server, DelegateGetStringForSave getStringForSave)
        {
            foreach (Order order in OrdersForClose)
            {
                RobotWindowVM.SendStrTextDb("OrdersForClose[0].NumberMarket " + OrdersForClose[0].NumberMarket.ToString());
                if (order != null
                       && order.State == OrderStateType.Activ
                        || order.State == OrderStateType.Patrial
                        || order.State == OrderStateType.Pending)
                {
                    server.CancelOrder(order);
                    RobotWindowVM.Log(order.SecurityNameCode, " Снимаем тейк на сервере \n" + getStringForSave(order));
                    Thread.Sleep(30);
                }
            }
        }

        /// <summary>
        /// проверяет принадлежность трейд к уроню и добавляет если да
        /// </summary>
        public bool AddMyTrade(MyTrade myTrade, decimal contLot)
        {
            foreach (MyTrade trade in _myTrades)
            {
                if (trade.NumberTrade == myTrade.NumberTrade)
                {
                    return false;
                }
            }

            if (IsMyTrade(myTrade))
            {
                _myTrades.Add(myTrade);

                CalculateOrders();
                CalculatePosition(contLot);
                return true;
            }
            return false;
        }
        /// <summary>
        /// расчет объема позиции (по лотам)
        /// </summary>
        private void CalculatePosition(decimal contLot)
        {            
            //decimal volume =0;
            decimal openPrice = 0;
            decimal accum = 0;

            foreach (MyTrade myTrade in _myTrades) 
            {
                if (_calcVolume == 0)
                {
                    openPrice = myTrade.Price;
                }
                else if (_calcVolume > 0)
                {
                    if (myTrade.Side == Side.Buy)
                    {
                        openPrice = (_calcVolume * openPrice + myTrade.Volume * myTrade.Price) / (_calcVolume + myTrade.Volume);
                    }
                    else
                    {
                        if (myTrade.Volume <= Math.Abs(_calcVolume))
                        {
                            accum += (myTrade.Price - openPrice) * myTrade.Volume;
                        }
                        else
                        {
                            accum += (myTrade.Price - openPrice) * _calcVolume;
                            openPrice = myTrade.Price;
                        }
                    }
                }
                else if (_calcVolume < 0)
                {
                    if (myTrade.Side == Side.Buy)
                    {
                        if (myTrade.Volume <= Math.Abs(_calcVolume))
                        {
                            accum += (myTrade.Price - openPrice) * myTrade.Volume;
                        }
                        else
                        {
                            accum += (myTrade.Price - openPrice) * _calcVolume;
                            openPrice = myTrade.Price;
                        }
                    }
                    else   
                    {
    // изсенил расчет 
                        //_calcVolume *= -1;
                        openPrice = (Math.Abs(_calcVolume) * openPrice + myTrade.Volume * myTrade.Price) / (Math.Abs(_calcVolume) + myTrade.Volume);
                    }
                }
             
                if (myTrade.Side == Side.Buy)
                {
                    _calcVolume += myTrade.Volume;
                }
                else if (myTrade.Side == Side.Sell)
                {
                    _calcVolume -= myTrade.Volume;
                }
             
                if (_calcVolume == 0)
                {
                    openPrice = 0;
                }
            }
            OpenPrice = openPrice;
            Accum = accum * contLot;
        }

        /// <summary>
        /// принадлежит ли трейд ордеру проверка
        /// </summary>
        private bool IsMyTrade(MyTrade myTrade)
        {
            foreach (Order order in OrdersForOpen)
            {
                if (order.NumberMarket == myTrade.NumberOrderParent)
                {
                    // номер трейда принадлежит ордеру открытия позы на бирже
                    // StatusLevel = PositionStatus.OPEN;
                    return true;                   
                }
            }
            foreach (Order order in OrdersForClose)
            {
                if (order.NumberMarket == myTrade.NumberOrderParent)
                {
                    // номер трейда принадлежит ордеру закрытия позы на бирже
                    // StatusLevel = PositionStatus.DONE;
                    return true;
                }
            }
            return false;
        } 

        #endregion

        #region =================================Делегаты ====================================

        public delegate string DelegateGetStringForSave(Order order);


        #endregion

    }
}
