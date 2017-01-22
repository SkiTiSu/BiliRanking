using BiliRanking.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Data.xaml 的交互逻辑
    /// </summary>
    public partial class Data : UserControl
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public Data()
        {
            InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            string[] avs = { "av6949636","av7052909","av7049564","av6930969","av7038486"};
            BiliInterfaceInfo[] lls = await concurrentAsync(100, avs, new Func<string, Task<BiliInterfaceInfo>>(BiliInterface.GetInfoTaskAsync));
            dataGrid.ItemsSource = lls;

            log.Info("获取完成");
        }

        //http://stackoverflow.com/questions/20355931/limiting-the-amount-of-concurrent-tasks-in-net-4-5
        private static async Task<R[]> concurrentAsync<T, R>(int maxConcurrency, IEnumerable<T> items, Func<T, Task<R>> createTask)
        {
            var allTasks = new List<Task<R>>();
            var activeTasks = new List<Task<R>>();
            foreach (var item in items)
            {
                if (activeTasks.Count >= maxConcurrency)
                {
                    var completedTask = await Task.WhenAny(activeTasks);
                    activeTasks.Remove(completedTask);
                }
                var task = createTask(item);
                allTasks.Add(task);
                activeTasks.Add(task);
            }
            return await Task.WhenAll(allTasks);
        }

        private void AVNUM_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = (Hyperlink)e.OriginalSource;
            Process.Start(link.NavigateUri.AbsoluteUri);
        }

        //详见BiliInterfaceInfo里的注释
        //http://code.cheesydesign.com/?p=701
        //private void dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        //{
        //    PropertyDescriptor pd = (PropertyDescriptor)e.PropertyDescriptor;
        //    e.Column.Header = pd.DisplayName;
        //}
    }
}
