using OsEngine.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OsEngine.ViewModels.MyRobotVM;

namespace OsEngine.ViewModels
{

    /// <summary>
    /// интерфейс для роботов 
    /// </summary>
    public interface IRobotVM 
    {

        /// <summary>
        /// список портфелей 
        /// </summary>
        public ObservableCollection<string> StringPortfolios { get; set; }

        public Security SelectedSecurity { get; set; }  
 
    }
}
