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
        public FormQuickCopy()
        {
            InitializeComponent();
        }

        public FormQuickCopy(List<BiliInterfaceInfo> infos)
        {
            binfos = infos;
            InitializeComponent();
            numericUpDownFpaiming.Value = 1;
        }

        public string DoReplace(string before, BiliInterfaceInfo info)
        {
            string after = before;

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

            return after;
        }

        public void DoFresh()
        {
            string copytext = DoReplace(comboBox1.Text ,binfos[(int)numericUpDownFpaiming.Value - 1]);
            labelResult.Text = copytext;
            Clipboard.SetText(copytext);
        }

        private void numericUpDownFpaiming_ValueChanged(object sender, EventArgs e)
        {
            labelNow.Text = binfos[(int)numericUpDownFpaiming.Value - 1].title;
            DoFresh();
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            string copytext = DoReplace(comboBox1.Text, binfos[(int)numericUpDownFpaiming.Value - 1]);
            Clipboard.SetText(copytext);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DoFresh();
        }

        private void FormQuickCopy_Load(object sender, EventArgs e)
        {
            labelNow.Text = binfos[(int)numericUpDownFpaiming.Value - 1].title;
            DoFresh();
        }
    }
}
