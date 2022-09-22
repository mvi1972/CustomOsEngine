using OsEngine.Market;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.ViewModels
{
    public class ChangeEmitentVM :ViewModelBase
    {
        public ObservableCollection <ServerType> ExChanges { get; set; } = new ObservableCollection<ServerType>();

        public ObservableCollection<string> EmitClasses { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> EmitNames { get; set; } 
        
    }
}
