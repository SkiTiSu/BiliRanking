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

namespace BiliRanking
{
    public static class BiliInterface
    {
        const string appkey = "95acd7f6cc3392f3";
        const string InterfaceUrl = "http://api.bilibili.com/view?type=json&appkey=95acd7f6cc3392f3&id=";
        

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
            /*
            string h5url = "http://www.bilibili.com/m/html5?aid=" + avnum + "&page=" + page;
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
            */
            string url = $"http://interface.bilibili.tv/playurl?appkey={appkey}&cid=";
            string html = GetHtml(url + cid + "&quality=4&type=mp4");
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

        public static BiliInterfaceInfo GetMP4info(string AVnum, int page)
        {
            /*
            BiliInterfaceInfo info = new BiliInterfaceInfo();
             string avnum = GetAVdenum(AVnum);
            Log.Info("开始获取MP4视频 - AV" + avnum);
            string h5url = "http://www.bilibili.com/m/html5?aid=" + avnum + "&page=" + page;
            try
            {
                string html = GetHtml(h5url);
                JavaScriptSerializer j = new JavaScriptSerializer();
                BiliH5videoInfo infoh = new BiliH5videoInfo();
                infoh = j.Deserialize<BiliH5videoInfo>(html);
                if (infoh.src == "http://static.hdslb.com/error.mp4")
                {
                    Log.Error("错误的AV号或页码！（还是没转码出来？...）");
                    return null;
                }
                else
                {
                    info.mp4url = infoh.src;
                    info.AVNUM = "AV" + GetAVdenum(AVnum);
                    info.cid = 233333;
                    if (info.mp4url.IndexOf("letv") > 0)
                    {
                        info.title = "(乐视源）MP4不获取title";
                    }
                    else if (info.mp4url.IndexOf("acgvideo") > 0)
                    {
                        info.title = "(B站源）MP4不获取title";
                    }
                    else
                    {
                        info.title = "(未知源）MP4不获取title";
                    }

                    info.pic = infoh.img;
                    return info;
                }
            }
            catch (Exception e)
            {
                Log.Error("AV" + avnum + "的数据发生错误，请稍后重试！" + e.Message);
                return null;
            }
            */
            string avnum = AVnum.ToUpper();
            if (avnum.Contains("AV"))
            {
                avnum = avnum.Substring(2, avnum.Length - 2);
            }

            Log.Info("正在获取API数据 - AV" + avnum);

            string html = GetHtml(InterfaceUrl + avnum);
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

                    info.mp4url = GetMP4Url(info.cid);

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

            Log.Info("正在获取API数据 - AV" + avnum);

            string html = GetHtml(InterfaceUrl + avnum);
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
                    xiuzheng = (double)(info.play / info.favorites) * 1500;
                    if (xiuzheng > 55)
                        xiuzheng = 55;
                    info.Ffavorites = Convert.ToUInt32(info.favorites * xiuzheng);

                    //硬币
                    xiuzheng = (double)(info.play / info.coins) * 5000;
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

            Log.Info("正在获取API数据 - AV" + avnum);

            string html = GetHtml(InterfaceUrl + avnum);
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

                    info.flvurl = GetFlvUrl(info.cid);

                }
            }
            catch (Exception e)
            {
                Log.Error("AV" + avnum + "的数据发生错误，请稍后重试！" + e.Message);
            }

            return info;
        }

        public static string GetFlvUrl(uint cid)
        {
            string url = $"http://interface.bilibili.tv/playurl?appkey={appkey}&cid=";
            string html = GetHtml(url + cid);
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
            try
            {
                WebClient myWebClient = new WebClient();
                myWebClient.Headers.Add("Cookie", FormMain.cookie);
                byte[] myDataBuffer = myWebClient.DownloadData(url);

                string sContentEncoding = myWebClient.ResponseHeaders["Content-Encoding"];
                if (sContentEncoding == "gzip")
                {
                    MemoryStream ms = new MemoryStream(myDataBuffer);
                    MemoryStream msTemp = new MemoryStream();
                    int count = 0;
                    GZipStream gzip = new GZipStream(ms,CompressionMode.Decompress);
                    byte[] buf = new byte[1000];
                    while ((count = gzip.Read(buf, 0, buf.Length))> 0)
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

    //TODO:API已更新！
    [Serializable]
    public class BiliInterfaceInfo
    {
        public uint tid;
        public string typename { get; set; }
        public uint play { get; set; }
        public uint review { get; set; }
        public uint video_review { get; set; }
        public uint favorites { get; set; }
        public string title { get; set; }
        public uint allow_bp;
        public uint allow_feed;
        public uint allow_download;
        public string description { get; set; }
        public string tag { get; set; }
        public string pic { get; set; }
        public string author { get; set; }
        public uint mid;
        public string face;
        public uint pages { get; set; }
        public string instant_server;
        public ulong created { get; set; }
        public string create
        {
            get
            {
                return created_at;
            }
            set
            {
                created_at = value;
            }
        }
        public string created_at { get; set; }
        //TODO: 评分数量有时候会是--
        //public uint credit{get; set; }
        public uint coins { get; set; }
        public string spid;
        public string src;
        public uint cid { get; set; }
        public string partname;
        public string offsite;
        public int code;
        public string error;
        public int aid
        {
            set
            {
                AVNUM = "AV" + value;
            }
        }
        public string AVNUM { get; set; }
        public string avnum
        {
            get
            {
                return AVNUM.ToLower();
            }
        }

        public string mp4url;
        public string flvurl;

        public uint Fplay { get; set; }
        public uint Fcoins { get; set; }
        public uint Freview { get; set; }
        public uint Ffavorites { get; set; }
        public uint Fdefen { get; set; }
        public int Fpaiming { get; set; }
    }
    
    public class BiliH5videoInfo
    {
        public string img;
        public string cid;
        public string src;
    }


    public class BiliIndexMainInfo
    {

    }
    public class BiliIndexInfo
    {
        public int code;
        public BiliInterfaceInfo[] list;
        public string name;
        public int num;
        public int pages;
        public int resaults;
    }
}
