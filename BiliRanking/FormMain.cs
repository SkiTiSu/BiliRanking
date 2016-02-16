using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using BiliRanking.Properties;

namespace BiliRanking
{
    public partial class FormMain : Form
    {
        public static string cookie;

        public FormMain()
        {
            InitializeComponent();
            this.Icon = Resources.logo;
            cookie = textBoxCookie.Text;
            dataGridViewRAW.AutoGenerateColumns = false;
            comboBoxListNum.SelectedIndex = 0;
            comboBoxTagZone.SelectedIndex = 0;
        }

        private void textBoxCookie_TextChanged(object sender, EventArgs e)
        {
            cookie = textBoxCookie.Text;
            webBrowser1.Document.Cookie = textBoxCookie.Text;
            Log.Info("Cookie已被更改为：" + textBoxCookie.Text);
        }

        private void buttonCookieHelp_Click(object sender, EventArgs e)
        {
            FormCookieHelp f = new FormCookieHelp();
            f.ShowDialog();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists(System.Environment.CurrentDirectory + @"\pic\"))
            {
                Log.Info("未检测到封面存放目录，正在创建\\pic");
                Directory.CreateDirectory(System.Environment.CurrentDirectory + @"\pic\");
            }

            if (!Directory.Exists(System.Environment.CurrentDirectory + @"\video\"))
            {
                Log.Info("未检测到视频存放目录，正在创建\\video");
                Directory.CreateDirectory(System.Environment.CurrentDirectory + @"\video\");
            }

            comboBoxListSort.DataSource = Enum.GetNames(typeof(BiliParse.SortType));
            comboBoxListZone.SelectedIndex = 0;
        }

        private void buttonListGen_Click(object sender, EventArgs e)
        {
            Log.Info("开始获取排行");
            BiliParse.SortType sort = (BiliParse.SortType)Enum.Parse(typeof(BiliParse.SortType), comboBoxListSort.SelectedItem.ToString(), false);
            int needpage = Convert.ToInt32(comboBoxListNum.Text) / 20;
            if ((Convert.ToInt32(comboBoxListNum.Text) % 20) != 0)
                needpage += 1;

            List<string> ss = new List<string>();

            for (int i = 1; i <= needpage; i++)
            {
                List<string> sts = BiliParse.GetList(sort, 119, i, dateTimePickerFrom.Value, dateTimePickerTo.Value);
                if (sts != null)
                    ss.AddRange(sts);
                else
                    break;
            }
            
            textBoxAV.Text = "";

            for (int i = 1; i <= Convert.ToInt32(comboBoxListNum.Text); i++)
            {
                try
                {
                    textBoxAV.Text += ss[i - 1] + "\r\n";
                }
                catch
                {
                    Log.Warn("选定区间内视频数量不满" + comboBoxListNum.Text + "个！仅有" + i.ToString() + "个。");
                    break;
                }
            }
            Log.Info("获取排行完成");
        }

        private void buttonGen_Click(object sender, EventArgs e)
        {
            Log.Info("开始批量获取");
            string[] lines = Regex.Split(textBoxAV.Text, "\r\n|\r|\n");
            List<BiliInterfaceInfo> ll = new List<BiliInterfaceInfo>();
            //dataGridViewRAW.DataSource = ll;
            //Gen(lines);

            textBoxOut.Text = "AV号,标题,播放数,弹幕数,收藏数,硬币数,评论数,up,时间,分区,播放得分,收藏得分,硬币得分,评论得分,总分\r\n";
            foreach (string s in lines)
            {
                if (s != "")
                {
                    BiliInterfaceInfo info = BiliInterface.GetInfo(s);
                    //System.Threading.Thread.Sleep(1000);
                    if (info.pic != null)
                    {
                        ll.Add(info);
                        textBoxOut.Text += GenHang(new string[] { s, info.title, info.play.ToString(), info.video_review.ToString(), info.favorites.ToString(), info.coins.ToString(),
                            info.review.ToString(), info.author, info.created_at, info.typename,
                            info.Fplay.ToString(), info.Ffavorites.ToString(), info.Fcoins.ToString(), info.Freview.ToString(), info.Fdefen.ToString() });
                        textBoxOut.Text += "\"\r\n";
                        Application.DoEvents();
                    }
                    else if (info.AVNUM != null)
                    {
                        ll.Add(info);
                    }
                }
            }

            ll.Sort(sortt);
            for (int i = 1; i <= ll.Count; i++)
            {
                ll[i - 1].Fpaiming = i;
            }
            dataGridViewRAW.DataSource = ll;
            
            Log.Info("批量获取完成");
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

        string GenHang(string[] hangs)
        {
            string r = "\"";
            foreach (string hang in hangs)
            {
                r += hang + "\",\"";
            }

            return r.Substring(0, r.Length - 3);
        }

        private void buttonPic_Click(object sender, EventArgs e)
        {
            Log.Info("开始批量获取");
            string[] lines = Regex.Split(textBoxAV.Text, "\r\n|\r|\n");
            foreach (string s in lines)
            {
                if (s != "")
                {
                    BiliInterface.GetPic(s);
                }
                //System.Threading.Thread.Sleep(1000);
            }
            Log.Info("批量获取完成");
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            string fileName = "BiliRanking.csv";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fileName = saveFileDialog1.FileName;
                try
                {
                    StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8);
                    sw.Write(textBoxOut.Text);
                    sw.Close();
                    Log.Info("成功导出文件 -> " + fileName);
                    Log.Info("请通过Excel打开并另存为xlsx文件");
                }
                catch
                {
                    Log.Error("文件保存错误！检查权限");
                }
            }

        }

        private void buttonAVClear_Click(object sender, EventArgs e)
        {
            textBoxAV.Text = "";
        }

        TSDownload tsd = new TSDownload();
        List<BiliInterfaceInfo> listb = new List<BiliInterfaceInfo>();
        List<string> listh5 = new List<string>();

        private void buttonDlMP4_Click(object sender, EventArgs e)
        {
            string[] lines = Regex.Split(textBoxAV.Text, "\r\n|\r|\n");

            timer1.Enabled = true;
            tsd.Progressbar = verticalProgressBar1;

            Log.Info("获取所有视频MP4地址");

            foreach (string s in lines)
            {
                if (s != "")
                {
                    BiliInterfaceInfo info = BiliInterface.GetMP4info(s, 1); //TODO:多页视频

                    if (info != null)
                    {
                        listb.Add(info);
                    }
                }
            }

            Log.Info("所有视频MP4地址获取完成");
            Log.Info("开始批量下载");

            DlNext();
        }

        private void buttonDL_Click(object sender, EventArgs e)
        {
            string[] lines = Regex.Split(textBoxAV.Text, "\r\n|\r|\n");

            timer1.Enabled = true;
            tsd.Progressbar = verticalProgressBar1;

            Log.Info("获取所有视频信息");

            foreach (string s in lines)
            {
                if (s != "")
                {
                    BiliInterfaceInfo info = BiliInterface.GetFlvInfo(s);

                    if (info.flvurl != null)
                    {

                        listb.Add(info);

                        //Log.Info("正在下载视频 - " + info.AVNUM);

                        //tsd = new TSDownload("http://www.bilibilijj.com/DownLoad/Cid/" + info.cid, System.Environment.CurrentDirectory +  @"\video\" + info.AVNUM + ".flv");
                        //tsd.Progressbar = progressBar1;
                        //tsd.Start();

                    }
                }
            }

            Log.Info("所有视频信息获取完成");
            Log.Info("开始批量下载");

            DlNext();
            //Log.Info("批量下载完成");
        }

        void DlNext()
        {
            try
            {
                tsd.Stop();
            }
            catch { }

            if (listb.Count != 0)
            {
                if (listb[0].mp4url != null)
                    tsd = new TSDownload(listb[0].mp4url, System.Environment.CurrentDirectory + @"\video\" + listb[0].AVNUM + ".mp4");
                else if(listb[0].flvurl != null)
                    tsd = new TSDownload(listb[0].flvurl, System.Environment.CurrentDirectory + @"\video\" + listb[0].AVNUM + ".flv");
                tsd.Progressbar = verticalProgressBar1;
                nowAV = listb[0];
                Log.Info("正在下载视频 - " + listb[0].AVNUM + " | " + tsd.URL);
                pictureBoxDl.ImageLocation = listb[0].pic;
                tsd.Start();
                listb.RemoveAt(0);
            }
            else
            {
                Log.Info("批量下载完成");
                timer1.Enabled = false;
                textBox1.Text = "下载状态将会显示在这里";
            }

        }

        BiliInterfaceInfo nowAV;

        private void timer1_Tick(object sender, EventArgs e)
        {
            string s = "[下载信息]\r\n";
            s += "标题：" + nowAV.title + "\r\n";
            s += "AV号：" + nowAV.AVNUM + "\r\n";
            s += "CID ：" + nowAV.cid + "\r\n";
            s += "\r\n";
            s += "[下载状态]\r\n";
            s += " 大小 ：" + ((float)tsd.TotalBytes) / 1024 / 1024 + "MiB\r\n";
            s += " 速度 ：" + tsd.Speed / 1024 + "KiB/s\r\n";
            s += "百分比：" + tsd.Percent + "%\r\n";
            textBox1.Text = s;
            if (tsd.Percent == 100.0)
            {
                DlNext();
            }
            //Application.DoEvents();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<BiliInterfaceInfo> linfo = new List<BiliInterfaceInfo>();
            int start = int.Parse(textBoxFubangStart.Text);

            foreach (BiliInterfaceInfo i in (List<BiliInterfaceInfo>)dataGridViewRAW.DataSource)
            {
                if (i.Fpaiming >= start)
                    linfo.Add(i);
            }

            //TODO: 再次排序
            Fubang fu = new Fubang();
            fu.Gen(linfo);
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (webBrowser1.Url.ToString() == "http://www.bilibili.com/")
            {
                Log.Info("Cookie已成功获取！");
                textBoxCookie.Text = webBrowser1.Document.Cookie;
                webBrowser1.Hide();
                //webBrowser1.Dispose(); //这会造成线程阻塞
            }
        }

        private void buttonListDate1_Click(object sender, EventArgs e)
        {
            dateTimePickerFrom.Value = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(-1);
            dateTimePickerTo.Value = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-15")).AddMonths(-1);
        }

        private void buttonListDate2_Click(object sender, EventArgs e)
        {
            dateTimePickerFrom.Value = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-16")).AddMonths(-1);
            dateTimePickerTo.Value = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddDays(-1);
        }

        private void buttonRawSave_Click(object sender, EventArgs e)
        {
            string fileName = DateTime.Now.ToString("yyMMdd-HHmmss") + ".sdyg";
            saveFileDialogGuichu.FileName = fileName;
            if (saveFileDialogGuichu.ShowDialog() == DialogResult.OK)
            {
                fileName = saveFileDialogGuichu.FileName;
                try
                {
                    FileStream fs = new FileStream(fileName, FileMode.Create);
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, dataGridViewRAW.DataSource);

                    Log.Info("成功导出文件 -> " + fileName);
                }
                catch
                {
                    Log.Error("文件保存错误！检查权限");
                }
            }
        }

        private void buttonRawRead_Click(object sender, EventArgs e)
        {
            if (openFileDialogGuichu.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(openFileDialogGuichu.FileName, FileMode.Open);
                BinaryFormatter bf = new BinaryFormatter();
                List<BiliInterfaceInfo> bi = bf.Deserialize(fs) as List<BiliInterfaceInfo>;
                textBoxAV.Text = "";
                foreach (BiliInterfaceInfo i in bi)
                {
                    textBoxAV.Text += i.AVNUM + "\r\n";
                }
                dataGridViewRAW.DataSource = bi;
            }
        }

        private void dataGridViewRAW_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && dataGridViewRAW.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                System.Diagnostics.Process.Start("http://www.bilibili.com/video/" + dataGridViewRAW.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString().ToLower());
        }

        private void buttonZhubang_Click(object sender, EventArgs e)
        {
            List<BiliInterfaceInfo> linfo = new List<BiliInterfaceInfo>();
            int end = int.Parse(textBoxZhubangEnd.Text);

            foreach (BiliInterfaceInfo i in (List<BiliInterfaceInfo>)dataGridViewRAW.DataSource)
            {
                if (i.Fpaiming <= end)
                    linfo.Add(i);
            }

            Zhubang zhu = new Zhubang();
            zhu.Gen(linfo);
        }

        private void buttonAbout_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/SkiTiSu/BiliRanking");
        }


        int CurrentRowIndex;
        int CurrentColumnIndex;

        private void dataGridViewRAW_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex > -1 && e.ColumnIndex > -1)
            {
                var dgv = (DataGridView)sender;
                CurrentRowIndex = e.RowIndex;
                CurrentColumnIndex = e.ColumnIndex;
                for (int i = 0; i < dgv.RowCount; i++)
                {
                    dgv.Rows[i].Selected = false;
                }
                dgv.CurrentRow.Selected = false;
                dgv.Rows[CurrentRowIndex].Selected = true;
                contextMenuStripRAW.Show(MousePosition.X, MousePosition.Y);
            }
        }

        private void 移除taToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("删掉了就不能恢复了哦！", "Ahhhh你要干嘛",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2) != DialogResult.OK)
            {
                return;
            }
            List<BiliInterfaceInfo> bak = (List<BiliInterfaceInfo>)dataGridViewRAW.DataSource;
            bak.RemoveAt(CurrentRowIndex);
            for (int i = 1; i <= bak.Count; i++)
            {
                bak[i - 1].Fpaiming = i;
            }
            dataGridViewRAW.DataSource = null;
            dataGridViewRAW.DataSource = bak;
        }

        private void buttonListTagGen_Click(object sender, EventArgs e)
        {
            string[] tags = Regex.Split(textBoxTags.Text, ";|；");
            int i = 0;
            string html = BiliInterface.GetHtml("http://www.bilibili.com/index/tag/" + "30" + "/60d/hot/1/" + tags[i] + ".json");
            if (html == null)
            {
                Log.Error("数据错误");
                return;
            }

            System.Web.Script.Serialization.JavaScriptSerializer j = new System.Web.Script.Serialization.JavaScriptSerializer();
            BiliIndexInfo info = new BiliIndexInfo();
            info = j.Deserialize<BiliIndexInfo>(html);

            dataGridViewRAW.DataSource = info.list;
            tabControlMain.SelectedIndex = 2;
        }
    }
}
