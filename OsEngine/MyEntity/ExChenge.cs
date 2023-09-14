using OsEngine.Market;
using OsEngine.ViewModels;

namespace OsEngine.MyEntity
{
    public class ExChenge : BaseVM 
    {
        /// <summary>
        /// Биржа
        /// </summary>
        public ExChenge(ServerType type)
        {
            Server = type; 
        }
        public ServerType Server 
        {
            get => _server;
            set
            {
                _server = value;
                OnPropertyChanged(nameof(Server));
            }
        }
        private ServerType _server;
    }
}
