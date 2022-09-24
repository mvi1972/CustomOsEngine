using OsEngine.Commands;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.MyEntity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.ViewModels
{
    /// <summary>
    /// Модель отображения выбора бумаг 
    /// </summary>
    public class ChangeEmitentVM :BaseVM
    {
        public ChangeEmitentVM()
        {
            Init();
        }

        #region Свойства ===============================================================================
        public ObservableCollection<ExChenge> ExChanges { get; set; } = new ObservableCollection<ExChenge>();

        public ObservableCollection<string> EmitClasses { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<Emitent> Securites { get; set; } = new ObservableCollection<Emitent>();

        #endregion

        #region Команды ===============================================================================
        private DelegateCommand commandSetExChange;
        public DelegateCommand CommandSetExChange
        {
            get
            {
                if(commandSetExChange == null)
                {
                    commandSetExChange = new DelegateCommand(SetExChange);
                }
                return commandSetExChange;
            }
        }

        #endregion

        #region Методы ===============================================================================

        void SetExChange(object o)    
        {

        }

        /// <summary>
        ///  Инициализация бумаг сервера для отображения 
        /// </summary>
        void Init()
        {
            List<IServer> servers = ServerMaster.GetServers();

            ExChanges.Clear();
 
            foreach (IServer server in servers)
            {
                ExChanges.Add( new ExChenge(server.ServerType));

            }
          
            OnPropertyChanged(nameof(ExChanges));
        } 

        #endregion


    }
}
