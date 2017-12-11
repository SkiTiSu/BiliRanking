using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BiliRanking.Core
{
    public class TSDownload
    {
        /// <summary>
        /// 默认构造函数
        /// </summary>
        public TSDownload() { }

        /// <summary>
        /// 构造函数，下载到同目录下，保存的文件名通过url转换
        /// </summary>
        /// <param name="url">下载地址</param>
        public TSDownload(string url)
        {
            URL = url;
            FileName = GetFileName(url);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="filename">保存的文件名（可包含绝对路径）</param>
        public TSDownload(string url, string filename)
        {
            URL = url;
            FileName = filename;
        }

        /// <summary>
        /// 下载地址
        /// </summary>
        public string URL { get; set; }
        /// <summary>
        /// 保存的文件名（可包含绝对路径）
        /// </summary>
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                //fileName = removeInvChrInPath(value); // 如果传入目录会把目录一起误伤
                fileName = value;
            }
        }
        private string fileName;
        /// <summary>
        /// （可选）进度条
        /// </summary>
        public ProgressBar Progressbar = null;

        /// <summary>
        /// 下载专用线程
        /// </summary>
        private Thread tdl;
        /// <summary>
        /// 速度计时器
        /// </summary>
        private System.Timers.Timer tims = new System.Timers.Timer(1000);

        public long Speed = 0;

        /// <summary>
        /// 开始下载（如文件已存在则自动加(n)）
        /// </summary>
        public void Start()
        {
            tims.AutoReset = true;
            tims.Elapsed += tims_Elapsed;
            tims.Enabled = true;

            tdl = new Thread(ThreadDL);
            tdl.Name = "TSLib-DL";
            tdl.Start();
        }

        public void StartWithoutThread()
        {
            ThreadDL();
        }

        void tims_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Speed = DownloadedBytes - lastDownloadedBytes;
            lastDownloadedBytes = DownloadedBytes;
        }

        /// <summary>
        /// 停止下载
        /// </summary>
        public void Stop()
        {
            tims.Enabled = false;

            tdl.Abort();
        }

        //TODO:加入断点续传，下了一半的文件用其它后缀
        //TODO:终极-通过TSLib下载的文件统一管理

        /// <summary>
        /// 已下载百分比
        /// </summary>
        public float Percent = 0;
        /// <summary>
        /// 总大小（字节）
        /// </summary>
        public long TotalBytes = 0;
        /// <summary>
        /// 已下载大小（字节）
        /// </summary>
        public long DownloadedBytes = 0;
        /// <summary>
        /// 1秒前已下载的大小（字节）
        /// </summary>
        private long lastDownloadedBytes = 0;

        /// <summary>
        /// 下载专用线程的方法
        /// </summary>
        private void ThreadDL()
        {
            HttpWebRequest Myrq = (HttpWebRequest)HttpWebRequest.Create(URL);
            Myrq.Timeout = 5000;
            HttpWebResponse myrp = null;
            //bool f = false;
            int retry = 0;
            while (myrp == null)
            {
                try
                {
                    myrp = (HttpWebResponse)Myrq.GetResponse();
                }
                catch
                {
                    Log.Warn(FileName + "地址失败！等待延时" + URL);
                    retry++;
                    Thread.Sleep(3000); //TODO:同步的等待方法可能导致问题
                }
                if (retry == 5)
                {
                    Log.Error("我去，" + FileName + "下载失败，待会单独再下这个！");
                    Myrq.Abort();
                    Percent = 100;
                    return;
                }
            }
            TotalBytes = myrp.ContentLength;
            SetPbM((int)TotalBytes >> 4);
            Stream st = myrp.GetResponseStream();
            st.ReadTimeout = 3000;

            //检测是否有重名，有则加入(1)，如仍重复加(2)，以此类推
            if (File.Exists(FileName))
            {
                string fn1 = FileName.Substring(0, FileName.LastIndexOf('.'));
                string fn2 = FileName.Substring(FileName.LastIndexOf('.'));
                int i = 0;
                string nFileName;
                do
                {
                    i++;
                    nFileName = string.Format("{0}({1}){2}", fn1, i, fn2);
                } while (File.Exists(nFileName));
                FileName = nFileName;
            }

            Stream so = new FileStream(FileName, FileMode.CreateNew);
            byte[] by = new byte[10240];
            int osize = st.Read(by, 0, (int)by.Length);
            while (osize > 0)
            {
                DownloadedBytes = osize + DownloadedBytes;
                so.Write(by, 0, osize);
                SetPbV((int)DownloadedBytes >> 4);
                try
                {
                    osize = st.Read(by, 0, (int)by.Length);
                }
                catch
                {
                    Log.Error("我去，" + FileName + "下载失败，待会单独再下这个！");
                    so.Close();
                    st.Close();
                    Myrq.Abort();
                    Percent = 100;
                    return;
                }
                Percent = (float)DownloadedBytes / (float)TotalBytes * 100;
            }
            so.Close();
            st.Close();
            Myrq.Abort();
        }

        /// <summary>
        /// 修改进度条总大小
        /// </summary>
        /// <param name="n">总大小</param>
        private void SetPbM(int n)
        {
            if (Progressbar != null)
            {
                Action<int> Change = s => Progressbar.Maximum = s;
                if (Progressbar.InvokeRequired)
                {
                    Progressbar.Invoke(Change, n);
                }
                else
                {
                    Change(n);
                }
            }

        }
        /// <summary>
        /// 修改进度条已下载大小
        /// </summary>
        /// <param name="n">已下载大小</param>
        private void SetPbV(int n)
        {
            if (tims.Enabled == false && Progressbar != null)
            {
                //TODO: 为什么上述条件都不符合还是能进入导致progressbar已经被别的接管后导致的错误？
                try
                {
                    Action<int> Change = s => Progressbar.Value = s;
                    if (Progressbar.InvokeRequired)
                    {
                        Progressbar.Invoke(Change, n);
                    }
                    else
                    {
                        Change(n);
                    }
                }
                catch {}

            }
        }

        /// <summary>
        /// 根据下载地址获取文件名（截尾）
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <returns>文件名</returns>
        public static string GetCutFileName(string url)
        {
            int li = url.LastIndexOf('/');
            return url.Substring(li + 1, url.Length - li - 1);
        }

        /// <summary>
        /// 根据HTTP头文件并转码获取文件名
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <returns>文件名</returns>
        public static string GetFileName(string url)
        {
            string t = "ErrorFile";

            //TODO:支持更多种http头
            try
            {
                HttpWebRequest Myrq = (HttpWebRequest)HttpWebRequest.Create(url);
                HttpWebResponse myrp = (HttpWebResponse)Myrq.GetResponse();
                WebHeaderCollection whc = myrp.Headers;

                Myrq.Abort();

                Encoding e1 = Encoding.GetEncoding("iso-8859-1");
                Encoding e2 = Encoding.Default;
                string s = whc[1];
                byte[] ss = e1.GetBytes(s);
                byte[] sss = Encoding.Convert(Encoding.UTF8, e2, ss);

                t = e2.GetString(sss);
                t = t.Substring(t.IndexOf("\"") + 1, t.LastIndexOf("\"") - t.IndexOf("\"") - 1);
            }
            catch
            {
                int li = url.LastIndexOf('/');
                t = url.Substring(li + 1, url.Length - li - 1);
            }

            return t;
        }

        /// <summary>
        /// 删除路径和文件名中的非法字符
        /// http://www.crifan.com/files/doc/docbook/crifanlib_csharp/release/webhelp/removeinvchrinpath.html
        /// </summary>
        /// <param name="origFileOrPathStr"></param>
        /// <returns></returns>
        public static string removeInvChrInPath(string origFileOrPathStr)
        {
            string validFileOrPathStr = origFileOrPathStr;

            //filter out invalid title and artist char
            //char[] invalidChars = { '\\', '/', ':', '*', '?', '<', '>', '|', '\b' };
            char[] invalidChars = Path.GetInvalidPathChars();
            char[] invalidCharsInName = Path.GetInvalidFileNameChars();

            foreach (char chr in invalidChars)
            {
                validFileOrPathStr = validFileOrPathStr.Replace(chr.ToString(), "");
            }

            foreach (char chr in invalidCharsInName)
            {
                validFileOrPathStr = validFileOrPathStr.Replace(chr.ToString(), "");
            }

            return validFileOrPathStr;
        }
    }
}

