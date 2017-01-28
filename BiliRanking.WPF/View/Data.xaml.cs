using BiliRanking.Core;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
            dataGrid.Items?.Clear();
            dataGrid.ItemsSource = lls.ToList();

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

        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "圣地亚哥数据库X|*.sdygx";
            if (dlg.ShowDialog() == true)
            {
                MemoryStream tempMs = new MemoryStream();
                using (FileStream fs = new FileStream(dlg.FileName, FileMode.Open))
                {
                    using (GZipStream Compress = new GZipStream(fs, CompressionMode.Decompress))
                    {
                        Compress.CopyTo(tempMs);
                    }
                }
                byte[] bytes = tempMs.ToArray();
                string str = Encoding.GetEncoding("UTF-8").GetString(bytes);
                BiliShell bs = JsonConvert.DeserializeObject<BiliShell>(str);
                if (bs.ver != 1)
                {
                    MessageBox.Show("此文件是使用新版BR生成的，无法打开！");
                    return;
                }
                List<BiliInterfaceInfo> bi = JsonConvert.DeserializeObject<List<BiliInterfaceInfo>>(bs.infos.ToString());
                SetNewData(bi);
            }
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "圣地亚哥数据库X|*.sdygx";
            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".sdygx";
            dlg.FileName = fileName;
            if (dlg.ShowDialog() == true)
            {
                fileName = dlg.FileName;
                try
                {
                    BiliShell bs = new BiliShell
                    {
                        ver = 1,
                        infos = GetData()
                    };

                    string str = JsonConvert.SerializeObject(bs);
                    byte[] bytes = Encoding.GetEncoding("UTF-8").GetBytes(str);

                    using (FileStream fs = new FileStream(fileName, FileMode.Create))
                    {
                        using (GZipStream Compress = new GZipStream(fs, CompressionMode.Compress))
                        {
                            Compress.Write(bytes, 0, bytes.Length);
                        }
                    }
                    log.Info("成功导出文件 -> " + fileName);
                }
                catch (Exception exc)
                {
                    log.Error("文件保存错误！" + exc.Message);
                }
            }
        }

        private void SetNewData(List<BiliInterfaceInfo> data)
        {
            if (dataGrid.ItemsSource != null) dataGrid.ItemsSource = null;
            dataGrid.Items?.Clear();
            dataGrid.ItemsSource = data;
        }

        private List<BiliInterfaceInfo> GetData()
        {
            return dataGrid.Items?.OfType<BiliInterfaceInfo>().ToList() ?? new List<BiliInterfaceInfo>();
        }

        private void AddData(BiliInterfaceInfo info)
        {
            if (info == null)
                return;
            List<BiliInterfaceInfo> bi = GetData();
            bi.Add(info);
            SetNewData(bi);
        }

        private void AddData(List<BiliInterfaceInfo> info)
        {
            if (info == null)
                return;
            List<BiliInterfaceInfo> bi = GetData();
            bi.AddRange(info);
            SetNewData(bi);
        }

        public delegate Point GetPosition(IInputElement element);
        int rowIndex = -1;

        private void dataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!dataGrid.AllowDrop)
                return;
            rowIndex = GetCurrentRowIndex(e.GetPosition);
            if (rowIndex < 0)
                return;
            dataGrid.SelectedIndex = rowIndex;
            BiliInterfaceInfo selectedEmp = dataGrid.Items[rowIndex] as BiliInterfaceInfo;
            if (selectedEmp == null)
                return;
            textBlockDraging.Text = $"[{selectedEmp.Fpaiming}] {selectedEmp.AVNUM} - {selectedEmp.title}";
            DragDropEffects dragdropeffects = DragDropEffects.Move;
            if (DragDrop.DoDragDrop(dataGrid, selectedEmp, dragdropeffects)
                                != DragDropEffects.None)
            {
                dataGrid.SelectedItem = selectedEmp;
            }
        }

        private void dataGrid_Drop(object sender, DragEventArgs e)
        {
            popup1.IsOpen = false;

            if (rowIndex < 0)
                return;
            int index = this.GetCurrentRowIndex(e.GetPosition);
            if (index < 0)
                return;
            if (index == rowIndex)
                return;
            if (index == dataGrid.Items.Count - 1)
            {
                MessageBox.Show("This row-index cannot be drop");
                return;
            }

            List<BiliInterfaceInfo> bi = dataGrid.ItemsSource as List<BiliInterfaceInfo>;
            BiliInterfaceInfo changed = bi[rowIndex];
            bi.RemoveAt(rowIndex);
            bi.Insert(index, changed);
            SetNewData(bi);
        }

        private bool GetMouseTargetRow(Visual theTarget, GetPosition position)
        {
            Rect rect = VisualTreeHelper.GetDescendantBounds(theTarget);
            Point point = position((IInputElement)theTarget);
            return rect.Contains(point);
        }

        private DataGridRow GetRowItem(int index)
        {
            if (dataGrid.ItemContainerGenerator.Status
                    != GeneratorStatus.ContainersGenerated)
                return null;
            return dataGrid.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow;
        }

        private int GetCurrentRowIndex(GetPosition pos)
        {
            int curIndex = -1;
            for (int i = 0; i < dataGrid.Items.Count; i++)
            {
                DataGridRow itm = GetRowItem(i);
                if (GetMouseTargetRow(itm, pos))
                {
                    curIndex = i;
                    break;
                }
            }
            return curIndex;
        }

        private void dataGrid_DragOver(object sender, DragEventArgs e)
        {
            //if (e.LeftButton != MouseButtonState.Pressed) return;
            if (!popup1.IsOpen)
            {
                popup1.IsOpen = true;
            }

            Size popupSize = new Size(popup1.ActualWidth, popup1.ActualHeight);
            popup1.PlacementRectangle = new Rect(new Point(e.GetPosition(this).X +10, e.GetPosition(this).Y + 10), popupSize);

            Point position = e.GetPosition(dataGrid);
            var row = UIHelpers.TryFindFromPoint<DataGridRow>(dataGrid, position);
            if (row != null) dataGrid.SelectedItem = row.Item;
        }

        private async void buttonInsert_Click(object sender, RoutedEventArgs e)
        {
            BiliInterfaceInfo bi = await BiliInterface.GetInfoTaskAsync(textBoxInsert.Text);
            if (bi.pic != null) AddData(bi);
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
