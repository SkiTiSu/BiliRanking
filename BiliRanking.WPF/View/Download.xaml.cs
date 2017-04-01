using BiliRanking.Core;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BiliRanking.WPF.View
{
    /// <summary>
    /// Download.xaml 的交互逻辑
    /// </summary>
    public partial class Download : UserControl
    {
        public Download()
        {
            InitializeComponent();
        }

        private void buttonGet_Click(object sender, RoutedEventArgs e)
        {
            var avs = SharedData.SortedAVs;
            string re = "";
            foreach (string item in avs)
            {
                var urls = BiliInterface.GetAVFlvUrl(item);
                if (urls != null)
                {
                    foreach (string u in urls)
                    {
                        re += u + "\r\n";
                    }
                }
                re += "\r\n";
            }
            textBox.Text = re;
        }
    }
}
