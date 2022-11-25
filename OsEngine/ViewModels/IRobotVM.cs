using OsEngine.Entity;
using OsEngine.MyEntity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OsEngine.ViewModels.GridRobotVM;

namespace OsEngine.ViewModels
{

    /// <summary>
    /// интерфейс для роботов 
    /// </summary>
    public interface IRobotVM 
    {
        /// <summary>
        /// заголовок робота
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// название статегии 
        /// </summary>
        public NameStrat NameStrat { get; set; }

        /// <summary>
        /// событие подписки на бумагу
        /// </summary>
        public event selectedSecurity OnSelectedSecurity;

        /// <summary>
        /// список портфелей 
        /// </summary>
        public ObservableCollection<string> StringPortfolios { get; set; }

        public Security SelectedSecurity { get; set; }
        /// <summary>
        /// список направления сделок 
        /// </summary>
        public List<Direction> Directions { get; set; }

        /// <summary>
        /// список типов расчета шага 
        /// </summary>
        public List<StepType> StepTypes { get; set; }

    }
}
