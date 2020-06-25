using MusicPlayer.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ServiceLocator.Setup();

            MainWindow = new LoadView();
            MainWindow.Show();
            GetView();
        }

        private MainWindowView InitView()
        {
            return new MainWindowView();
        }

        private void GetView()
        {
            MainWindowView mainWindowView = InitView();

            MainWindow.Close();
            MainWindow = mainWindowView;
            MainWindow.Show();
        }

    }
}
