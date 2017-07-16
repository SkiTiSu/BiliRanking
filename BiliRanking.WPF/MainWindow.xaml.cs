using BiliRanking.Core;
using BiliRanking.WPF.Domain;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
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

        bool isSupportBlur = true;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Version currentOSVer = Environment.OSVersion.Version;
            log.Debug($"OS Version: {currentOSVer}");
            if (currentOSVer >= new Version(10, 0))
            {
                Task.Delay(50).ContinueWith(_ =>
                {
                    this.Dispatcher.Invoke(() => {
                        EnableBlurWin10();
                        }); 
                });
            }
            else if (currentOSVer == new Version(6, 1))
            {
                Task.Delay(200).ContinueWith(_ =>
                {
                    this.Dispatcher.Invoke(() => {
                        EnableBlurWin7();
                    });
                });
                log.Warn("Win7放弃支持毛玻璃，即使有毛玻璃会有几道光，且会根据主题颜色变色，赶紧升级Win10吧！");
            }
            else
            {
                isSupportBlur = false;
                this.Background = new SolidColorBrush(Color.FromArgb(255, 250, 250, 250));
                log.Warn("还没用Win10啊，不能感受毛玻璃效果了，赶紧升级吧！");
            }

            BiliApiHelper.access_key = Properties.Settings.Default.access_key;

            bool isTryLogin = true;

            if (!string.IsNullOrEmpty(BiliApiHelper.access_key))
            {
                log.Info("已通过配置文件读取到授权码");
                log.Debug("授权码：" + BiliApiHelper.access_key);
            }
            else
            {
                log.Warn("没有获取到授权码，里区将对你躲♂藏");
                isTryLogin = false;
            }

            Task.Run(async () =>
            {
                if (isTryLogin)
                {
                    BiliUser bu = new BiliUser();
                    UserInfoModel um = await bu.GetMyUserInfo();
                    if (um.uname != null)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            log.Info("授权码有效，登录账户名：" + um.uname);
                            UserInfoName.Content = um.uname;
                            UserInfoAvatar.Source = new BitmapImage(new Uri(um.face));

                            UserInfoOther.Text = $"{um.RankStr} LV{um.level_info.current_level} 硬币:{um.coins}";
                        });
                    }
                    else
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            log.Warn("授权码已经失效");
                            UserInfoName.Content = "授权码已经失效！";
                        });
                    }
                }

                if (!Directory.Exists(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\pic\"))
                {
                    log.Info("未检测到封面存放目录，正在创建\\pic");
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\pic\");
                }

                if (!Directory.Exists(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\video\"))
                {
                    log.Info("未检测到视频存放目录，正在创建\\video");
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\video\");
                }
            });

            Updater up = new Updater();
            up.CheckUpdate();
            textBlockTitle.Text = $"BiliRanking V{up.Version} - 不 止 统 计";
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            try
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
            catch { }
        }

        #region Blur for Windows 10
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        internal void EnableBlurWin10()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }
        #endregion

        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (isSupportBlur)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.Background = new SolidColorBrush(Color.FromArgb(255, 250, 250, 250));
                }
                else
                {
                    this.Background = new SolidColorBrush(Color.FromArgb(178, 250, 250, 250));
                }
            }
        }

        #region Blur for Windows 7 
        private const int WM_DWMCOMPOSITIONCHANGED = 0x031E;
        private const int DWM_BB_ENABLE = 0x1;

        [StructLayout(LayoutKind.Sequential)]
        private struct DWM_BLURBEHIND
        {
            public int dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern void DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWM_BLURBEHIND blurBehind);
        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMargins);
        [DllImport("dwmapi.dll", PreserveSig = false)]
        private static extern bool DwmIsCompositionEnabled();

        private void EnableBlurWin7()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var hs = HwndSource.FromHwnd(hwnd);
            hs.CompositionTarget.BackgroundColor = System.Windows.Media.Colors.Transparent;

            var margins = new MARGINS();
            margins.cxLeftWidth = margins.cxRightWidth = margins.cyTopHeight = margins.cyBottomHeight = -1;

            DwmExtendFrameIntoClientArea(hwnd, ref margins);

            DWM_BLURBEHIND bbh = new DWM_BLURBEHIND();
            bbh.fEnable = true;
            bbh.dwFlags = DWM_BB_ENABLE;
            DwmEnableBlurBehindWindow(hwnd, ref bbh);
        }
        #endregion

    }
    
    //https://stackoverflow.com/questions/43962325/custom-caption-buttons-in-wpf
    public class SCommandHelper
    {
        public static bool GetUseWindowCommandBindings(DependencyObject obj)
        {
            return (bool)obj.GetValue(UseWindowCommandBindingsProperty);
        }

        public static void SetUseWindowCommandBindings(DependencyObject obj, bool value)
        {
            obj.SetValue(UseWindowCommandBindingsProperty, value);
        }

        public static readonly DependencyProperty UseWindowCommandBindingsProperty =
            DependencyProperty.RegisterAttached("UseWindowCommandBindings", typeof(bool), typeof(SCommandHelper), new PropertyMetadata(false, UseWindowCommandBindingsChanged));

        private static void UseWindowCommandBindingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs dpce)
        {
            if (d is Window w && dpce.NewValue is bool b && b)
            {
                w.CommandBindings.Add(
                    new CommandBinding(
                        SystemCommands.MinimizeWindowCommand,
                        (s, e) => SystemCommands.MinimizeWindow(w),
                        (s, e) => e.CanExecute = true
                        ));
                w.CommandBindings.Add(
                    new CommandBinding(
                        SystemCommands.RestoreWindowCommand,
                        (s, e) => SystemCommands.RestoreWindow(w),
                        (s, e) => e.CanExecute = true
                        ));
                w.CommandBindings.Add(
                    new CommandBinding(
                        SystemCommands.MaximizeWindowCommand,
                        (s, e) => SystemCommands.MaximizeWindow(w),
                        (s, e) => e.CanExecute = true
                        ));
                w.CommandBindings.Add(
                    new CommandBinding(
                        SystemCommands.CloseWindowCommand,
                        (s, e) => SystemCommands.CloseWindow(w),
                        (s, e) => e.CanExecute = true
                        ));
            }
        }
    }
}
