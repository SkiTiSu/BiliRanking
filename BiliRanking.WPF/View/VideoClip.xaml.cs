//using Meta.Vlc;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace BiliRanking.WPF.View
{
    public class diyProp : DependencyObject
    {
        public static int GetTimeChange(DependencyObject obj)
        {
            return (int)obj.GetValue(TimeChangeProperty);
        }

        public static void SetTimeChange(DependencyObject obj, int value)
        {
            obj.SetValue(TimeChangeProperty, value);
        }

        // Using a DependencyProperty as the backing store for TimeChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeChangeProperty =
            DependencyProperty.RegisterAttached("TimeChange", typeof(int), typeof(diyProp), new PropertyMetadata(0));

    }
    /// <summary>
    /// VideoClip.xaml 的交互逻辑
    /// </summary>
    public partial class VideoClip : UserControl
    {
        TimeSpan markStart = new TimeSpan();
        TimeSpan markEnd = new TimeSpan();
        float frameRate = 25;
        public VideoClip()
        {
            InitializeComponent();

            //Player.TimeChanged += Player_TimeChanged;
        }

        private void Player_TimeChanged(object sender, EventArgs e)
        {
            //textkBlock1.Text = Player.Time.ToString();
        }

        private void ProgressBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var value = (float)(e.GetPosition(ProgressBar).X / ProgressBar.ActualWidth);
            ProgressBar.Value = value;
            FreshTime();
        }

        private void buttonLoad_Click(object sender, RoutedEventArgs e)
        {

            var openfiles = new OpenFileDialog();
            if (openfiles.ShowDialog() == true)
            {
                //Player.Stop();
                //Player.LoadMedia(openfiles.FileName);
                //Player.Play();
            }
            return;

            //String pathString = path.Text;

            //Uri uri = null;
            //if (!Uri.TryCreate(pathString, UriKind.Absolute, out uri)) return;

            //Player.Stop();
            //Player.LoadMedia(uri);
            //if you pass a string instead of a Uri, LoadMedia will see if it is an absolute Uri, else will treat it as a file path
            //Player.Play();
        }

        private void buttonPlay_Click(object sender, RoutedEventArgs e)
        {
            //Player.Play();
        }

        private void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            //Player.Pause();
        }

        private void buttonStepForward_Click(object sender, RoutedEventArgs e)
        {

            //Player.NextFrame();

            //Player.Play();
            //Player.Pause();

            //try
            //{
            //    frameRate = (Player.VlcMediaPlayer.Media.GetTracks()[0] as VideoTrack).FrameRateNum / 1000;
            //}
            //catch
            //{
            //    frameRate = 25;
            //}
            //frameRate = 25;
            //int step = Convert.ToInt32(1000 / frameRate);
            //if (Player.Time.Milliseconds % step != 0)
            //{
            //    Player.Time = Player.Time.Add(new TimeSpan(0, 0, 0, 0, -(Player.Time.Milliseconds % step)));
            //}
            //Player.VlcMediaPlayer.Time = Player.VlcMediaPlayer.Time.Add(new TimeSpan(0, 0, 0, 0, step));

            FreshTime();
        }

        private void FreshTime()
        {
            //textBlock1.Text = Player.Time.ToString();

            //if (markEnd == new TimeSpan())
            //{
            //    markEnd = Player.Length;
            //}

            //Rectangle r = new Rectangle();
            //r.Fill = new SolidColorBrush(Colors.Brown);
            //r.Height = 5;
            //r.Width = ((markEnd.TotalMilliseconds - markStart.TotalMilliseconds) / Player.Length.TotalMilliseconds) * canvasTimeline.ActualWidth;
            //r.SetValue(Canvas.LeftProperty, (double)(markStart.TotalMilliseconds / Player.Length.TotalMilliseconds) * canvasTimeline.ActualWidth);

            //canvasTimeline.Children.Clear();
            //canvasTimeline.Children.Add(r);
        }

        private void buttonStepBackward_Click(object sender, RoutedEventArgs e)
        {
            int step = Convert.ToInt32(1000 / frameRate);
            //Player.Time = Player.Time.Add(new TimeSpan(0, 0, 0, 0, -step));

            FreshTime();
        }

        private void buttonMarkStart_Click(object sender, RoutedEventArgs e)
        {
            //markStart = Player.Time;
            FreshTime();
        }

        private void buttonMarkEnd_Click(object sender, RoutedEventArgs e)
        {
            //markEnd = Player.Time;
            FreshTime();
        }
        //Process p = new Process();
        private void buttonRender_Click(object sender, RoutedEventArgs e)
        {
            ////string fullName = Player.VlcMediaPlayer.Media.Mrl.Substring(8);
            //string directoryName = System.IO.Path.GetDirectoryName(fullName);
            //string fileName = System.IO.Path.GetFileName(fullName);
            //string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fullName);
            //string extension = System.IO.Path.GetExtension(fullName);

            ////string command = $"-i \"{fullName}\" -ss {markStart.ToString()} -to {markEnd.ToString()} -vcodec libx264 -acodec copy -crf 18 \"{System.IO.Path.Combine(directoryName, "CLIP_" + fileName)}\"";
            dialogProcessing.IsOpen = true;
            Process p = new Process();
            p.StartInfo.FileName = Environment.CurrentDirectory + "\\ffmpeg.exe";
            //p.StartInfo.Arguments = command;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.ErrorDataReceived += new DataReceivedEventHandler(Output);
            p.Start();
            p.BeginErrorReadLine();
            p.EnableRaisingEvents = true;
            p.Exited += new EventHandler(CmdProcess_Exited);
            //Clipboard.SetText(command);
        }
        private void Output(object sendProcess, DataReceivedEventArgs output)
        {
            if (!String.IsNullOrEmpty(output.Data))
            {
                Debug.WriteLine(output.Data);
                textBlockProcessing.Dispatcher.BeginInvoke(
                new Action(
                delegate
                {
                    textBlockProcessing.Text = output.Data;
                }));
            }
        }

        private void buttonTest_Click(object sender, RoutedEventArgs e)
        {
            dialogProcessing.IsOpen = true;
        }

        private void CmdProcess_Exited(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(
                new Action(
                delegate
                {
                    dialogProcessing.IsOpen = false;
                }));
        }

        private void buttonMarkStartN_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            //markStart = Player.Time + new TimeSpan(0, 0, Convert.ToInt32(((string)btn.Content).Substring(0, ((string)btn.Content).Length - 1)));
            FreshTime();
        }
    }
}
