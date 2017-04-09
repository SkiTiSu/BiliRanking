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
using BiliRanking.Core;

namespace BiliRanking.WPF.View
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent();
        }

        private async void buttonDo_Click(object sender, RoutedEventArgs e)
        {
            string accesskey = await BiliApiHelper.LoginBilibili(textBoxName.Text, textBoxPassword.Text);
            textBoxAccessKey.Text = accesskey;
        }
    }
}
