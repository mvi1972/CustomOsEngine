using OsEngine.Entity;
using OsEngine.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.MyEntity
{
    public class Level : BaseVM
    {
        #region ======================================Поля===========================================
        CultureInfo CultureInfo = new CultureInfo("ru-RU");

        #endregion
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


        /// <summary>
        /// объем позиции
        /// </summary>
        public decimal Volume
        {
            get => _volume;

            set
            {
                _volume = value;
                OnPropertyChanged(nameof(Volume));
            }
        }
        public decimal _volume = 0;

        public decimal Margine
        {
            get => _margine;

            set
            {
                _margine = value;
                OnPropertyChanged(nameof(Margine));
            }
        }
        public decimal _margine = 0;

        public decimal Accum
        {
            get => _accum;

            set
            {
                _accum = value;
                OnPropertyChanged(nameof(Accum));
            }
        }
        public decimal _accum = 0;
        /// <summary>
        /// расчетная цена для тейк профита 
        /// </summary>
        public decimal TacePrice
        {
            get => _tacePrice;

            set
            {
                _tacePrice = value;
                OnPropertyChanged(nameof(TacePrice));
            }
        }
        public decimal _tacePrice = 0;

        /// <summary>
        ///  лимитка на позицию
        /// </summary>
        public Order Order
        {
            get => _order;

            set
            {
                _order = value;
                OnPropertyChanged(nameof(OrderVolume));
                OnPropertyChanged(nameof(StateOrder));
            }
        }
        public Order _order = null;
        /// <summary>
        /// объем ордера 
        /// <summary>
        public decimal OrderVolume
        {
            get
            {
                if (Order != null
                    &&
                    (Order.State == OrderStateType.Activ
                    || Order.State == OrderStateType.Patrial
                    || Order.State == OrderStateType.Pending))
                {
                    return Order.Volume - Order.VolumeExecute;
                }
                return 0;
            }
        }
        /// <summary>
        /// статус ордера открытия поз
        /// </summary>
        public OrderStateType StateOrder
        {
            get
            {
                if (Order != null)
                {
                    return Order.State;
                }
                return 0;
            }
        }

        /// <summary>
        /// лимитка на тейк
        /// </summary>
        public Order LimitTake
        {
            get => _limitTake;

            set
            {
                _limitTake = value;
                OnPropertyChanged(nameof(StateTake));
                OnPropertyChanged(nameof(TakeVolume));
            }
        }
        public Order _limitTake = null;

        public decimal TakeVolume
        {
            get
            {
                if (LimitTake != null
                    &&
                    (LimitTake.State == OrderStateType.Activ
                    || LimitTake.State == OrderStateType.Patrial
                    || LimitTake.State == OrderStateType.Pending))
                {
                    return LimitTake.Volume - LimitTake.VolumeExecute;
                }
                return 0;
            }
        }
        /// <summary>
        /// статус тейк ордера закрытия поз
        /// </summary>
        public OrderStateType StateTake
        {
            get
            {
                if (LimitTake != null)
                {
                    return LimitTake.State;
                }
                return 0;
            }
        }

        /// <summary>
        /// разрешение открыть позицию        
        /// </summary>
        public bool PassVolume
        {
            get => _passVolume;

            set
            {
                _passVolume = value;
                OnPropertyChanged(nameof(PassVolume));
                OnPropertyChanged(nameof(OrderVolume));
                OnPropertyChanged(nameof(StateOrder));
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
                OnPropertyChanged(nameof(PassTake));
                OnPropertyChanged(nameof(StateTake));
                OnPropertyChanged(nameof(TakeVolume));
            }
        }
        public bool _passTake = true;
        #endregion
        #region ======================================Методы===============================================
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
            str += "OrderVolume = " + OrderVolume.ToString(CultureInfo) + " | ";
            str += "StateOrder = " + StateOrder + " | ";
            str += "TakeVolume = " + TakeVolume.ToString(CultureInfo) + " | ";
            str += "TacePrice = " + TacePrice.ToString(CultureInfo) + " | ";
            str += "StateTake = " + StateTake + " | ";

            return str;
        }

        public void AddMyTrade(MyTrade myTrade)
        {
            if (Volume ==0)
            {
                OpenPrice = myTrade.Price;
            }
            else if (Volume > 0)
            {
                if (myTrade.Side == Side.Buy)
                {
                    OpenPrice = (Volume * OpenPrice + myTrade.Volume * myTrade.Price) / (Volume + myTrade.Volume);
                }
                else
                {
                    if (myTrade.Volume <= Math.Abs(Volume))
                    {
                        Accum += (myTrade.Price - OpenPrice) * myTrade.Volume;
                    }
                    else
                    {
                        Accum += (myTrade.Price - OpenPrice) * Volume;
                        OpenPrice=myTrade.Price;    
                    }
                }
            }
            else if ( Volume < 0)
            {
                if (myTrade.Side == Side.Buy)
                {
                    if (myTrade.Volume <= Math.Abs(Volume))
                    {
                        Accum += (myTrade.Price - OpenPrice) * myTrade.Volume;
                    }
                    else
                    {
                        Accum += (myTrade.Price - OpenPrice) * Volume;
                        OpenPrice = myTrade.Price;
                    }
                }
                else
                {
                    OpenPrice = (Volume * OpenPrice + myTrade.Volume * myTrade.Price) / (Volume + myTrade.Volume);
                }
            }
            if (myTrade.Side == Side.Buy)
            {
                Volume += myTrade.Volume;
            }
            else
            {
                Volume -= myTrade.Volume;
            }
            if (Volume ==0)
            {
                OpenPrice=0;
            }
        }

        #endregion

    }
}
