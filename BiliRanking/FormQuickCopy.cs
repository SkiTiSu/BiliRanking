using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BiliRanking.Core;

namespace BiliRanking
{
    public partial class FormQuickCopy : Form
    {
        List<BiliInterfaceInfo> binfos;
        private GlobalKeyboardHook globalKeyboardHook;
        public FormQuickCopy()
        {
            InitializeComponent();

            globalKeyboardHook = new GlobalKeyboardHook();
            globalKeyboardHook.KeyboardPressed += OnKeyPressed;

            comboBoxLifeTime.SelectedIndex = 0;
        }

        public FormQuickCopy(List<BiliInterfaceInfo> infos)
            : this()
        {
            binfos = infos;
            reverseUpDownFpaiming.Value = 1;
            reverseUpDownFpaiming.Maximum = binfos.Count;
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
                button1_Click(null, null);
                e.Handled = true;
            }
            else if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown && e.KeyboardData.VirtualCode == (int)Keys.Right)
            {
                button2_Click(null, null);
                e.Handled = true;
            }
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
            after = after.Replace("{code}", GenCodeDanmaku(info));
            after = after.Replace("{timecode}", GenTimecode(info));

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
            after = after.Replace("{代码}", GenCodeDanmaku(info));
            after = after.Replace("{时间码}", GenTimecode(info));

            return after;
        }

        private string GenCodeDanmaku(BiliInterfaceInfo info)
        {
            string res = @"vtt=$.createButton(
{
    lifeTime:{lifetime},
    x:Player.width-180,
    width:150,
    y:Player.height-40,
    height:30,
    text:""跳转到该投稿"",
    onclick: function()
    {
        Player.jump(""{avnum}"", 1, true);
    }
});
v32.setStyle(""fillColors"",[0xFF0000,0xCCCCCC]);";

            res = res.Replace("{avnum}", info.avnum);

            return res;
        }

        private string GenTimecode(BiliInterfaceInfo info)
        {
            try
            {
                return (TimeSpan.Parse(info.Tstart.Substring(0, info.Tstart.LastIndexOf(":"))).TotalSeconds + 2).ToString();
                //return (TimeSpan.Parse(info.Tstart.Split('.')[0]).TotalSeconds + 3).ToString();
            }
            catch
            {
                return "时间码解析失败";
            }
        }

        public void DoFresh()
        {
            string copytext = DoReplace(comboBox1.Text ,binfos[(int)reverseUpDownFpaiming.Value - 1]);
            textBoxResult.Text = copytext;
            Clipboard.SetText(copytext);
            textBoxResult.Copy();
        }

        private void reverseUpDownFpaiming_ValueChanged(object sender, EventArgs e)
        {
            labelNow.Text = binfos[(int)reverseUpDownFpaiming.Value - 1].title;
            DoFresh();
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            Copy();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DoFresh();
        }

        private void FormQuickCopy_Load(object sender, EventArgs e)
        {
            labelNow.Text = binfos[(int)reverseUpDownFpaiming.Value - 1].title;
            DoFresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = ((comboBox1.SelectedIndex - 1) != -1) ? comboBox1.SelectedIndex - 1 : 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = ((comboBox1.SelectedIndex) != (comboBox1.Items.Count - 1)) ? comboBox1.SelectedIndex + 1 : comboBox1.Items.Count - 1;
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            DoFresh();
        }

        private void FormQuickCopy_FormClosing(object sender, FormClosingEventArgs e)
        {
            globalKeyboardHook?.Dispose();
        }

        private void buttonLifetime12_Click(object sender, EventArgs e)
        {
            comboBoxLifeTime.Text = "22";
        }

        private void buttonLifetime30_Click(object sender, EventArgs e)
        {
            comboBoxLifeTime.Text = "34";
        }

        private void textBoxResult_TextChanged(object sender, EventArgs e)
        {
            Copy();
        }

        private void Copy()
        {
            if (textBoxResult.Text != null)
            {
                try
                {
                    Clipboard.SetText(textBoxResult.Text);
                }
                catch
                {
                    Log.Warn("剪贴板访问错误，使用方法2");
                    textBoxResult.SelectAll();
                    textBoxResult.Copy();
                }
            }
        }

        string lastLifeTime = "{lifetime}";
        private void comboBoxLifeTime_TextChanged(object sender, EventArgs e)
        {
            textBoxResult.Text = textBoxResult.Text.Replace("{lifetime}", comboBoxLifeTime.Text);
            textBoxResult.Text = textBoxResult.Text.Replace("lifeTime:" + lastLifeTime, "lifeTime:" + comboBoxLifeTime.Text);
            lastLifeTime = comboBoxLifeTime.Text;
        }
    }
}
