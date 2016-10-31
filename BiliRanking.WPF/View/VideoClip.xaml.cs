using Microsoft.Win32;
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
    /// VideoClip.xaml 的交互逻辑
    /// </summary>
    public partial class VideoClip : UserControl
    {
        public VideoClip()
        {
            InitializeComponent();

            Player.TimeChanged += Player_TimeChanged;
        }

        private void Player_TimeChanged(object sender, EventArgs e)
        {
            textBlock1.Text = Player.Time.ToString();
        }

        private void ProgressBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var value = (float)(e.GetPosition(ProgressBar).X / ProgressBar.ActualWidth);
            ProgressBar.Value = value;
        }

        private void buttonLoad_Click(object sender, RoutedEventArgs e)
        {
            
            var openfiles = new OpenFileDialog();
            if (openfiles.ShowDialog() == true)
            {
                Player.Stop();
                Player.LoadMedia(openfiles.FileName);
                Player.Play();
            }
            return;

            //String pathString = path.Text;

            Uri uri = null;
            //if (!Uri.TryCreate(pathString, UriKind.Absolute, out uri)) return;

            //Player.Stop();
            //Player.LoadMedia(uri);
            //if you pass a string instead of a Uri, LoadMedia will see if it is an absolute Uri, else will treat it as a file path
            //Player.Play();
        }

        private void buttonPlay_Click(object sender, RoutedEventArgs e)
        {
            Player.Play();
        }

        private void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            Player.Pause();
        }

        private void buttonStepForward_Click(object sender, RoutedEventArgs e)
        {
            
            //Player.NextFrame();
            Player.Time = Player.Time.Add(new TimeSpan(0, 0, 0, 0, 200));
        }

        private void buttonSkipForward_Click(object sender, RoutedEventArgs e)
        {
            textBlock1.Text = Player.Time.ToString();
        }
    }
}
