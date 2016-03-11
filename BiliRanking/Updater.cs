using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliRanking
{
    //本类参考了ss-csharp的UpdaterChecker类
    public class Updater
    {
        public const string Version = "1.1.1";

        private const string UpdateURL = "https://api.github.com/repos/SkiTiSu/BiliRanking/releases";
        private const string UserAgent = "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.3319.102 Safari/537.36";

        public string LatestVersionNumber;

        private bool checkBeta = false;

        public void CheckUpdate(bool beta = false)
        {
            checkBeta = beta;

            DirectoryInfo theFolder = new DirectoryInfo(Environment.CurrentDirectory);
            FileInfo[] fileInfo = theFolder.GetFiles();
            foreach (FileInfo file in fileInfo)
            {
                if (file.Name.Contains(".delete"))
                {
                    File.Delete(file.FullName);
                    Log.Debug($"已删除更新残留文件{file.FullName}");
                    MessageBox.Show("更新完成o(^▽^)o", "蛤蛤蛤蛤蛤", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            try
            {
                Log.Info("正在检查更新");
                WebClient http = CreateWebClient();
                http.DownloadStringCompleted += Http_DownloadStringCompleted;
                http.DownloadStringAsync(new Uri(UpdateURL));
            }
            catch
            {
                Log.Error("检查更新失败 - 版本检查失败");
            }
        }

        private void Http_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                string response = e.Result;

                JArray result = JArray.Parse(response);
                List<Asset> asserts = new List<Asset>();

                bool hasBeta = false;
                bool hasStable = false;

                if (result != null)
                {
                    foreach (JObject release in result)
                    {
                        if ((bool)release["prerelease"])
                        {
                            Asset ass = new Asset();
                            ass.Parase(release);
                            if (ass.IsNewVersion(Version))
                            {
                                asserts.Add(ass);
                                hasBeta = true;
                            }
                        }
                        else
                        {
                            Asset ass = new Asset();
                            ass.Parase(release);
                            if (ass.IsNewVersion(Version))
                            {
                                asserts.Add(ass);
                                hasStable = true;
                            }
                            break;
                        }
                    }
                }

                if (asserts.Count != 0)
                {
                    foreach (Asset ass in asserts)
                    {
                        Log.Info($"发现新版本{ass.name}" + (ass.prerelease ? "beta" : ""));
                    }
                    int stable = asserts.Count - 1;
                    if (!checkBeta && hasStable)
                    {
                        DialogResult res = MessageBox.Show($"发现新版本{asserts[stable].name}啦\r\n更新日志：\r\n{asserts[stable].body}\r\n\r\n马上更新嘛？", @"\(^o^)/更新啦", MessageBoxButtons.YesNo);
                        if (res == DialogResult.Yes)
                        {
                            StartDownload(asserts[stable]);
                        }
                        else
                        {
                            Log.Info("更新被取消");
                        }
                    }
                    else if(checkBeta && (hasBeta || hasStable))
                    {
                        DialogResult res = MessageBox.Show($"发现新版本{asserts[0].name}beta啦\r\n更新日志：\r\n{asserts[0].body}\r\n\r\n马上更新嘛？", @"\(^o^)/更新啦", MessageBoxButtons.YesNo);
                        if (res == DialogResult.Yes)
                        {
                            StartDownload(asserts[0]);
                        }
                        else
                        {
                            Log.Info("更新被取消");
                        }
                    }
                    else
                    {
                        Log.Info("没有符合要求的更新");
                    }
                }
                else
                {
                    Log.Info("没有发现新版本");
                }

            }
            catch (Exception ex)
            {
                Log.Error("检查更新失败 - 解析json失败");
                Log.Debug(ex.Message);
            }
}

        string DownloadFileName = "";

        private void StartDownload(Asset ass)
        {
            Log.Info($"开始下载 - {ass.browser_download_url}");
            Log.Info("提示：由于服务器在境外，下载可能比较慢，稍安勿躁哦！下载期间就不要再动程序了~");

            DownloadFileName = Environment.CurrentDirectory + $@"\BiliRanking.{ass.name}.exe";
            try
            {
                WebClient http = CreateWebClient();
                http.DownloadFileCompleted += Http_DownloadFileCompleted; ;
                http.DownloadFileAsync(new Uri(ass.browser_download_url), DownloadFileName);
            }
            catch (Exception ex)
            {
                Log.Error("文件下载失败");
                Log.Debug(ex.Message);
            }
        }

        private void Http_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            MessageBox.Show($"新版本已经准备完毕//天书好是辛苦的呢！\r\n我要重启自己来更新咯(●'◡'●)", "注意啊啊啊", MessageBoxButtons.OK, MessageBoxIcon.Information);
            string filename = Assembly.GetExecutingAssembly().Location;
            File.Move(filename, filename + ".delete");
            System.Diagnostics.Process.Start(DownloadFileName);
            //File.Move(filename + ".new", "BiliRanking.exe");
            Application.Exit();
        }

        private WebClient CreateWebClient()
        {
            WebClient http = new WebClient();
            http.Headers.Add("User-Agent", UserAgent);
            //http.Proxy = new WebProxy(IPAddress.Loopback.ToString(), config.localPort);
            http.Encoding = System.Text.Encoding.UTF8; //否则遇到中文Json解析器会报错
            return http;
        }

        public class Asset
        {
            public string name;
            public bool prerelease;
            public string browser_download_url;
            public string body;

            public void Parase(JObject asset)
            {
                name = (string)asset["name"];
                prerelease = (bool)asset["prerelease"];
                try
                {
                    browser_download_url = (string)asset["assets"][0]["browser_download_url"];
                }
                catch (Exception e)
                {
                    Log.Error("检查更新失败 - 尼玛天书怎么搞了个没有下载地址的更新信息呢？");
                    throw e;
                }
                body = (string)asset["body"];
            }

            public bool IsNewVersion(string currentVersion)
            {
                return CompareVersion(name, currentVersion) > 0;
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
}
