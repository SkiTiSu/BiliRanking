using BiliRanking.WPF.Domain;
using MaterialDesignThemes.Wpf;
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
                Message = { Text = ((ButtonBase)sender).Content.ToString() }
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
                takf.KeyFrames.Add(new SplineThicknessKeyFrame(new Thickness(0, 0, 0, 0), KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0,0,750)), new KeySpline(0.5, 0.75, 0, 1)));
            }
            else
            {
                takf.KeyFrames.Add(new SplineThicknessKeyFrame(new Thickness(0, 0, 0, 0), KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0))));
                takf.KeyFrames.Add(new SplineThicknessKeyFrame(new Thickness(0, 0, -130, 0), KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0,0,750)), new KeySpline(0.5, 0.75, 0, 1)));
            }


            Storyboard.SetTargetProperty(takf, new PropertyPath("Margin"));
            storyboard.Children.Add(takf);
            storyboard.Begin(gridAVs);
        }
    }
}
