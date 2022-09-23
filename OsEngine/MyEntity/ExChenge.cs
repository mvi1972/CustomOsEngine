using OsEngine.Market;
using OsEngine.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.MyEntity
{
    public class ExChenge : BaseVM 
    {
        public ExChenge(ServerType type)
        {
            Server = type.ToString(); 
        }
        public string Server 
        {
            get => _server;
            set
            {
                _server = value;
                OnPropertyChanged(nameof(Server));
            }
        }
        private string _server;
    }
}
