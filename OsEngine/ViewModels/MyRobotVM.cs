using OsEngine.Commands;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.Market.Servers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OsEngine.ViewModels
{
    public class MyRobotVM : ViewModelBase
    {
        public MyRobotVM()
        {
            ServerMaster.ServerCreateEvent += ServerMaster_ServerCreateEvent;
        }

 
        #region Поля =====================================================================================

        private DelegateCommand comandServerConect;

        // List<IServer> _servers = new List<IServer>();
        IServer _server;

        List<Security> _securities = new List<Security>();

        private Security _security;

        #endregion

        #region Свойства ================================================================================== 

        public ObservableCollection <string> ListSecurities { get; set; } =  new ObservableCollection<string>();
        public string SelectedSecurity
        { 
            get => _selectedSecurity;
            set
            {
                _selectedSecurity = value;  
                OnPropertyChanged(nameof(SelectedSecurity));
                _security = GetSecuritiesFofName(_selectedSecurity);
                StartSecuritiy(_security);
            }
        }
        private string _selectedSecurity = "";

        #endregion

        #region Команды =====================================================================================

        public DelegateCommand ComandServerConect
        {
            get
            {
                if (comandServerConect == null)
                {
                    comandServerConect = new DelegateCommand(ServerConect);
                }
                return comandServerConect;
            }
        }

        #endregion

        #region Методы =====================================================================================

        private void StartSecuritiy(Security security)
        {
            if (security == null)
            {
                return;
            }
            Task.Run(() =>
            {
                while (true)
                {
                    var series = _server.StartThisSecurity(security.Name, new TimeFrameBuilder(), security.NameClass);
                    if (series != null)
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                }
            });
        }
        private void ServerMaster_ServerCreateEvent(IServer newserver)
        {
            /*
            foreach (IServer server in _servers)
            {
                if (newserver == server)
                {
                    return;
                }
            } 
            _servers.Add(newserver);
            newserver.PortfoliosChangeEvent += Newserver_PortfoliosChangeEvent;
            newserver.SecuritiesChangeEvent += Newserver_SecuritiesChangeEvent;
            newserver.NeadToReconnectEvent += Newserver_NeadToReconnectEvent;
            newserver.NewMarketDepthEvent += Newserver_NewMarketDepthEvent;
            newserver.NewTradeEvent += Newserver_NewTradeEvent;
            newserver.NewOrderIncomeEvent += Newserver_NewOrderIncomeEvent;
            newserver.NewMyTradeEvent += Newserver_NewMyTradeEvent;
            newserver.ConnectStatusChangeEvent += Newserver_ConnectStatusChangeEvent;
            */
            if (newserver == _server)
            {
                return;
            }
            _server =newserver;
            _server.PortfoliosChangeEvent += Newserver_PortfoliosChangeEvent;
            _server.SecuritiesChangeEvent += Newserver_SecuritiesChangeEvent;
            _server.NeadToReconnectEvent += Newserver_NeadToReconnectEvent;
            _server.NewMarketDepthEvent += Newserver_NewMarketDepthEvent;
            _server.NewTradeEvent += Newserver_NewTradeEvent;
            _server.NewOrderIncomeEvent += Newserver_NewOrderIncomeEvent;
            _server.NewMyTradeEvent += Newserver_NewMyTradeEvent;
            _server.ConnectStatusChangeEvent += Newserver_ConnectStatusChangeEvent;

        }

        private void Newserver_ConnectStatusChangeEvent(string conenect)
        {
            
        }

        private void Newserver_NewMyTradeEvent(Entity.MyTrade myTrade)
        {
            
        }

        private void Newserver_NewOrderIncomeEvent(Entity.Order order)
        {
           
        }

        private void Newserver_NewTradeEvent(List<Entity.Trade> trade)
        {
            
        }

        private void Newserver_NewMarketDepthEvent(Entity.MarketDepth marketDepth)
        {
            
        }

        private void Newserver_NeadToReconnectEvent()
        {
            
        }
        private Security GetSecuritiesFofName(string name)
        {
            for (int i = 0; i < _securities.Count; i++)
            {
                if (_securities[i].Name == name)
                {
                    return _securities[i];
                }
            }
            return null;
        }
        private void Newserver_SecuritiesChangeEvent(List<Entity.Security> securities)
        {
            ObservableCollection<string> listsecurities = new ObservableCollection<string>();
            for (int i = 0; i < securities.Count; i++)
            {
                listsecurities.Add(securities[i].Name);
            }

            ListSecurities = listsecurities;
            OnPropertyChanged(nameof(ListSecurities));
            _securities = securities;
        }

        private void Newserver_PortfoliosChangeEvent(List<Entity.Portfolio> portfolios)
        {
            
        }

        void ServerConect (object o)
        {
            ServerMaster.ShowDialog(false);
        }

        #endregion
    }
}
