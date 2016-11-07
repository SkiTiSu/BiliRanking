using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BiliRanking.WPF.View
{
    /// <summary>
    /// Home.xaml 的交互逻辑
    /// </summary>
    public partial class Home : UserControl
    {
        public Home()
        {
            InitializeComponent();

            textComplileVer.Text = "编译版本号：" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void DonateButton_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/SkiTiSu/BiliRanking");
        }
    }
}
