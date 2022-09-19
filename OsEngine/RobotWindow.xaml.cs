﻿using MahApps.Metro.Controls;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Threading;
using System.Diagnostics;
using OsEngine.Market;
using System.Threading;
using OsEngine.ViewModels;

namespace OsEngine 
{
    /// <summary>
    /// Логика взаимодействия для RobotWindow.xaml
    /// </summary>
    public partial class RobotWindow : MetroWindow
    {
        public RobotWindow()
        {
            Process ps = Process.GetCurrentProcess();
            ps.PriorityClass = ProcessPriorityClass.RealTime;

            InitializeComponent();
            DataContext = new RobotWindowVM();

            ProccesIsWorked = true;
            _window = this;

            ServerMaster.ActivateLogging();
            this.Closed += RobotWindow_Closed; //событие закрытия окна

        }
        /// <summary>
        /// закрываем все рабочие процессы осы
        /// </summary>
        private void RobotWindow_Closed(object sender, EventArgs e)
        {
            ProccesIsWorked = false;
            Thread.Sleep(7000);
            Process.GetCurrentProcess().Kill();
        }

        public static Dispatcher GetDispatcher
        {
            get { return _window.Dispatcher; }
        }
        private static RobotWindow _window;

        /// <summary>
        /// работает ли приложение или закрывается
        /// </summary>
        public static bool ProccesIsWorked;


    }
}
