using BiliRanking.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BiliRanking.WPF
{

    public class Updater
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        FileVersionInfo myFileVersion = FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
        public string Version = "";

        private const string UpdateURL = "http://dl.skitisu.com/biliranking/update.json";
        private const string UserAgent = "";

        public string LatestVersionNumber;

        private bool checkBeta = false;

        public Updater()
        {
            Version = myFileVersion.FileVersion;
        }

        public void CheckUpdate(bool beta = false)
        {
            checkBeta = beta;

            DirectoryInfo theFolder = new DirectoryInfo(FileManager.currentPath);
            FileInfo[] fileInfo = theFolder.GetFiles();
            foreach (FileInfo file in fileInfo)
            {
                if (file.Name.Contains(".delete"))
                {
                    File.Delete(file.FullName);
                    log.Debug($"已删除更新残留文件{file.FullName}");
                    MessageBox.Show("更新完成o(^▽^)o", "蛤蛤蛤蛤蛤", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            try
            {
                log.Info("正在检查更新");
                WebClient http = CreateWebClient();
                http.DownloadStringCompleted += Http_DownloadStringCompleted;
                http.DownloadStringAsync(new Uri(UpdateURL + "?ts=" + ((DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds).ToString()));
            }
            catch
            {
                log.Error("检查更新失败 - 版本检查失败");
            }
        }

        private void Http_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                string response = e.Result;

                JObject result = JObject.Parse(response);

                //bool hasBeta = false;
                //bool hasStable = false;

                if (result != null)
                {
                    JToken data = result["data"];
                    var ver = (string)data["version"];
                    int isnew = CompareVersion(ver, Version);

                    if (isnew > 0)
                    {
                        log.Info($"发现新版本{ver}");
                        DialogResult res = MessageBox.Show($"发现新版本{ver}啦\r\n{data["updateInfo"]}\r\n\r\n马上更新嘛？", @"\(^o^)/更新啦", MessageBoxButtons.YesNo);
                        if (res == DialogResult.Yes)
                        {
                            StartDownload((string)data["url"]);
                        }
                        else
                        {
                            log.Info("更新被取消");
                        }
                    }
                    else
                    {
                        log.Info("没有发现新版本");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("检查更新失败 - 解析json失败");
                Log.Debug(ex.Message);
            }
        }

        string DownloadFileName = "";

        private void StartDownload(string downloadurl)
        {
            Log.Info($"开始下载 - {downloadurl}");
            Log.Info("提示：由于服务器在境外，下载可能比较慢，稍安勿躁哦！下载期间就不要再动程序了~");

            DownloadFileName = Path.Combine(FileManager.currentPath, "BiliRanking.downloading");
            try
            {
                WebClient http = CreateWebClient();
                http.DownloadFileCompleted += Http_DownloadFileCompleted; ;
                http.DownloadFileAsync(new Uri(downloadurl), DownloadFileName);
            }
            catch (Exception ex)
            {
                Log.Error("文件下载失败");
                Log.Debug(ex.Message);
            }
        }

        private void Http_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            string filename = Assembly.GetExecutingAssembly().Location;
            File.Move(filename, filename + ".delete");
            File.Move(DownloadFileName, FileManager.currentPath + $@"\BiliRanking.exe");
            MessageBox.Show($"新版本已经准备完毕//天书好是辛苦的呢！\r\n我要重启自己来更新咯(●'◡'●)", "注意啊啊啊", MessageBoxButtons.OK, MessageBoxIcon.Information);
            System.Diagnostics.Process.Start(FileManager.currentPath + $@"\BiliRanking.exe");
            //File.Move(filename + ".new", "BiliRanking.exe");
            System.Windows.Application.Current.Shutdown();
        }

        private WebClient CreateWebClient()
        {
            WebClient http = new WebClient();
            http.Headers.Add("User-Agent", UserAgent);
            //http.Proxy = new WebProxy(IPAddress.Loopback.ToString(), config.localPort);
            http.Encoding = System.Text.Encoding.UTF8; //否则遇到中文Json解析器会报错
            return http;
        }

        public static int CompareVersion(string l, string r)
        {
            if (l.StartsWith("V") || l.StartsWith("v"))
                l = l.Substring(1);
            var ls = l.Split('.');
            var rs = r.Split('.');
            for (int i = 0; i < Math.Max(ls.Length, rs.Length); i++)
            {
                int lp = (i < ls.Length) ? int.Parse(ls[i]) : 0;
                int rp = (i < rs.Length) ? int.Parse(rs[i]) : 0;
                if (lp != rp)
                {
                    return lp - rp;
                }
            }
            return 0;
        }
    }
}
