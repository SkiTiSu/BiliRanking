using BiliRanking.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BiliRanking.WPF.View
{
    /// <summary>
    /// WindowQuickCopy.xaml 的交互逻辑
    /// </summary>
    public partial class WindowQuickCopy : Window
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        List<BiliInterfaceInfo> binfos;
        private GlobalKeyboardHook globalKeyboardHook;

        public WindowQuickCopy(IEnumerable<BiliInterfaceInfo> infos)
        {
            binfos = infos.ToList();
            InitializeComponent();

            globalKeyboardHook = new GlobalKeyboardHook();
            globalKeyboardHook.KeyboardPressed += OnKeyPressed;
        }

        private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            //Debug.WriteLine(e.KeyboardData.VirtualCode);

            //if (e.KeyboardData.VirtualCode != GlobalKeyboardHook.VkSnapshot)
            //    return;

            // seems, not needed in the life.
            //if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.SysKeyDown &&
            //    e.KeyboardData.Flags == GlobalKeyboardHook.LlkhfAltdown)
            //{
            //    MessageBox.Show("Alt + Print Screen");
            //    e.Handled = true;
            //}
            //else

            //TODO:加入选项是否完全拦截
            if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown && e.KeyboardData.VirtualCode == (int)Keys.Left)
            {
                buttonLeft_Click(null, null);
                e.Handled = true;
            }
            else if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown && e.KeyboardData.VirtualCode == (int)Keys.Right)
            {
                buttonRight_Click(null, null);
                e.Handled = true;
            }
        }

        public void DoFresh()
        {
            string copytext = DoReplace(comboBoxTemplate.Text, binfos[int.Parse(textBoxFPaiming?.Text ?? "1") - 1]);
            textBoxResult.Text = copytext;
        }

        public string DoReplace(string before, BiliInterfaceInfo info)
        {
            string after = before;

            after = after.Replace("<", "{");
            after = after.Replace(">", "}");

            after = after.Replace("{}", "\r\n");

            after = after.Replace("{huanhang}", "\r\n");
            after = after.Replace("{title}", info.title);
            after = after.Replace("{time}", info.created_at.Replace(" ", "　")); //半角换成全角
            after = after.Replace("{created_at}", info.created_at);
            after = after.Replace("{avnum}", info.avnum);
            after = after.Replace("{author}", info.author);
            after = after.Replace("{zongfen}", info.Fdefen.ToString());
            after = after.Replace("{bofang}", info.play.ToString());
            after = after.Replace("{yingbi}", info.coins.ToString());
            after = after.Replace("{shoucang}", info.favorites.ToString());
            after = after.Replace("{danmu}", info.video_review.ToString());
            after = after.Replace("{pinglun}", info.review.ToString());
            after = after.Replace("{tag}", info.tag.ToString());
            //after = after.Replace("{code}", GenCodeDanmaku(info));
            //after = after.Replace("{timecode}", GenTimecode(info));

            after = after.Replace("{换行}", "\r\n");
            after = after.Replace("{标题}", info.title);
            after = after.Replace("{时间}", info.created_at.Replace(" ", "　")); //半角换成全角
            after = after.Replace("{半角时间}", info.created_at);
            after = after.Replace("{av号}", info.avnum); after = after.Replace("{AV号}", info.avnum);
            after = after.Replace("{作者}", info.author); after = after.Replace("{UP}", info.author);
            after = after.Replace("{总分}", info.Fdefen.ToString());
            after = after.Replace("{播放}", info.play.ToString());
            after = after.Replace("{硬币}", info.coins.ToString());
            after = after.Replace("{收藏}", info.favorites.ToString());
            after = after.Replace("{弹幕}", info.video_review.ToString());
            after = after.Replace("{评论}", info.review.ToString());
            //after = after.Replace("{代码}", GenCodeDanmaku(info));
            //after = after.Replace("{时间码}", GenTimecode(info));

            return after;
        }

        private void Copy()
        {
            if (textBoxResult.Text != null)
            {
                try
                {
                    System.Windows.Clipboard.SetText(textBoxResult.Text);
                }
                catch
                {
                    Log.Warn("剪贴板访问错误，使用方法2");
                    try
                    {
                        textBoxResult.SelectAll();
                        textBoxResult.Copy();
                    }
                    catch
                    {
                        log.Error("两种复制方法均失效，请手动复制");
                    }
                }
            }
        }

        private void buttonCopy_Click(object sender, RoutedEventArgs e)
        {
            DoFresh();
            //Copy(); //是否需要？
        }

        private void buttonLeft_Click(object sender, RoutedEventArgs e)
        {
            comboBoxTemplate.SelectedIndex = ((comboBoxTemplate.SelectedIndex - 1) != -1) ? comboBoxTemplate.SelectedIndex - 1 : 0;
        }

        private void buttonRight_Click(object sender, RoutedEventArgs e)
        {
            comboBoxTemplate.SelectedIndex = ((comboBoxTemplate.SelectedIndex) != (comboBoxTemplate.Items.Count - 1)) ? comboBoxTemplate.SelectedIndex + 1 : comboBoxTemplate.Items.Count - 1;
        }

        private void textBoxResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            Copy();
        }

        private void buttonFPaimingUp_Click(object sender, RoutedEventArgs e)
        {
            int a = int.Parse(textBoxFPaiming.Text);
            textBoxFPaiming.Text = (((a - 1) > 0) ? a - 1 : 1).ToString();
        }

        private void buttonFPaimingDown_Click(object sender, RoutedEventArgs e)
        {
            int a = int.Parse(textBoxFPaiming.Text);
            if ((a + 1) <= binfos.Count)
            {
                textBoxFPaiming.Text = (a + 1).ToString();
            }
        }

        private void comboBoxTemplate_TextChanged(object sender, TextChangedEventArgs e)
        {
            DoFresh();
        }

        private void textBoxFPaiming_TextChanged(object sender, TextChangedEventArgs e)
        {
            DoFresh();
        }
    }
}
