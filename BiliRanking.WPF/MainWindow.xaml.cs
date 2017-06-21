using BiliRanking.Core;
using BiliRanking.WPF.Domain;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BiliRanking.WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        public MainWindow()
        {
            InitializeComponent();

            textBlockTitle.Text += " build " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            AllocConsole();
        }

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //until we had a StaysOpen glag to Drawer, this will help with scroll bars
            var dependencyObject = Mouse.Captured as DependencyObject;
            while (dependencyObject != null)
            {
                if (dependencyObject is ScrollBar) return;
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            MenuToggleButton.IsChecked = false;
        }

        private async void MenuPopupButton_OnClick(object sender, RoutedEventArgs e)
        {
            var sampleMessageDialog = new SampleMessageDialog
            {
                Message = { Text = ((ButtonBase)sender).Tag.ToString() }
            };

            await DialogHost.Show(sampleMessageDialog, "RootDialog");
        }

        private void buttonAVsShowHidden_Click(object sender, RoutedEventArgs e)
        {
            Storyboard storyboard = new Storyboard();

            ThicknessAnimationUsingKeyFrames takf = new ThicknessAnimationUsingKeyFrames();

            if (gridAVs.Margin.Right < 0)
            {
                takf.KeyFrames.Add(new SplineThicknessKeyFrame(new Thickness(0, 0, -130, 0), KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0))));
                takf.KeyFrames.Add(new SplineThicknessKeyFrame(new Thickness(0, 0, 0, 0), KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 750)), new KeySpline(0.5, 0.75, 0, 1)));
            }
            else
            {
                takf.KeyFrames.Add(new SplineThicknessKeyFrame(new Thickness(0, 0, 0, 0), KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0))));
                takf.KeyFrames.Add(new SplineThicknessKeyFrame(new Thickness(0, 0, -130, 0), KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 750)), new KeySpline(0.5, 0.75, 0, 1)));
            }


            Storyboard.SetTargetProperty(takf, new PropertyPath("Margin"));
            storyboard.Children.Add(takf);
            storyboard.Begin(gridAVs);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BiliApiHelper.access_key = Properties.Settings.Default.access_key;

            if (!string.IsNullOrEmpty(BiliApiHelper.access_key))
            {
                log.Info("已通过配置文件读取到授权码");
                log.Debug("授权码：" + BiliApiHelper.access_key);
            }
            else
            {
                log.Warn("没有获取到授权码，里区将对你躲♂藏");
                return;
            }

            BiliUser bu = new BiliUser();
            UserInfoModel um = await bu.GetMyUserInfo();
            if (um.uname != null)
            {
                log.Info("授权码有效，登录账户名：" + um.uname);
                UserInfoName.Content = um.uname;
                UserInfoAvatar.Source = new BitmapImage(new Uri(um.face));

                UserInfoOther.Text = $"{um.RankStr} LV{um.level_info.current_level} 硬币:{um.coins}";
            }
            else
            {
                log.Warn("授权码已经失效");
                UserInfoName.Content = "授权码已经失效！";
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string fileName = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            log.Info($"拖入文件{fileName}");
            Item shuju = (Item)GetItem("数据获取");
            listBoxItems.SelectedItem = shuju;
            var shujushili = (View.Data)shuju.Content;
            shujushili.OpenFile(fileName);

            object GetItem(string name)
            {
                foreach (var item in listBoxItems.Items)
                {
                    if (((Item)item).Name == name)
                    {
                        return item;
                    }
                }
                return null;
            }
        }
    }
}
