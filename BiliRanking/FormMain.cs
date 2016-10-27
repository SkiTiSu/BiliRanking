using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using BiliRanking.Properties;
using BiliRanking.Core;
using Newtonsoft.Json;
using System.IO.Compression;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using MaterialSkin.Controls;
using MaterialSkin;
using System.Configuration;

namespace BiliRanking
{
    public partial class FormMain : MaterialForm
    {
        ConfigHelper config = new ConfigHelper();

        public static string cookie;

        public static string[] wenhouyu = new string[]
        {
            "我又打算做好看版的了，兹磁不兹磁？"
        };
        /*
            "拒绝DSSQ，人人有责(◐﹏◐)",
            "天书不仅听音很准，而且歌唱的也不错呢（大雾",
            "啊♂ 乖 乖 站 好 ┗(O﹏O)┛",
            "你造吗，哲学的英文其实是Billy Herrington",
            "你造黄金の兄贵率是多少吗？1海灵顿=44cm",
            "金坷垃 金克拉 JinKeLa！",
            "天书说咱要建个会自动定时统计数据的网站（迷のflag",
            "所以说Web版（看台）不就一步一步做出来了吗（笑",
            "大力出？？？所以说大力到底是神马（纯洁脸",
            "最爱葛平老师了",
         */

        public FormMain()
        {
            InitializeComponent();

            this.Icon = Resources.logo;
            Random ran = new Random();
            int RandKey = ran.Next(0, wenhouyu.Length - 1);
            this.Text = $"BiliRanking V{Updater.Version} 来自中二的四季天书 - {wenhouyu[RandKey]}";
            cookie = textBoxCookie.Text;
            dataGridViewRAW.AutoGenerateColumns = false;
            comboBoxListNum.SelectedIndex = 0;
            comboBoxTagZone.SelectedIndex = 0;

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Purple800, Primary.Purple900, Primary.Purple500, Accent.Orange200, TextShade.WHITE);
            //this.Font = new System.Drawing.Font("Microsoft Yahei UI",20);

            BiliApiHelper.access_key = config.Get("access_key");
            Log.Debug($"通过配置文件读取的授权码为{BiliApiHelper.access_key}");
            if (!string.IsNullOrEmpty(BiliApiHelper.access_key))
            {
                Log.Info("已通过配置文件读取到授权码");
            }
            else
            {
                Log.Warn("没有获取到授权码！无法正常使用！");
            }
        }

        private void textBoxCookie_TextChanged(object sender, EventArgs e)
        {
            cookie = textBoxCookie.Text;
            //webBrowser1.Document.Cookie = textBoxCookie.Text;
            BiliInterface.cookie = cookie;
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

        private void FormMain_Shown(object sender, EventArgs e)
        {
            Updater up = new Updater();
            up.CheckUpdate();
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

            for (int i = 0; i < Convert.ToInt32(comboBoxListNum.Text); i++)
            {
                try
                {
                    textBoxAV.Text += ss[i] + "\r\n";
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

            TaskbarProgress.SetState(this.Handle, TaskbarProgress.TaskbarStates.Indeterminate);

            //if (cookie == null || cookie == "")
            //{
            //    Log.Warn("Cookie为空，会导致会员独享视频无法获取！");
            //}

            string[] lines = Regex.Split(textBoxAV.Text, "\r\n|\r|\n");
            List<BiliInterfaceInfo> ll = new List<BiliInterfaceInfo>();
            string failedAVs = "";
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
                    else
                    {
                        failedAVs += s + ";";
                    }
                }
            }

            ll.Sort(sortt);
            for (int i = 1; i <= ll.Count; i++)
            {
                ll[i - 1].Fpaiming = i;
            }
            dataGridViewRAW.DataSource = ll;
            
            if (failedAVs != "")
            {
                Log.Error("注意！下列视频数据未正确获取！\r\n" + failedAVs);
            }

            TaskbarProgress.SetState(this.Handle, TaskbarProgress.TaskbarStates.NoProgress);

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
                List<BiliInterfaceInfo> lb = (List<BiliInterfaceInfo>)dataGridViewRAW.DataSource;
                if (lb != null)
                    listb[0].Fpaiming = lb.Find(x => x.AVNUM == listb[0].AVNUM).Fpaiming;
                string topstring = "";
                if (listb[0].Fpaiming != 0 && listb[0].Fpaiming <= 20)
                    topstring = "TOP_" + listb[0].Fpaiming + "-";
                if (listb[0].mp4url != null)
                    tsd = new TSDownload(listb[0].mp4url, Environment.CurrentDirectory + $@"\video\{topstring}{listb[0].AVNUM}-{TSDownload.removeInvChrInPath(listb[0].title)}.mp4");
                else if(listb[0].flvurl != null)
                    tsd = new TSDownload(listb[0].flvurl, Environment.CurrentDirectory + $@"\video\{topstring}{listb[0].AVNUM}-{TSDownload.removeInvChrInPath(listb[0].title)}.flv");
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

        private void buttonFubang1_Click(object sender, EventArgs e)
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
        private void buttonFubang2_Click(object sender, EventArgs e)
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
            fu.Gen2(linfo);
        }

        private string GetEleConFromHtml (string html,string keywords,string endchar = "\"")
        {
            int i = html.IndexOf(keywords) + keywords.Length;
            int j = i;
            while (html.Substring(j, 1) != endchar)
            {
                j++;
            }
            return html.Substring(i, j - i);
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

        private void buttonListData2wAgo_Click(object sender, EventArgs e)
        {
            dateTimePickerFrom.Value = getWeekUpOfDate(DateTime.Now, DayOfWeek.Monday, -2);
            dateTimePickerTo.Value = dateTimePickerFrom.Value.AddDays(6);
        }

        private void buttonListData1wAgo_Click(object sender, EventArgs e)
        {
            dateTimePickerFrom.Value = getWeekUpOfDate(DateTime.Now, DayOfWeek.Monday, -1);
            dateTimePickerTo.Value = dateTimePickerFrom.Value.AddDays(6);
        }

        public DateTime getWeekUpOfDate(DateTime dt, DayOfWeek weekday, int Number)
        {
            int wd1 = (int)weekday;
            int wd2 = (int)dt.DayOfWeek;
            return wd2 == wd1 ? dt.AddDays(7 * Number) : dt.AddDays(7 * Number - wd2 + wd1);
        }

        private void buttonRawSave_Click(object sender, EventArgs e)
        {
            string fileName = DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".sdygx";
            saveFileDialogGuichu.FileName = fileName;
            if (saveFileDialogGuichu.ShowDialog() == DialogResult.OK)
            {
                fileName = saveFileDialogGuichu.FileName;
                try
                {
                    BiliShell bs = new BiliShell
                    {
                        ver = 1,
                        infos = (List<BiliInterfaceInfo>)dataGridViewRAW.DataSource
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
                MemoryStream tempMs = new MemoryStream();
                using (FileStream fs = new FileStream(openFileDialogGuichu.FileName, FileMode.Open))
                {
                    using (GZipStream Compress = new GZipStream(fs, CompressionMode.Decompress))
                    {
                        Compress.CopyTo(tempMs);
                    }
                }

                byte[] bytes = tempMs.ToArray();
                string str = Encoding.GetEncoding("UTF-8").GetString(bytes);
                BiliShell bs = JsonConvert.DeserializeObject<BiliShell>(str);
                List<BiliInterfaceInfo> bi = bs.infos;
                textBoxAV.Text = "";
                //TODO: 判断是否为空文件
                foreach (BiliInterfaceInfo i in bi)
                {
                    textBoxAV.Text += i.AVNUM + "\r\n";
                }
                dataGridViewRAW.DataSource = bi;

                Log.Info("文件加载完成，正在生成csv");
                textBoxOut.Text = "AV号,标题,播放数,弹幕数,收藏数,硬币数,评论数,up,时间,分区,播放得分,收藏得分,硬币得分,评论得分,总分\r\n";
                foreach (BiliInterfaceInfo info in bi)
                {
                            textBoxOut.Text += GenHang(new string[] { info.AVNUM.ToLower(), info.title, info.play.ToString(), info.video_review.ToString(), info.favorites.ToString(), info.coins.ToString(),
                            info.review.ToString(), info.author, info.created_at, info.typename,
                            info.Fplay.ToString(), info.Ffavorites.ToString(), info.Fcoins.ToString(), info.Freview.ToString(), info.Fdefen.ToString() });
                            textBoxOut.Text += "\"\r\n";
                            Application.DoEvents();
                }
                Log.Info("生成csv完成");
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
            Log.Info("开始根据TAG获取数据");
            string[] tags = Regex.Split(textBoxTags.Text, ";|；");
            int i = 0;
            string html = BiliInterface.GetHtml("http://www.bilibili.com/index/tag/" + "30" + "/60d/hot/1/" + tags[i] + ".json");
            if (html == null)
            {
                Log.Error("数据错误");
                return;
            }

            List<BiliInterfaceInfo> infos = new List<BiliInterfaceInfo>();

            System.Web.Script.Serialization.JavaScriptSerializer j = new System.Web.Script.Serialization.JavaScriptSerializer();
            BiliIndexInfo info = new BiliIndexInfo();
            info = j.Deserialize<BiliIndexInfo>(html);

            Log.Info($"一共找到了{info.pages}页的数据");

            foreach (var l in info.list)
            {
                infos.Add(l);
            }


            for (int k = 2; k <= info.pages; k++)
            {
                html = BiliInterface.GetHtml("http://www.bilibili.com/index/tag/" + "30" + "/60d/hot/" + k.ToString() + "/" + tags[i] + ".json");
                info = new BiliIndexInfo();
                try
                {
                    info = j.Deserialize<BiliIndexInfo>(html);
                    foreach (var l in info.list)
                    {
                        infos.Add(l);
                    }
                }
                catch (Exception)
                {
                    html = html.Replace("\"--\"", "\"0\"");
                    info = j.Deserialize<BiliIndexInfo>(html);
                    foreach (var l in info.list)
                    {
                        infos.Add(l);
                    }

                    Log.Error($"在第{k.ToString()}页遇到不可读的“--”数据，B站真是不可描述= =，天书已经把不可读数据变成了0了，最好将所有数据通过API获取一遍");
                }
            }

            /* 算分
            foreach (var iinfo in infos)
            {
                if (iinfo.play <= 500)
                {
                    iinfo.Fdefen = iinfo.play + 5 * iinfo.review + 20 * iinfo.coins + 15 * iinfo.favorites + iinfo.video_review;
                }
                else if (iinfo.play <= 800)
                {
                    iinfo.Fdefen = iinfo.play + 5 * iinfo.review + 18 * iinfo.coins + 13 * iinfo.favorites;
                }
                else
                {
                    iinfo.Fdefen = 800 + 5 * iinfo.review + 18 * iinfo.coins + 13 * iinfo.favorites;
                }

            }
            
            infos.Sort(sortt);
            for (int ii = 1; ii <= infos.Count; ii++)
            {
                infos[ii - 1].Fpaiming = ii;
            }
            */

            textBoxOut.Text = "AV号,标题,播放数,弹幕数,收藏数,硬币数,评论数,up,时间,分区,总分\r\n";
            textBoxAV.Text = "";
            foreach (var iinfo in infos)
            {
                textBoxAV.Text += iinfo.avnum + "\r\n";
                textBoxOut.Text += GenHang(new string[] { iinfo.avnum, iinfo.title, iinfo.play.ToString(), iinfo.video_review.ToString(), iinfo.favorites.ToString(), iinfo.coins.ToString(),
                            iinfo.review.ToString(), iinfo.author, iinfo.created_at, iinfo.typename,
                            iinfo.Fdefen.ToString() });
                textBoxOut.Text += "\"\r\n";
                Application.DoEvents();
            }

            dataGridViewRAW.DataSource = infos;
            tabControlMain.SelectedIndex = 2;

            Log.Info("根据TAG获取数据完成，目前的数据是TAG接口返回的，最好重新通过API获取一遍数据");
        }

        private void 按视频模板复制数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BiliInterfaceInfo curr = ((List<BiliInterfaceInfo>)dataGridViewRAW.DataSource)[CurrentRowIndex];
            string copytext = curr.play + "\r\n" + curr.coins + "\r\n" + curr.favorites + "\r\n" + curr.video_review + "\r\n" + curr.review;
            Clipboard.SetText(copytext);
            Log.Info(String.Format("已复制{0}的数据到剪贴板", curr.AVNUM));
        }

        private void 复制总分ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BiliInterfaceInfo curr = ((List<BiliInterfaceInfo>)dataGridViewRAW.DataSource)[CurrentRowIndex];
            string copytext = curr.Fdefen.ToString();
            Clipboard.SetText(copytext);
            Log.Info(String.Format("已复制{0}的总分到剪贴板", curr.AVNUM));
        }

        private void 复制标题和信息行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BiliInterfaceInfo curr = ((List<BiliInterfaceInfo>)dataGridViewRAW.DataSource)[CurrentRowIndex];
            string copytext = curr.title + "\r\n" + curr.created_at + "   " + curr.author + "   " + curr.avnum;
            Clipboard.SetText(copytext);
            Log.Info(String.Format("已复制{0}的标题和信息行到剪贴板", curr.AVNUM));
        }

        private void 复制数据含中文ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BiliInterfaceInfo curr = ((List<BiliInterfaceInfo>)dataGridViewRAW.DataSource)[CurrentRowIndex];
            string copytext = String.Format("播放{0,11}\r\n硬币{1,11}\r\n收藏{2,11}\r\n弹幕{3,11}\r\n评论{4,11}", curr.play, curr.coins, curr.favorites, curr.video_review, curr.review);
            Clipboard.SetText(copytext);
            Log.Info(String.Format("已复制{0}的数据（含中文）到剪贴板", curr.AVNUM));
        }

        private void buttonRawProgram_Click(object sender, EventArgs e)
        {
            List<BiliInterfaceInfo> now = (List<BiliInterfaceInfo>)dataGridViewRAW.DataSource;
            if (now == null || now.Count < int.Parse(textBoxRawProgramTo.Text))
            {
                MessageBox.Show("没有足够的数据orz\r\n是不是被你吃掉了？", "噫……", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (int.Parse(textBoxRawProgramFrom.Text)> int.Parse(textBoxRawProgramTo.Text))
            {
                MessageBox.Show("脑子抽调啦！@_@\r\n怎么能倒过来呢", "噫……", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string copytext = "";
            for (int i = int.Parse(textBoxRawProgramFrom.Text) - 1; i <= int.Parse(textBoxRawProgramTo.Text) - 1; i++)
            {
                copytext += String.Format("{0:D2}|{3} {1} UP主：{2}\r\n", now[i].Fpaiming, now[i].title, now[i].author, now[i].avnum);
            }
            Clipboard.SetText(copytext);
            Log.Info("节目单已复制");
        }

        private void OnlyDigi_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void buttonUpdateBeta_Click(object sender, EventArgs e)
        {
            Updater up = new Updater();
            up.CheckUpdate(true);
        }

        private void buttonRAWReadExcel_Click(object sender, EventArgs e)
        {
            /*
            if (openFileDialogExcel.ShowDialog() == DialogResult.OK)
            {
                FileStream stream;
                try
                {
                    stream = File.Open(openFileDialogExcel.FileName, FileMode.Open, FileAccess.Read);
                }
                catch (Exception ee)
                {
                    Log.Error("读取时发生错误，文件没有关闭？" + ee.Message);
                    return;
                }

                IExcelDataReader excelReader;

                if (Path.GetExtension(openFileDialogExcel.FileName) == ".xls")
                {
                    Log.Debug("xls");
                    excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else
                {
                    Log.Debug("xlsx");
                    excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }

                excelReader.IsFirstRowAsColumnNames = true;
                DataSet result = excelReader.AsDataSet();
            }
            */
            //string data = Clipboard.GetText();

            MessageBox.Show("请一定将表格中的列按照内置表格中除去最后两列的方式排序（不能含有小写逗号\",\"，不可以含有小数），\r\n然后将数据区域（不含标题）复制到剪贴板，点击确定\r\n//抱歉现在的方法可能太不人性化了，以后天书会改进的", "锵锵锵");

            try
            {
                Log.Info("开始读取剪贴板数据");
                var fmt_csv = DataFormats.CommaSeparatedValue;
                var dataobject = Clipboard.GetDataObject();
                var stream = (Stream)dataobject.GetData(fmt_csv);
                //var enc = System.Text.Encoding.GetEncoding(1252);
                var enc = Encoding.Default;
                var reader = new StreamReader(stream, enc);
                string data_csv = reader.ReadToEnd();

                string[] lines = Regex.Split(data_csv, "\r\n");
                List<BiliInterfaceInfo> blist = new List<BiliInterfaceInfo>();
                foreach (string line in lines)
                {
                    string[] items = Regex.Split(line, ",");
                    if (items.Length < 16)
                    {
                        if (line != "\0")
                            Log.Warn("该行数据不合法：" + line);
                        continue;
                    }
                    BiliInterfaceInfo info = new BiliInterfaceInfo();
                    info.Fpaiming = int.Parse(items[0]);
                    info.AVNUM = items[1];
                    info.title = items[2];
                    info.play = uint.Parse(items[3]);
                    info.video_review = uint.Parse(items[4]);
                    info.favorites = uint.Parse(items[5]);
                    info.coins = uint.Parse(items[6]);
                    info.review = uint.Parse(items[7]);
                    info.author = items[8];
                    info.created_at = items[9];
                    info.typename = items[10];
                    info.Fplay = uint.Parse(items[11]);
                    info.Ffavorites = uint.Parse(items[12]);
                    info.Fcoins = uint.Parse(items[13]);
                    info.Freview = uint.Parse(items[14]);
                    info.Fdefen = uint.Parse(items[15]);
                    blist.Add(info);
                }

                dataGridViewRAW.DataSource = blist;
                textBoxAV.Text = "";
                foreach (BiliInterfaceInfo i in blist)
                {
                    textBoxAV.Text += i.AVNUM + "\r\n";
                }
                Log.Info("读取与转换完成");
            }
            catch (Exception ee)
            {
                Log.Error("发生错误：" + ee.Message);
            }
        }

        private void buttonQuickCopy_Click(object sender, EventArgs e)
        {
            FormQuickCopy fq = new FormQuickCopy((List<BiliInterfaceInfo>)dataGridViewRAW.DataSource);
            fq.Show();
        }

        public static byte[] Compress(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                GZipStream Compress = new GZipStream(ms, CompressionMode.Compress);

                Compress.Write(bytes, 0, bytes.Length);

                Compress.Close();

                return ms.ToArray();

            }
        }

        private void buttonDlMP4JJ_Click(object sender, EventArgs e)
        {
            string[] lines = Regex.Split(textBoxAV.Text, "\r\n|\r|\n");

            timer1.Enabled = true;
            tsd.Progressbar = verticalProgressBar1;

            Log.Info("获取所有视频MP4地址");

            foreach (string s in lines)
            {
                if (s != "")
                {
                    BiliInterfaceInfo info = BiliInterface.GetMP4info(s, 1, true);

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

        private void 进入唧唧下载MP4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //http://www.bilibilijj.com/Files/DownLoad/9626305.mp4
            BiliInterfaceInfo curr = ((List<BiliInterfaceInfo>)dataGridViewRAW.DataSource)[CurrentRowIndex];
            Process.Start($"http://www.bilibilijj.com/Files/DownLoad/{curr.cid}.mp4");
            string html = BiliInterface.GetHtml($"http://www.bilibilijj.com/Files/DownLoad/{curr.cid}.mp4");
            //
        }

        private async void buttonLogin_Click(object sender, EventArgs e)
        {
            string res = await BiliApiHelper.LoginBilibili(textBoxLoginName.Text, textBoxLoginPasswd.Text);
            //MessageBox.Show(res);
            Log.Info("授权码:" + res);
        }
    }
}
