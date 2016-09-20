using System;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace BiliRanking.Core
{
    public static class BiliInterface
    {
        public static string cookie = "";
        const string appkey = "c1b107428d337928";
        //8e9fc618fbd41e28 不需要appsec
        const string dlappkey = "f3bb208b3d081dc8";
        const string dlappsec = "1c15888dc316e05a15fdd0a02ed6584f";
        //86385cdc024c0f6c
        /* 已经尝试的无效的appkey与appsec：
         * f3bb208b3d081dc8
         * c1b107428d337928 ea85624dfcf12d7cc7b2b3a94fac1f2c
         * 
         * 
         */
        const string appsec = "ea85624dfcf12d7cc7b2b3a94fac1f2c";

        public static string GetSign(SortedDictionary<string, string> sparam)
        {
            //
            sparam.Add("_appver", "3040000");
            sparam.Add("_tid", "0");
            sparam.Add("_p", "1");
            sparam.Add("_down", "0");
            //
            sparam.Add("platform", "android");
            sparam.Add("_device", "android");
            sparam.Add("_hwid", "ccbb856c97ccb8d2");
            sparam.Add("ts", ((long)((DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds)).ToString());
            if (!sparam.ContainsKey("appkey")) sparam.Add("appkey", appkey);
            if (!sparam.ContainsKey("type")) sparam.Add("type", "json");
            if (!sparam.ContainsKey("appsec")) sparam.Add("appsec", appsec);
            string final_param = "";
            foreach (var aparam in sparam)
            {
                if (aparam.Value == null || aparam.Key == "appsec") continue;
                if (final_param != "") final_param += "&";
                final_param += aparam.Key + "=" + aparam.Value;
            }
            using (var md5 = MD5.Create())
            {
                string hashed = BitConverter.ToString(md5.ComputeHash(Encoding.ASCII.GetBytes(final_param + sparam["appsec"]))).Replace("-", "").ToLower();
                final_param += "&sign=" + hashed;
            }
            return final_param;
        }

        public static string GetAVdenum(string AVnum)
        {
            string avnum = AVnum.ToUpper();
            if (avnum.Contains("AV"))
            {
                avnum = avnum.Substring(2, avnum.Length - 2);
            }
            return avnum;
        }

        public static void GetPic(string AVnum)
        {
            string avnum = GetAVdenum(AVnum);
            string file = Environment.CurrentDirectory + @"\pic\AV" + avnum + ".jpg";
            if (File.Exists(file))
            {
                Log.Info("封面已存在 - AV" + avnum);
            }
            else
            {
                BiliInterfaceInfo info = GetInfo(AVnum);
                if (info.pic != null)
                {
                    string url = info.pic;
                    Log.Info("正在获取封面 - " + info.AVNUM + " | " + url + " -> " + file);
                    TSDownload tsd = new TSDownload(url, file);
                    tsd.Start();
                }
            }
        }

        public static void GetPic(BiliInterfaceInfo info)
        {
            string avnum = info.AVNUM;
            string file = Environment.CurrentDirectory + @"\pic\" + avnum + ".jpg";
            if (File.Exists(file))
            {
                Log.Info("封面已存在 - " + avnum);
            }
            else
            {
                if (info.pic != null)
                {
                    string url = info.pic;
                    Log.Info("正在获取封面 - " + info.AVNUM + " | " + url + " -> " + file);
                    TSDownload tsd = new TSDownload(url, file);
                    tsd.StartWithoutThread();
                }
            }
        }

        public static string GetMP4Url(uint cid)
        {
            Log.Info("开始获取MP4地址 - CID" + cid);

            SortedDictionary<string, string> parampairs = new SortedDictionary<string, string>();
            parampairs.Add("cid", cid.ToString());
            parampairs.Add("quality", "3");
            parampairs.Add("type", "mp4");
            parampairs.Add("appkey", dlappkey);
            string param = GetSign(parampairs);

            string html = GetHtml("http://interface.bilibili.com/playurl?" + param);

            if (!html.Contains("<result>su"))
            {
                Log.Error("MP4地址获取失败！ - CID：" + cid);
                return null;
            }

            byte[] byteArray = Encoding.UTF8.GetBytes(html);
            MemoryStream stream = new MemoryStream(byteArray);
            XElement xe = XElement.Load(stream);
            var t = xe.Elements("url");
            IEnumerable<string> elements = from ele in xe.Descendants("url") //where ele.Name == "url"
                                           select ele.Value;
            return elements.ToArray()[0];
        }

        public static string GetMP4UrlBackUp(uint aid, int page = 1)
        {
            Log.Info("开始使用备选方案获取MP4地址 - aid" + aid);
            string h5url = "http://www.bilibili.com/m/html5?aid=" + aid + "&page=" + page;
            string html = GetHtml(h5url);
            JavaScriptSerializer j = new JavaScriptSerializer();
            BiliH5videoInfo info = new BiliH5videoInfo();
            info = j.Deserialize<BiliH5videoInfo>(html);
            if (info.src == "http://static.hdslb.com/error.mp4")
            {
                Log.Error("错误的AV号或页码！（有些老视频没有mp4格式的哦= =）");
                return null;
            }
            else
            {
                return info.src;
            }
        }

        public static BiliInterfaceInfo GetMP4info(string AVnum, int page, bool isJJ = false)
        {
            string avnum = AVnum.ToUpper();
            if (avnum.Contains("AV"))
            {
                avnum = avnum.Substring(2, avnum.Length - 2);
            }

            Log.Info("正在通过API获取数据 - AV" + avnum);

            SortedDictionary<string, string> parampairs = new SortedDictionary<string, string>();
            parampairs.Add("id", avnum);
            string param = GetSign(parampairs);

            string html = GetHtml("http://api.bilibili.com/view?" + param);

            JavaScriptSerializer j = new JavaScriptSerializer();
            BiliInterfaceInfo info = new BiliInterfaceInfo();
            try
            {
                info = j.Deserialize<BiliInterfaceInfo>(html);

                if (info.code == -403)
                {
                    if (info.error == "no perm error")
                        Log.Error("没有数据！（正在补档或被删除？）");
                    else
                        Log.Error("本视频为会员独享，需要Cookie！");
                }
                else if (info.code == -503)
                {
                    Log.Warn("到达连续获取上限，延时两秒");
                    System.Threading.Thread.Sleep(2000);
                    return GetInfo(AVnum);
                }
                else if (info.code == -404)
                {
                    Log.Error("视频不存在！");
                }
                else if (info.code != 0)
                {
                    Log.Error("返回未知错误：" + info.code);
                }
                else
                {
                    info.AVNUM = "AV" + avnum;
                    info.title = info.title.Replace("&amp;", "&");
                    info.title = info.title.Replace("&lt;", "<");
                    info.title = info.title.Replace("&gt;", ">");
                    info.title = info.title.Replace("&quot;", "\"");

                    //下载视频，无需算分

                    //info.mp4url = GetMP4Url(info.cid);
                    info.mp4url = GetMP4UrlBackUp(UInt32.Parse(avnum));
                }
            }
            catch (Exception e)
            {
                Log.Error("AV" + avnum + "的数据发生错误，请稍后重试！" + e.Message);
            }

            return info;
        }

        //TODO: 加入P2、Pn的信息获取
        public static BiliInterfaceInfo GetInfo(string AVnum)
        {
            string avnum = AVnum.ToUpper();
            if (avnum.Contains("AV"))
            {
                avnum = avnum.Substring(2, avnum.Length - 2);
            }

            Log.Info("正在通过API获取数据 - AV" + avnum);

            SortedDictionary<string, string> parampairs = new SortedDictionary<string, string>();
            parampairs.Add("id", avnum);
            string param = GetSign(parampairs);

            string html = GetHtml("http://api.bilibili.com/view?" + param);

            JavaScriptSerializer j = new JavaScriptSerializer();
            BiliInterfaceInfo info = new BiliInterfaceInfo();
            try
            {
                info = j.Deserialize<BiliInterfaceInfo>(html);

                if (info.code == -403)
                {
                    if (info.error == "no perm error")
                        Log.Error("没有数据！（正在补档或被删除？）");
                    else
                        Log.Error("本视频为会员独享，需要Cookie！");
                }
                else if (info.code == -503)
                {
                    Log.Warn("到达连续获取上限，延时两秒");
                    System.Threading.Thread.Sleep(2000);
                    return GetInfo(AVnum);
                }
                else if (info.code == -404)
                {
                    Log.Error("视频不存在！");
                }
                else if (info.code != 0)
                {
                    Log.Error("返回未知错误：" + info.code);
                }
                else
                {
                    info.AVNUM = "AV" + avnum;
                    info.title = info.title.Replace("&amp;", "&");
                    info.title = info.title.Replace("&lt;", "<");
                    info.title = info.title.Replace("&gt;", ">");
                    info.title = info.title.Replace("&quot;", "\"");

                    //算分
                    double xiuzheng = 0;

                    //收藏
                    xiuzheng = ((double)info.favorites / (double)info.play) * 1500;
                    if (xiuzheng > 55)
                        xiuzheng = 55;
                    info.Ffavorites = Convert.ToUInt32(info.favorites * xiuzheng);

                    //硬币
                    xiuzheng = ((double)info.coins / (double)info.play) * 5000;
                    if (xiuzheng > 25)
                        xiuzheng = 25;
                    info.Fcoins = Convert.ToUInt32(info.coins * xiuzheng);

                    //评论
                    xiuzheng = ((double)(info.review + info.favorites + info.coins) / (double)(info.play + info.review + info.video_review * 5)) * 800;
                    if (xiuzheng > 30)
                        xiuzheng = 30;
                    info.Freview = Convert.ToUInt32(info.review * xiuzheng);

                    //播放
                    info.Fplay = info.Ffavorites + info.Fcoins;
                    if (info.play <= info.Fplay)
                        info.Fplay = info.play;
                    else
                        info.Fplay = info.Fplay + (info.play - info.Fplay) / 2;

                    //得分
                    info.Fdefen = info.Ffavorites + info.Fcoins + info.Freview + info.Fplay;

                }
            }
            catch (Exception e)
            {
                Log.Error("AV" + avnum + "的数据发生错误，请稍后重试！" + e.Message);
            }

            return info;
        }

        public static BiliInterfaceInfo GetFlvInfo(string AVnum)
        {
            string avnum = AVnum.ToUpper();
            if (avnum.Contains("AV"))
            {
                avnum = avnum.Substring(2, avnum.Length - 2);
            }

            Log.Info("正在通过API获取数据 - AV" + avnum);

            SortedDictionary<string, string> parampairs = new SortedDictionary<string, string>();
            parampairs.Add("id", avnum);
            string param = GetSign(parampairs);

            string html = GetHtml("http://api.bilibili.com/view?" + param);

            JavaScriptSerializer j = new JavaScriptSerializer();
            BiliInterfaceInfo info = new BiliInterfaceInfo();
            try
            {
                info = j.Deserialize<BiliInterfaceInfo>(html);

                if (info.code == -403)
                {
                    if (info.error == "no perm error")
                        Log.Error("没有数据！（正在补档或被删除？）");
                    else
                        Log.Error("本视频为会员独享，需要Cookie！");
                }
                else if (info.code == -503)
                {
                    Log.Warn("到达连续获取上限，延时两秒");
                    System.Threading.Thread.Sleep(2000);
                    return GetInfo(AVnum);
                }
                else if (info.code == -404)
                {
                    Log.Error("视频不存在！");
                }
                else if (info.code != 0)
                {
                    Log.Error("返回未知错误：" + html);
                }
                else
                {
                    info.AVNUM = "AV" + avnum;
                    info.title = info.title.Replace("&amp;", "&");
                    info.title = info.title.Replace("&lt;", "<");
                    info.title = info.title.Replace("&gt;", ">");
                    info.title = info.title.Replace("&quot;", "\"");

                    //下载视频，无需算分

                    //info.flvurl = GetFlvUrl(info.cid);
                    info.flvurl = GetFlvUrl(UInt32.Parse(info.avnum.Substring(2)),info.cid);

                }
            }
            catch (Exception e)
            {
                Log.Error("AV" + avnum + "的数据发生错误，请稍后重试！" + e.Message);
            }

            return info;
        }

        public static string GetFlvUrl(uint aid, uint cid)
        {
            SortedDictionary<string, string> parampairs = new SortedDictionary<string, string>();
            //parampairs.Add("aid", aid.ToString());
            //parampairs.Add("cid", cid.ToString());
            //-parampairs.Add("type", null);
            //parampairs.Add("otype", "json");
            //parampairs.Add("type", "mp4");
            //-parampairs.Add("player", "1");
            //-parampairs.Add("ts", ((long)((DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds)).ToString());
            //parampairs.Add("appkey", null);
            parampairs.Add("appsec", dlappsec);
            parampairs.Add("cid", cid.ToString());
            parampairs.Add("from", "miniplay");
            parampairs.Add("player", "1");
            //parampairs.Add("quality", "3");
            //string param = GetSign(parampairs);

            string final_param = "";
            foreach (var aparam in parampairs)
            {
                if (aparam.Value == null || aparam.Key == "appsec") continue;
                if (final_param != "") final_param += "&";
                final_param += aparam.Key + "=" + aparam.Value;
            }
            using (var md5 = MD5.Create())
            {
                string hashed = BitConverter.ToString(md5.ComputeHash(Encoding.ASCII.GetBytes(final_param + parampairs["appsec"]))).Replace("-", "").ToLower();
                final_param += "&sign=" + hashed;
            }

            string html = GetHtml("http://interface.bilibili.com/playurl?" + final_param);

            //string url = $"http://interface.bilibili.tv/playurl?appkey=95acd7f6cc3392f3&cid=";
            //string html = GetHtml(url + cid);
            if (!html.Contains("<result>su"))
            {
                Log.Error("FLV地址获取失败！ - CID：" + cid);
                return null;
            }

            byte[] byteArray = Encoding.UTF8.GetBytes(html);
            MemoryStream stream = new MemoryStream(byteArray);
            XElement xe = XElement.Load(stream);
            var t = xe.Elements("url");
            IEnumerable<string> elements = from ele in xe.Descendants("url") //where ele.Name == "url"
                                           select ele.Value;
            return elements.ToArray()[0];
        }

        public static string GetHtml(string url)
        {
            Log.Debug("获取网页 - " + url);
            try
            {
                WebClient myWebClient = new WebClient();
                myWebClient.Headers.Add("Cookie", cookie);
                myWebClient.Headers.Add("User-Agent", "Mozilla / 5.0(Windows NT 5.1) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 35.0.3319.102 Safari / 537.36");
                //myWebClient.Headers.Add("User-Agent", "Mozilla / 5.0 BiliDroid/3.3.0 (bbcallen@gmail.com)");
                Random ran = new Random();
                int ip4 = ran.Next(1, 255);
                int select = ran.Next(1, 2);
                string ip;
                if (select == 1)
                    ip = "220.181.111." + ip4;
                else
                    ip = "59.152.193." + ip4;
                myWebClient.Headers.Add("Client-IP", ip);

                byte[] myDataBuffer = myWebClient.DownloadData(url);

                string sContentEncoding = myWebClient.ResponseHeaders["Content-Encoding"];
                if (sContentEncoding == "gzip")
                {
                    MemoryStream ms = new MemoryStream(myDataBuffer);
                    MemoryStream msTemp = new MemoryStream();
                    int count = 0;
                    GZipStream gzip = new GZipStream(ms, CompressionMode.Decompress);
                    byte[] buf = new byte[1000];
                    while ((count = gzip.Read(buf, 0, buf.Length)) > 0)
                    {
                        msTemp.Write(buf, 0, count);
                    }
                    myDataBuffer = msTemp.ToArray();
                }

                return Encoding.UTF8.GetString(myDataBuffer);
            }
            catch
            {
                Log.Error("获取失败！请检查网路设置！");
                return null;
                //throw new Exception("获取失败");
            }
        }

        
    }
}
