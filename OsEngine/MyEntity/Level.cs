using OsEngine.Entity;
using OsEngine.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.MyEntity
{
    public class Level : BaseVM
    {
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
        public decimal _openPrice =0;

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
        public decimal _volume =0 ;

        public decimal Margine
        {
            get => _margine;

            set
            {
                _margine = value;
                OnPropertyChanged(nameof(Margine));
            }
        }
        public decimal  _margine = 0; 
        
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
        /// объем выставленной лимитки на покуп
        /// </summary>
        public decimal LimitVolume
        {
            get => _limitVolume;

            set
            {
                _limitVolume = value;
                OnPropertyChanged(nameof(LimitVolume));
            }
        }
        public decimal _limitVolume = 0;

        /// <summary>
        /// лимитка на тейк
        /// </summary>
        public decimal LimitTake
        {
            get => _limitTake;

            set
            {
                _limitTake = value;
                OnPropertyChanged(nameof(LimitTake));
            }
        }
        public decimal _limitTake = 0;

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
            }
        }
        public bool _passVolume = true ;

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
            }
        }
        public bool _passTake = true;
    }
}
