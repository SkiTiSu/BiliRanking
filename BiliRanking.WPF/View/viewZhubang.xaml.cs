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
    /// Zhubang.xaml 的交互逻辑
    /// </summary>
    public partial class viewZhubang : UserControl
    {
        public viewZhubang()
        {
            InitializeComponent();
        }

        private void buttonGen_Click(object sender, RoutedEventArgs e)
        {
            string template = @"{title}|思源黑体 Light|26.45|#374a71|False|160|925|0|0|0|0|True|770
{created_at}|小塚ゴシック Pro B|26.45|#374a71|True|0|0|850|920|540|60|False
番号:{AVNUM}|思源黑体 Regular|22|#ffffff|False|192|827
UP主:{author}|思源黑体 Bold|22|#ffffff|False|648|827
{paiming}|小塚ゴシック Pro B|30.86|#ce5783|False|1690|166|0|0|0|0|False
{bofang}|小塚ゴシック Pro B|28.18|#425783|False|1598|290|0|0|0|0|False
{pinglun}|小塚ゴシック Pro B|16|#425783|False|1613|395|0|0|0|0|False
{danmu}|小塚ゴシック Pro B|16|#425783|False|1613|476|0|0|0|0|False
{shoucang}|小塚ゴシック Pro B|16|#425783|False|1613|555|0|0|0|0|False
{yingbi}|小塚ゴシック Pro B|16|#425783|False|1613|633|0|0|0|0|False
{zongfen}|小塚ゴシック Pro B|28.18|#425783|False|1598|728|0|0|0|0|False";
            int shuliang = Convert.ToInt32(textBoxTo.Text);
            Core.Zhubang zb = new Core.Zhubang();
            zb.GenStardust(SharedData.Infos.Take(shuliang).ToList(), template);
        }
    }
}
