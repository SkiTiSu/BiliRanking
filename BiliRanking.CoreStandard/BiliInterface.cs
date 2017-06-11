using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BiliRanking.CoreStandard
{
    public static class BiliInterface
    {
        public static string cookie = "";
        const string appkey = "c1b107428d337928";
        const string appsec = "ea85624dfcf12d7cc7b2b3a94fac1f2c";
        //8e9fc618fbd41e28 不需要appsec
        const string dlappkey = "f3bb208b3d081dc8";
        const string dlappsec = "1c15888dc316e05a15fdd0a02ed6584f";
        //86385cdc024c0f6c
        /* 已经尝试的无效的appkey与appsec：
         * f3bb208b3d081dc8
         * c1b107428d337928 ea85624dfcf12d7cc7b2b3a94fac1f2c
         */

        public static string GetSign(SortedDictionary<string, string> sparam)
        {
            sparam.Add("_appver", "3040000");
            sparam.Add("_tid", "0");
            sparam.Add("_p", "1");
            sparam.Add("_down", "0");

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

        public enum ScoreType
        {
            None,
            Guichu,
            VC211
        }

        public static async Task<BiliVideoDataModel> GetInfoNewAsync(string AVnum, ScoreType stype = ScoreType.Guichu)
        {
            string avnum = GetAVdenum(AVnum);
            log.Info("正在通过API获取数据 - AV" + avnum);

            string uri = string.Format("http://app.bilibili.com/x/view?_device=wp&_ulv=10000&access_key={0}&aid={1}&appkey=422fd9d7289a1dd9&build=411005&plat=4&platform=android&ts={2}",
                BiliApiHelper.access_key, avnum, BiliApiHelper.GetTimeSpen);
            uri += "&sign=" + BiliApiHelper.GetSign(uri);

            Stopwatch sw = new Stopwatch();
            sw.Restart();
            string html = await GetHtmlAsync(uri);
            log.Info($"获取数据完成 - AV{avnum} 用时：{sw.ElapsedMilliseconds}ms");
            sw.Stop();

            BiliInterfaceInfo info = new BiliInterfaceInfo();
            info.AVNUM = "AV" + avnum;
            try
            {
                BiliVideoModel model = JsonConvert.DeserializeObject<BiliVideoModel>(html);

                if (model.code == -403)
                {
                    if (model.data.ToString().Contains("no perm"))
                    {
                        log.Error("没有数据！（正在补档或被删除？）"); //TODO: 在新版API中还需要吗？
                    }
                    else
                    {
                        log.Error("本视频为会员独享，或账号方面错误！");
                    }
                }
                else if (model.code == -404)
                {
                    log.Error("视频不存在！");
                }
                else if (model.code == -500)
                {
                    log.Error("服务器错误，代码-500，请稍后再试");
                }
                else if (model.code == -502)
                {
                    log.Error("网关错误，代码-502，请稍后再试");
                }
                else
                {
                    BiliVideoDataModel InfoModel = JsonConvert.DeserializeObject<BiliVideoDataModel>(model.data.ToString());
                    return InfoModel;
                }
            }
            catch (Exception e)
            {
                log.Error("AV" + avnum + "的数据发生错误，请稍后重试！" + e.Message);
                //return null; //TODO: 出错时返回后的检查方式需要变更
            }

            return null;
        }

        public static async Task<BiliInterfaceInfo> GetInfoAsync(string AVnum, ScoreType stype = ScoreType.Guichu)
        {
            string avnum = GetAVdenum(AVnum);
            log.Info("正在通过API获取数据 - AV" + avnum);

            string uri = string.Format("http://app.bilibili.com/x/view?_device=wp&_ulv=10000&access_key={0}&aid={1}&appkey=422fd9d7289a1dd9&build=411005&plat=4&platform=android&ts={2}",
                BiliApiHelper.access_key, avnum, BiliApiHelper.GetTimeSpen);
            uri += "&sign=" + BiliApiHelper.GetSign(uri);

            Stopwatch sw = new Stopwatch();
            sw.Restart();
            string html = await GetHtmlAsync(uri);
            log.Info($"获取数据完成 - AV{avnum} 用时：{sw.ElapsedMilliseconds}ms");
            sw.Stop();

            BiliInterfaceInfo info = new BiliInterfaceInfo();
            info.AVNUM = "AV" + avnum;
            try
            {
                BiliVideoModel model = JsonConvert.DeserializeObject<BiliVideoModel>(html);

                if (model.code == -403)
                {
                    if (model.data.ToString().Contains("no perm"))
                    {
                        log.Error("没有数据！（正在补档或被删除？）"); //TODO: 在新版API中还需要吗？
                    }
                    else
                    {
                        log.Error("本视频为会员独享，或账号方面错误！");
                    }
                }
                else if (model.code == -404)
                {
                    log.Error("视频不存在！");
                }
                else if (model.code == -500)
                {
                    log.Error("服务器错误，代码-500，请稍后再试");
                }
                else if (model.code == -502)
                {
                    log.Error("网关错误，代码-502，请稍后再试");
                }
                else
                {
                    //基础信息
                    BiliVideoModel InfoModel = JsonConvert.DeserializeObject<BiliVideoModel>(model.data.ToString());
                    //UP信息
                    BiliVideoModel UpModel = JsonConvert.DeserializeObject<BiliVideoModel>(InfoModel.owner.ToString());
                    //数据信息
                    BiliVideoModel DataModel = JsonConvert.DeserializeObject<BiliVideoModel>(InfoModel.stat.ToString());
                    //关注信息
                    BiliVideoModel AttentionModel = JsonConvert.DeserializeObject<BiliVideoModel>(InfoModel.req_user.ToString());
                    //分P信息
                    info.pagesn = JsonConvert.DeserializeObject<List<BiliVideoModel>>(InfoModel.pages.ToString());
                    //--数据转换开始--
                    info.title = InfoModel.title;
                    info.created_at = InfoModel.Created_at;
                    info.typename = InfoModel.tname;
                    info.pic = InfoModel.pic;
                    info.author = UpModel.name;
                    info.cid = Convert.ToUInt32(info.pagesn[0].cid);
                    info.play = Convert.ToUInt32(DataModel.view);
                    info.video_review = Convert.ToUInt32(DataModel.danmaku);
                    info.review = Convert.ToUInt32(DataModel.reply);
                    info.coins = Convert.ToUInt32(DataModel.coin);
                    info.favorites = Convert.ToUInt32(DataModel.favorite);
                    info.tag = "";
                    if (InfoModel.tags != null) //注意有的视频竟然会没有tag
                    {
                        string[] pretags = ((JArray)InfoModel.tags).ToObject<string[]>();

                        foreach (string pretag in pretags)
                        {
                            info.tag += "," + pretag;
                        }
                        info.tag = info.tag.Substring(1);
                    }
                    info.description = InfoModel.desc;
                    //--数据转换结束--
                    //info.title = HttpUtility.HtmlDecode(info.title);
                    //--or
                    info.title = info.title.Replace("&amp;", "&");
                    info.title = info.title.Replace("&lt;", "<");
                    info.title = info.title.Replace("&gt;", ">");
                    info.title = info.title.Replace("&quot;", "\"");
                    switch (stype)
                    {
                        case ScoreType.None:
                            break;
                        case ScoreType.Guichu:
                            CalScoreGuichu(ref info);
                            break;
                        case ScoreType.VC211:
                            CalScoreVC211(ref info);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("AV" + avnum + "的数据发生错误，请稍后重试！" + e.Message);
                //return null; //TODO: 出错时返回后的检查方式需要变更
            }

            return info;
        }

        public static BiliInterfaceInfo GetInfo(string AVnum, ScoreType stype = ScoreType.Guichu) => AsyncHelper.RunSync(() => GetInfoAsync(AVnum, stype));

        public static Task<BiliInterfaceInfo> GetInfoTaskAsync(string s, ScoreType stype = ScoreType.Guichu) => Task.Run(() => GetInfoAsync(s, stype));

        public static void CalScoreGuichu(ref BiliInterfaceInfo info)
        {
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

        public static void CalScoreVC211(ref BiliInterfaceInfo info)
        {
            //修正B
            double xiuzhengB = Math.Round((double)info.favorites / info.play * 250, 2);
            if (xiuzhengB > 50) xiuzhengB = 50;
            //播放得点
            info.Fplay = info.play / (uint)info.pagesCount;
            if (info.Fplay > 10000)
            {
                info.Fplay = (uint)(info.Fplay * 0.5) + 5000;
            }
            if (xiuzhengB < 10)
            {
                info.Fplay = (uint)(info.Fplay * xiuzhengB * 0.1);
            }
            //修正A
            double xiuzhengA = Math.Round((double)(info.Fplay + info.favorites) / (info.play + info.favorites + info.video_review * 10 + info.review * 20), 2);
            //总分
            info.Fdefen = (uint)(info.Fplay + (info.review * 25 + info.video_review) * xiuzhengA + info.favorites * xiuzhengB);
        }

        public static string GetCsvInfos(List<BiliInterfaceInfo> infos)
        {
            StringBuilder csv = new StringBuilder();
            csv.AppendLine("AV号,标题,播放数,弹幕数,收藏数,硬币数,评论数,up,时间,分区,播放得分,收藏得分,硬币得分,评论得分,总分");
            foreach (BiliInterfaceInfo info in infos)
            {
                string[] columns = new string[] { info.avnum, info.title.Replace("\0",""), info.play.ToString(), info.video_review.ToString(), //为毛av6859961的标题后面有个\0？导致textbox显示到那边就不往后显示了，为毛！！
                    info.favorites.ToString(), info.coins.ToString(), info.review.ToString(), info.author, info.created_at,
                    info.typename, info.Fplay.ToString(), info.Ffavorites.ToString(), info.Fcoins.ToString(),
                    info.Freview.ToString()};
                csv.Append("\"");
                foreach (string column in columns)
                {
                    csv.Append(column);
                    csv.Append("\",\"");
                }
                csv.Append(info.Fdefen.ToString());
                csv.AppendLine("\"");
            }
            return csv.ToString();
        }

        public static Task<string> GetCsvInfosAsync(List<BiliInterfaceInfo> infos) => Task.Run(() => GetCsvInfos(infos));

        public static BiliInterfaceInfo GetFlvInfo(string AVnum)
        {
            string[] avnp = Regex.Split(AVnum, "_|-|#");
            string avnum = GetAVdenum(avnp[0]);

            int page = 1;
            if (avnp.Length > 1)
            {
                try
                {
                    page = int.Parse(avnp[1]);
                }
                catch
                {
                    log.Warn(AVnum + " - 无法识别分P编号，将下载P1");
                }

            }

            BiliInterfaceInfo info = null;

            try
            {
                info = GetInfo(avnum);
                if (page > 1)
                {
                    if (info.pagesn.Count >= page)
                    {
                        info.title = info.title + $"_P{page}_{info.pagesn[page - 1].part}";
                        info.flvurl = GetFlvUrl(UInt32.Parse(info.avnum.Substring(2)), uint.Parse(info.pagesn[page - 1].cid));
                    }
                    else
                    {
                        log.Warn(AVnum + $" - 目标视频仅有{info.pagesn.Count}P，将下载P1");
                        if (info.pagesn.Count > 1)
                        {
                            info.title = info.title + $"_P{page}_{info.pagesn[page - 1].part}";
                        }
                        info.flvurl = GetFlvUrl(UInt32.Parse(info.avnum.Substring(2)), info.cid);
                    }
                }
                else
                {
                    if (info.pagesn.Count > 1)
                    {
                        info.title = info.title + $"_P{page}_{info.pagesn[page - 1].part}";
                    }
                    info.flvurl = GetFlvUrl(UInt32.Parse(info.avnum.Substring(2)), info.cid);
                }

            }
            catch (Exception e)
            {
                log.Error("AV" + avnum + "的数据发生错误，请稍后重试！" + e.Message);
            }

            return info;
        }

        public static IEnumerable<string> GetAVFlvUrl(string AVnum)
        {
            string[] avnp = Regex.Split(AVnum, "_|-|#");
            string avnum = GetAVdenum(avnp[0]);

            int page = 1;
            if (avnp.Length > 1)
            {
                try
                {
                    page = int.Parse(avnp[1]);
                }
                catch
                {
                    log.Warn(AVnum + " - 无法识别分P编号，将下载P1");
                }

            }

            BiliInterfaceInfo info = null;
            IEnumerable<string> flvs = null;

            try
            {
                info = GetInfo(avnum);
                if (page > 1)
                {
                    if (info.pagesn.Count >= page)
                    {
                        info.title = info.title + $"_P{page}_{info.pagesn[page - 1].part}";
                        flvs = GetFlvUrls(uint.Parse(info.pagesn[page - 1].cid));
                    }
                    else
                    {
                        log.Warn(AVnum + $" - 目标视频仅有{info.pagesn.Count}P，将下载P1");
                        if (info.pagesn.Count > 1)
                        {
                            info.title = info.title + $"_P{page}_{info.pagesn[page - 1].part}";
                        }
                        flvs = GetFlvUrls(info.cid);
                    }
                }
                else
                {
                    if (info.pagesn.Count > 1)
                    {
                        info.title = info.title + $"_P{page}_{info.pagesn[page - 1].part}";
                    }
                    flvs = GetFlvUrls(info.cid);
                }

            }
            catch (Exception e)
            {
                log.Error("AV" + avnum + "的数据发生错误，请稍后重试！" + e.Message);
            }

            return flvs;
        }

        public static string GetFlvUrl(uint aid, uint cid)
        {
            var t = GetFlvUrls(cid);
            if (t != null)
            {
                log.Debug("获取到下载地址：" + t.ToArray()[0]);
                return t.ToArray()[0];
            }
            else
            {
                return null;
            }
        }

        public static IEnumerable<string> GetFlvUrls(uint cid)
        {
            SortedDictionary<string, string> parampairs = new SortedDictionary<string, string>();
            parampairs.Add("appsec", dlappsec);
            parampairs.Add("cid", cid.ToString());
            parampairs.Add("from", "miniplay");
            parampairs.Add("player", "1");
            //parampairs.Add("quality", "3");
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
            if (!html.Contains("<result>su"))
            {
                log.Error("FLV地址获取失败！ - CID：" + cid);
                return null;
            }
            byte[] byteArray = Encoding.UTF8.GetBytes(html);
            MemoryStream stream = new MemoryStream(byteArray);
            XElement xe = XElement.Load(stream);
            var t = xe.Elements(XName.Get("durl"));
            var tt = from ele in t
                     select ele.Element("url").Value;
            log.Debug($"找到{tt.Count()}个分段");
            return tt;
        }

        public static async void GetTagSort(int tid, string tags, int page = 1, int pagesize = 30, string order ="default")
        {
            string uri = string.Format($"http://api.bilibili.cn/tags?appkey=4ebafd7c4951b366&tid={tid}&tags={tags}&page={page}&pagesize={pagesize}&order={order}&ts={BiliApiHelper.GetTimeSpen}");
            uri += "&sign=" + BiliApiHelper.GetSign(uri, "8cb98205e9b2ad3669aad0fce12a4c13");

            string html = await GetHtmlAsync(uri);
            Console.ReadKey();
        }

        public static string GetHtml(string url) => AsyncHelper.RunSync(() => GetHtmlAsync(url));

        public static async Task<string> GetHtmlAsync(string url)
        {
            log.Debug("获取网页 - " + url);
            try
            {
                HttpClient hc = new HttpClient((new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip
                }));
                hc.DefaultRequestHeaders.Add("Cookie", cookie);
                hc.DefaultRequestHeaders.Add("UserAgent", "Mozilla / 5.0(Windows NT 5.1) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 35.0.3319.102 Safari / 537.36");
                //myWebClient.Headers.Add("User-Agent", "Mozilla / 5.0 BiliDroid/3.3.0 (bbcallen@gmail.com)");
                Random ran = new Random();
                int ip4 = ran.Next(1, 255);
                int select = ran.Next(1, 2);
                string ip;
                if (select == 1)
                    ip = "220.181.111." + ip4;
                else
                    ip = "59.152.193." + ip4;
                hc.DefaultRequestHeaders.Add("Client-IP", ip);

                byte[] myDataBuffer = await hc.GetByteArrayAsync(new Uri(url));

                //string sContentEncoding = hc.ResponseHeaders["Content-Encoding"];
                //if (sContentEncoding == "gzip")
                //{
                //    MemoryStream ms = new MemoryStream(myDataBuffer);
                //    MemoryStream msTemp = new MemoryStream();
                //    int count = 0;
                //    GZipStream gzip = new GZipStream(ms, CompressionMode.Decompress);
                //    byte[] buf = new byte[1000];
                //    while ((count = gzip.Read(buf, 0, buf.Length)) > 0)
                //    {
                //        msTemp.Write(buf, 0, count);
                //    }
                //    myDataBuffer = msTemp.ToArray();
                //}
                return Encoding.UTF8.GetString(myDataBuffer);

            }
            catch (Exception e)
            {
                log.Error("获取失败！请检查网路设置！" + e.Message);
                return null;
            }
        }
    }
}
