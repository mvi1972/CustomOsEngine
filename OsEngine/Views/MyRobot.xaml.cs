using OsEngine.Market;
using OsEngine.Market.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OsEngine.Views
{
    /// <summary>
    /// Логика взаимодействия для MyRobot.xaml
    /// </summary>
    public partial class MyRobot : Window
    {
        public MyRobot()
        {
            InitializeComponent();
            ServerMaster.ServerCreateEvent += ServerMaster_ServerCreateEvent;
        }

        private IServer _server;

        private void ServerMaster_ServerCreateEvent(IServer server)
        {
            _server = server;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ServerMaster.ShowDialog(false);
        }
    }
}
