using OsEngine.Entity;
using OsEngine.Market.Servers;
using OsEngine.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OsEngine.MyEntity
{
    public class Level : BaseVM
    {
 
        #region ======================================Свойства===============================================
        /// <summary>
        /// расчеткая цена уровня
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
        /// реалькая цена открытой позиции
        /// </summary>
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
        ///  список лимиток на тейк
        /// </summary>
        public List<Order> OrdersForClose = new List<Order>();

        /// <summary>
        /// лимитки на открытие позиций 
        /// </summary>
        public List<Order> OrdersForOpen = new List<Order>();

        private List<MyTrade> _myTrades = new List<MyTrade>();

        private decimal _calcVolume = 0;

        #endregion
        #region ======================================Методы===============================================

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

        public bool NewOrder(Order newOrder)
        {
            foreach (Order order in OrdersForOpen)
            {
                if (order.NumberMarket == newOrder.NumberMarket)
                {
                    CalculateOrders();

                    return true;
                }
            }
            foreach (Order order in OrdersForClose)
            {
                if (order.NumberMarket == newOrder.NumberMarket)
                {
                    CalculateOrders();

                    return true;
                }
            }
            return false;
        }

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
                }
                else if (order.State == OrderStateType.Pending
                        || order.State == OrderStateType.None)
                {
                    passLimit = false;  
                }
            }

            foreach (Order order in OrdersForClose)
            {
                volumeExecute -= order.VolumeExecute;
                if (order.State == OrderStateType.Activ
                    || order.State == OrderStateType.Patrial)
                {
                    activeTake += order.Volume - order.VolumeExecute;
                }
                else if (order.State == OrderStateType.Pending
                        || order.State == OrderStateType.None)
                {
                    passTake = false;
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
                }
            }
            orders = newOrders;  
        }

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

        }

        /// <summary>
        ///  формируем строку для сохранения
        /// </summary>
        public string GetStringForSave()
        {
            string str = "";

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
                    CanselCloseOrders(server, getStringForSave);
                    CanselOpenOrders(server, getStringForSave);
                    Thread.Sleep(2000);
                    if (LimitVolume==0 && TakeVolume==0)
                    {
                        break;
                    }
                }
            });
 
        }

        private void CanselOpenOrders(IServer server, DelegateGetStringForSave getStringForSave)
        {
            foreach (Order order in OrdersForOpen)
            {
                if (order != null
                       && order.State == OrderStateType.Activ
                        || order.State == OrderStateType.Patrial
                        || order.State == OrderStateType.Pending)
                {
                    server.CancelOrder(order);
                    RobotWindowVM.Log(order.SecurityNameCode, " Снимаем лимитки на сервере " + getStringForSave(order));
                    Thread.Sleep(30);
                }
            }
        }

        private void CanselCloseOrders(IServer server, DelegateGetStringForSave getStringForSave)
        {
            foreach (Order order in OrdersForClose)
            {
                if (order != null
                       && order.State == OrderStateType.Activ
                        || order.State == OrderStateType.Patrial
                        || order.State == OrderStateType.Pending)
                {
                    server.CancelOrder(order);
                    RobotWindowVM.Log(order.SecurityNameCode, " Снимаем тейк на сервере " + getStringForSave(order));
                    Thread.Sleep(30);
                }
            }
        }

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
                        openPrice = (_calcVolume * openPrice + myTrade.Volume * myTrade.Price) / (_calcVolume + myTrade.Volume);
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
        private bool IsMyTrade(MyTrade myTrade)
        {
            foreach (Order order in OrdersForOpen)
            {
                if (order.NumberMarket == myTrade.NumberOrderParent)
                {
                    return true;
                }
            }
            foreach (Order order in OrdersForClose)
            {
                if (order.NumberMarket == myTrade.NumberOrderParent)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Делегаты ================================================================================================

        public delegate string DelegateGetStringForSave(Order order);


        #endregion

    }
}
