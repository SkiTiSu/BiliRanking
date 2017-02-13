using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace BiliRanking.WPF
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {

        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            BiliRanking.WPF.Properties.Settings.Default.Save();
        }
    }
}
