using BiliRanking.Core;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// List.xaml 的交互逻辑
    /// </summary>
    public partial class List : UserControl
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public List()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            comboBoxListSort.ItemsSource = Enum.GetNames(typeof(BiliParse.SortType));
            if (comboBoxListSort.ItemsSource != null) comboBoxListSort.SelectedIndex = 0;

            buttonListDate2wAgo_Click(sender, e);
        }

        private void buttonGenOld_Click(object sender, RoutedEventArgs e)
        {
            log.Info("开始获取排行");

            BiliParse.SortType sort = (BiliParse.SortType)Enum.Parse(typeof(BiliParse.SortType), comboBoxListSort.SelectedItem.ToString(), false);
            int needpage = Convert.ToInt32(comboBoxListNum.Text) / 20;
            if ((Convert.ToInt32(comboBoxListNum.Text) % 20) != 0)
                needpage += 1;

            List<string> ss = new List<string>();

            string czone = comboBoxListZone.Text;
            string tzone = Regex.Match(czone, @"\d+").Value;

            for (int i = 1; i <= needpage; i++)
            {
                List<string> sts = BiliParse.GetListOld(sort, int.Parse(tzone), i, datePickerFrom.SelectedDate.Value, datePickerTo.SelectedDate.Value);
                if (sts != null)
                    ss.AddRange(sts);
                else
                    break;
            }

            SharedData.AVs = "";

            for (int i = 0; i < Convert.ToInt32(comboBoxListNum.Text); i++)
            {
                try
                {
                    SharedData.AVs += ss[i] + "\r\n";
                }
                catch
                {
                    log.Warn("选定区间内视频数量不满" + comboBoxListNum.Text + "个！仅有" + i.ToString() + "个。");
                    break;
                }
            }
        }

        private void buttonListDate2wAgo_Click(object sender, RoutedEventArgs e)
        {
            datePickerFrom.SelectedDate = getWeekUpOfDate(DateTime.Now, DayOfWeek.Monday, -2);
            datePickerTo.SelectedDate = datePickerFrom.SelectedDate.Value.AddDays(6);
        }

        private void buttonListDate1wAgo_Click(object sender, RoutedEventArgs e)
        {
            datePickerFrom.SelectedDate = getWeekUpOfDate(DateTime.Now, DayOfWeek.Monday, -1);
            datePickerTo.SelectedDate = datePickerFrom.SelectedDate.Value.AddDays(6);
        }

        public DateTime getWeekUpOfDate(DateTime dt, DayOfWeek weekday, int Number)
        {
            int wd1 = (int)weekday;
            int wd2 = (int)dt.DayOfWeek;
            wd1 = (wd1 == 0) ? 7 : wd1; //修改为周一作为第一天
            wd2 = (wd2 == 0) ? 7 : wd2;
            return wd2 == wd1 ? dt.AddDays(7 * Number) : dt.AddDays(7 * Number - wd2 + wd1);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow Form = Application.Current.Windows[0] as MainWindow;
            Form.listBoxItems.SelectedIndex = 2;
        }

        private void buttonParse_Click(object sender, RoutedEventArgs e)
        {
            MatchCollection mc = Regex.Matches(textBoxNeedParse.Text, @"[aA][vV]\d+");
            SharedData.AVs = "";
            foreach (Match m in mc)
            {
                SharedData.AVs += m.Value + "\r\n";
            }
        }

        private void buttonGen_Click(object sender, RoutedEventArgs e)
        {
            List<string> l22 = BiliParse.GetList(22, datePickerFrom.SelectedDate.Value, datePickerTo.SelectedDate.Value);
            List<string> l26 = BiliParse.GetList(26, datePickerFrom.SelectedDate.Value, datePickerTo.SelectedDate.Value);
            List<string> l126 = BiliParse.GetList(126, datePickerFrom.SelectedDate.Value, datePickerTo.SelectedDate.Value);

            List<string> all = new List<string>();

            all.AddRange(l22);
            all.AddRange(l26);
            all.AddRange(l126);
            
            SharedData.AVs = "";
            foreach (string av in all)
            {
                SharedData.AVs += av + "\r\n";
            }
        }
    }
}
