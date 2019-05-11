﻿using BiliRanking.Core;
using BiliRanking.WPF.Domain;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        DateTime genTime;

        public Data()
        {
            InitializeComponent();
        }

        private async void buttonGen_Click(object sender, RoutedEventArgs e)
        {
            log.Info("开始批量获取");
            ScoreType st = (ScoreType)((EnumerationExtension.EnumerationMember)comboBoxScoreType.SelectedItem).Value;
            IEnumerable<string> avs = SharedData.SortedAVs;
            BiliInterfaceInfo[] lls = await concurrentAsync(
                Convert.ToInt32(textBoxConcurrency.Text), //现在有限制了，不能弄那么快了
                avs,
                new Func<string, ScoreType, bool, Task<BiliInterfaceInfo>>(BiliInterface.GetInfoTaskAsync),
                st,
                toggleButtonUseKanb.IsChecked.GetValueOrDefault());
            List<BiliInterfaceInfo> ll = new List<BiliInterfaceInfo>();
            string failedAVs = "";
            foreach (BiliInterfaceInfo info in lls)
            {
                if (info.pic != null)
                {
                    ll.Add(info);
                }
                else
                {
                    failedAVs += info.avnum + ";";
                }
            }
            ll.Sort(sortt);
            for (int i = 1; i <= ll.Count; i++)
            {
                ll[i - 1].Fpaiming = i;
            }
            if (failedAVs != "")
            {
                log.Warn("注意！下列视频数据未正确获取！\r\n" + failedAVs);
            }
            RefreshGenTime(DateTime.Now);
            SetNewData(ll);
            log.Info("批量获取完成");
        }

        public static int sortt(BiliInterfaceInfo x, BiliInterfaceInfo y)
        {
            int res = 0;
            if (x.Fdefen > y.Fdefen)
                res = -1;
            else
                res = 1;
            return res;
        }

        public void RefreshGenTime(DateTime time)
        {
            genTime = time;
            textBlockGenTime.Text = genTime.ToString("yyyy/MM/dd HH:mm");
        }

        //http://stackoverflow.com/questions/20355931/limiting-the-amount-of-concurrent-tasks-in-net-4-5
        private static async Task<R[]> concurrentAsync<T1, T2, T3, R>(int maxConcurrency, IEnumerable<T1> items, Func<T1, T2, T3, Task<R>> createTask, T2 stype, T3 useKanb)
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
                var task = createTask(item, stype, useKanb);
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
                OpenFile(dlg.FileName);
            }
        }

        public void OpenFile(string name)
        {
            if (!name.EndsWith(".sdygx"))
            {
                MessageBox.Show("不是圣地亚哥数据库X文件啊！拖错啦！");
                return;
            }
            MemoryStream tempMs = new MemoryStream();
            using (FileStream fs = new FileStream(name, FileMode.Open))
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
            RefreshGenTime(bs.genTime);
            SharedData.SortedAVs = from a in bi select a.avnum;
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
                        infos = GetData(),
                        genTime = genTime
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

        List<BiliInterfaceInfo> lastItemSource;
        private void SetNewData(List<BiliInterfaceInfo> data)
        {
            if (dataGrid.ItemsSource != null)
            {
                lastItemSource = dataGrid.ItemsSource as List<BiliInterfaceInfo>;
                dataGrid.ItemsSource = null;
            }
            dataGrid.Items?.Clear();
            dataGrid.ItemsSource = data;
            dataGrid.Items.Refresh();
            SharedData.Infos = data;
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

        private void RemoveData(BiliInterfaceInfo info)
        {
            if (info == null)
                return;
            List<BiliInterfaceInfo> bi = GetData();
            //TODO: 更换新逻辑
            bi.Remove(info);
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

            List<BiliInterfaceInfo> bi = new List<BiliInterfaceInfo>();
            (dataGrid.ItemsSource as List<BiliInterfaceInfo>).ForEach(i => bi.Add(i)); //为了实现撤销需要深复制
            //List<BiliInterfaceInfo> bi = dataGrid.ItemsSource as List<BiliInterfaceInfo>;
            BiliInterfaceInfo changed = bi[rowIndex];
            bi.RemoveAt(rowIndex);
            bi.Insert(index, changed);
            SetNewData(bi);
        }

        private bool GetMouseTargetRow(Visual theTarget, GetPosition position)
        {
            Rect rect;
            try
            {
                rect = VisualTreeHelper.GetDescendantBounds(theTarget);
            }
            catch
            {
                log.Trace("拖拽得太狠啦 - 2");
                return false;
            }
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
                if (itm != null)
                {
                    if (GetMouseTargetRow(itm, pos))
                    {
                        curIndex = i;
                        break;
                    }
                }
                else
                {
                    log.Trace("拖拽得太狠啦或拽到了AV号 - 1");
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
            popup1.PlacementRectangle = new Rect(new Point(e.GetPosition(this).X + 10, e.GetPosition(this).Y + 10), popupSize);

            Point position = e.GetPosition(dataGrid);
            var row = UIHelpers.TryFindFromPoint<DataGridRow>(dataGrid, position);
            if (row != null) dataGrid.SelectedItem = row.Item;
        }

        private async void buttonInsert_Click(object sender, RoutedEventArgs e)
        {
            BiliInterfaceInfo bi = await BiliInterface.GetInfoTaskAsync(textBoxInsert.Text);
            if (bi.pic != null) AddData(bi);
        }

        private async void buttonUndo_Click(object sender, RoutedEventArgs e)
        {
            if (lastItemSource == null)
            {
                var sampleMessageDialog = new SampleMessageDialog
                {
                    Message = { Text = "还没有做过任何修改哦~" }
                };
                await DialogHost.Show(sampleMessageDialog, "RootDialog");
                return;
            }
            SetNewData(lastItemSource);
        }

        private async void buttonBulkInsert_Click(object sender, RoutedEventArgs e)
        {
            log.Info("开始批量获取");
            var avs = SharedData.SortedAVs;
            ScoreType st = (ScoreType)((EnumerationExtension.EnumerationMember)comboBoxScoreType.SelectedItem).Value;
            BiliInterfaceInfo[] lls = await concurrentAsync(
                Convert.ToInt32(textBoxConcurrency.Text),
                avs,
                new Func<string, ScoreType, bool, Task<BiliInterfaceInfo>>(BiliInterface.GetInfoTaskAsync),
                st,
                toggleButtonUseKanb.IsChecked.GetValueOrDefault());
            List<BiliInterfaceInfo> ll = new List<BiliInterfaceInfo>();
            string failedAVs = "";
            foreach (BiliInterfaceInfo info in lls)
            {
                if (info.pic != null)
                {
                    ll.Add(info);
                }
                else
                {
                    failedAVs += info.avnum + ";";
                }
            }
            if (failedAVs != "")
            {
                log.Warn("注意！下列视频数据未正确获取！\r\n" + failedAVs);
            }
            AddData(ll);
            log.Info("批量获取完成");
        }

        private void userControlData_Loaded(object sender, RoutedEventArgs e)
        {
            comboBoxScoreType.SelectedIndex = 1;
        }

        private void buttonRerank_Click(object sender, RoutedEventArgs e)
        {
            List<BiliInterfaceInfo> ll = (List<BiliInterfaceInfo>)dataGrid.ItemsSource;
            ll?.Sort(sortt);
            for (int i = 1; i <= ll?.Count; i++)
            {
                ll[i - 1].Fpaiming = i;
            }
            SetNewData(ll);
        }

        private void buttonQuickCopy_Click(object sender, RoutedEventArgs e)
        {
            WindowQuickCopy wqc = new WindowQuickCopy(SharedData.Infos);
            wqc.Show();
        }

        private void menuItemDeleteLine_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)menuItem.Parent;
            var item = (DataGrid)contextMenu.PlacementTarget;
            var bi = item.SelectedCells[0].Item as BiliInterfaceInfo;
            RemoveData(bi);
        }

        private void buttonImportCsv_Click(object sender, RoutedEventArgs e)
        {
            List<KeyValuePair<string, string>> propmap = new List<KeyValuePair<string, string>>();
            foreach (var column in dataGrid.Columns)
            {
                if (!string.IsNullOrEmpty(column.SortMemberPath))
                {
                    if (column.SortMemberPath != "avurl")
                    {
                        propmap.Add(new KeyValuePair<string, string>(column.Header.ToString(), column.SortMemberPath));
                    }
                    else
                    {
                        propmap.Add(new KeyValuePair<string, string>("AV号", "AVNUM"));
                    }
                }
            }
            MessageBox.Show("为了兼容csv，目前的方法存在限制，文本中不能含有小写逗号\",\"，不可以含有小数\r\n请连同标题行复制到剪贴板后，点击确定", "锵锵锵");

            try
            {
                log.Info("开始读取剪贴板数据");
                var dataobject = Clipboard.GetDataObject();
                string data_csv = (string)dataobject.GetData(DataFormats.CommaSeparatedValue);
                string[] lines = Regex.Split(data_csv, "\r\n");
                string[] firstitems = Regex.Split(lines[0], ",");
                List<BiliInterfaceInfo> blist = new List<BiliInterfaceInfo>();

                Window window = new Window
                {
                    Title = "匹配",
                    Topmost = true,
                    Width = 300,
                    Height = 110 + 28 * firstitems.Length,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                Grid grid = new Grid();
                ComboBox[] comboBoxes = new ComboBox[firstitems.Length];
                StackPanel stackPanelAll = new StackPanel { Orientation = Orientation.Vertical };
                stackPanelAll.Children.Add(new TextBlock { Text = "请手动匹配未识别的标题行与数据类型", Margin = new Thickness(5, 10, 0, 10) });
                for (int i = 0; i <= firstitems.Length - 1; i++)
                {
                    StackPanel stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    stackPanel.Children.Add(new TextBlock { Text = firstitems[i], Margin = new Thickness(5, 0, 10, 0), VerticalAlignment = VerticalAlignment.Center });
                    comboBoxes[i] = new ComboBox
                    {
                        ItemsSource = propmap,
                        MinWidth = 100,
                        SelectedIndex = propmap.FindIndex(x => x.Key == firstitems[i])
                    };
                    stackPanel.Children.Add(comboBoxes[i]);
                    stackPanelAll.Children.Add(stackPanel);
                }
                Button buttonOK = new Button
                {
                    Content = "OK",
                    Margin = new Thickness(5, 0, 5, 5),
                    VerticalAlignment = VerticalAlignment.Bottom
                };
                buttonOK.Click += (ob, eb) =>
                {
                    window.Close();
                };
                grid.Children.Add(stackPanelAll);
                grid.Children.Add(buttonOK);
                window.Content = grid;
                window.ShowDialog();

                for (int i = 1; i <= lines.Length - 1; i++)
                {

                    string[] items = Regex.Split(lines[i], ",");
                    if (items.Length < firstitems.Length)
                    {
                        if (lines[i] != "\0")
                            log.Warn("该行数据不合法：" + lines[i]);
                        continue;
                    }
                    BiliInterfaceInfo info = new BiliInterfaceInfo();
                    for (int j = 0; j <= items.Length - 1; j++)
                    {
                        SetModelValue(((KeyValuePair<string, string>)comboBoxes[j].SelectedValue).Value, items[j], info);
                    }
                    blist.Add(info);
                }
                SetNewData(blist);
                string avs = "";
                foreach (BiliInterfaceInfo i in blist)
                {
                    avs += i.AVNUM + "\r\n";
                }
                SharedData.AVs = avs;
                log.Info("读取与转换完成");
            }
            catch (Exception ee)
            {
                log.Error("发生错误：" + ee.Message);
            }
        }

        public bool SetModelValue(string FieldName, string Value, object obj)
        {
            try
            {
                Type Ts = obj.GetType();
                object v = TypeDescriptor.GetConverter(Ts.GetProperty(FieldName).PropertyType).ConvertFrom(Value);
                //object v = Convert.ChangeType(Value, Ts.GetProperty(FieldName).PropertyType);
                Ts.GetProperty(FieldName).SetValue(obj, v, null);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async void buttonGetPicUrl_Click(object sender, RoutedEventArgs e)
        {
            log.Info("开始更新封面头像地址（非下载）");
            ScoreType st = (ScoreType)((EnumerationExtension.EnumerationMember)comboBoxScoreType.SelectedItem).Value;
            IEnumerable<string> avs = SharedData.SortedAVs;
            BiliInterfaceInfo[] lls = await concurrentAsync(
                Convert.ToInt32(textBoxConcurrency.Text), //现在有限制了，不能弄那么快了
                avs,
                new Func<string, ScoreType, bool, Task<BiliInterfaceInfo>>(BiliInterface.GetInfoTaskAsync),
                st,
                toggleButtonUseKanb.IsChecked.GetValueOrDefault());
            List<BiliInterfaceInfo> ll = new List<BiliInterfaceInfo>();
            string failedAVs = "";
            foreach (BiliInterfaceInfo info in lls)
            {
                if (info.pic != null)
                {
                    BiliInterfaceInfo oldinfo = SharedData.Infos.Where(x => x.avnum == info.avnum).First();
                    oldinfo.pic = info.pic;
                    oldinfo.face = info.face;
                    oldinfo.mid = info.mid;
                }
                else
                {
                    failedAVs += info.avnum + ";";
                }
            }
            if (failedAVs != "")
            {
                log.Warn("注意！下列视频数据未正确获取！\r\n" + failedAVs);
            }
            log.Info("更新封面头像地址完成");
        }
    }
}
