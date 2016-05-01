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
                    info.aid = avnum;
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
                myWebClient.Headers.Add("Cookie", "sid=arhywva7; fts=1461861233");
                myWebClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/35.0.3319.102 Safari/537.36");
                //MDZZ，mono的WebClient有bug，一转byte[]就报错
                string myData = myWebClient.DownloadString(url);

                return myData;

            }
            catch (Exception e)
            {
                Log.Error("获取失败！请检查网路设置！" + e.Message);
                return null;
                //throw new Exception("获取失败");
            }
        }
        
        /// <summary>
        /// 排序类型。注意：Default和New需要ToLower！
        /// </summary>
        public enum SortType
        {
            hot,
            Default,
            New,
            review,
            damku,
            comment,
            promote,
            pinyin,
            stow
        }

        public static List<string> GetList(SortType type, int zone, int page, DateTime from, DateTime to)
        {
            Log.Info("正在获取排行 - 依据" + type.ToString().ToLower() + "/分区" + zone + "/分页" + page + "/时间" + from.ToString("yyyy-MM-dd") + "~" + to.ToString("yyyy-MM-dd"));
            string url = "http://www.bilibili.com/list/" + type.ToString() + "-" + zone + "-" + page + "-" + from.ToString("yyyy-MM-dd") + "~" + to.ToString("yyyy-MM-dd") + ".html";
            string html = BiliInterface.GetHtml(url);
            if (html == null) return null;
            int p = 0;
            List<string> r = new List<string>();
            while (html.IndexOf("o/av", p) > 0)
            {
                p = html.IndexOf("o/av", p);
                string s = html.Substring(p + 2, html.IndexOf("/", p + 2) - p - 2);
                if (!r.Contains(s))
                    r.Add(s);

                p += 3;
            }
            return r;
        }

        public static void UpdateRanking()
        {
            Log.Info("开始获取排行");
            int needpage = 1;
            DateTime dateFrom;
            DateTime dateTo;

            if (DateTime.Now.Day <= 15)
            {
                dateFrom = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-16")).AddMonths(-1);
                dateTo = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddDays(-1);
            }
            else
            {
                dateFrom = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01"));
                dateTo = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-15"));
            }

            List<string> ss = new List<string>();
            List<BiliInterfaceInfo> infos = new List<BiliInterfaceInfo>();
            DateTime genTime = DateTime.Now;

            try
            {
                for (int i = 1; i <= needpage; i++)
                {
                    List<string> sts = GetList(SortType.hot, 119, i, dateFrom, dateTo);
                    if (sts != null)
                        ss.AddRange(sts);
                    else
                        break;
                }
                
                foreach (string s in ss)
                {
                    infos.Add(GetInfo(s));
                }

                infos.Sort(sortt);
                for (int i = 1; i <= infos.Count; i++)
                {
                    infos[i - 1].Fpaiming = i;
                }
            }
            catch
            {
                UpdateStatus += " （最近一次于" + genTime.ToString("yyyy-MM-dd HH:mm") + "获取数据失败！）";
                Log.Error("获取排行失败");
                return;
            }

            UpdateStatus = $"本期统计日期：{dateFrom.ToString("yyyy-MM-dd")} ~ {dateTo.ToString("yyyy-MM-dd")} 数据更新时间：{genTime.ToString("yyyy-MM-dd HH:mm")}";
            NowList = infos;

            Log.Info("获取排行完成");
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

        public static string Serialize(object o)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            System.Web.Script.Serialization.JavaScriptSerializer json = new System.Web.Script.Serialization.JavaScriptSerializer();
            json.Serialize(o, sb);
            return sb.ToString();
        }

        public static List<BiliInterfaceInfo> NowList { get; set; }
        public static string UpdateStatus { get; set; }

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
        public string aid {
            get; set;
        }
        public string AVNUM { get; set; }

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
