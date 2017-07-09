using AngleSharp.Parser.Html;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace BiliRanking.Core
{
    public class BiliParse
    {
        private static HtmlParser htmlParser = new HtmlParser();
        /// <summary>
        /// 排序类型。注意：Default和New需要ToLower！
        /// </summary>
        public enum SortType
        {
            [Description("播放数")]
            hot,
            [Description("按新投稿排序")]
            Default,
            [Description("按新评论排序")]
            New,
            [Description("评论数")]
            review,
            [Description("弹幕数")]
            damku,
            [Description("用户评分")]
            comment,
            [Description("硬币数")]
            promote,
            [Description("按标题拼音排序")]
            pinyin,
            [Description("收藏（不存在于API文档）")]
            stow
        }

        public static List<string> GetListOld(SortType type, int zone, int page, DateTime from, DateTime to)
        {
            Log.Info("正在获取排行（旧版） - 依据" + type.ToString().ToLower() + "/分区" + zone + "/分页" + page + "/时间" + from.ToString("yyyy-MM-dd") + "~" + to.ToString("yyyy-MM-dd"));
            string url = "http://www.bilibili.com/list/" + type.ToString() + "-" + zone + "-" + page + "-" + from.ToString("yyyy-MM-dd") + "~" + to.ToString("yyyy-MM-dd") + ".html";
            string html = BiliInterface.GetHtml(url);
            if (html == null) return null;
            int p = html.IndexOf("href=\"/video/av");
            List<string> r = new List<string>();
            while (p > 0)
            {
                string s = html.Substring(p + 13, html.IndexOf("/", p + 13) - p - 13);
                if (!r.Contains(s))
                    r.Add(s);
                p = html.IndexOf("href=\"/video/av", p + 3);
            }
            return r;
        }

        public static List<string> GetList(int cate_id, DateTime from, DateTime to)
        {
            Log.Info($"正在获取排行 - 分区{cate_id} / 时间{from.ToString("yyyyMMdd")}~{to.ToString("yyyyMMdd")}");
            string url = "http://" + $"s.search.bilibili.com/cate/search?main_ver=v3&search_type=video&view_type=hot_rank&pic_size=160x100&order=click&copy_right=-1&cate_id={cate_id}&page=1&pagesize=150&time_from={from.ToString("yyyyMMdd")}&time_to={to.ToString("yyyyMMdd")}";
            string html = BiliInterface.GetHtml(url);
            if (html == null) return null;
            JObject obj = JObject.Parse(html);
            IEnumerable<string> avs = from n in obj["result"]
                                      select "av" + Regex.Match((string)n["arcurl"], @"\d+").Value;

            return avs.ToList();

        }



        public static List<string> GetSearch(string keyword, int tids_1, int tids_2, string order, DateTime needFrom, int need = 0)
        {
            int page = 1;
            List<string> re = new List<string>();
            string url = "http://" + $"search.bilibili.com/ajax_api/video?keyword={keyword}&page={page}&order={order}&tids_1={tids_1}&tids_2={tids_2}";
            string html = BiliInterface.GetHtml(url);
            if (html == null) return null;
            JObject obj = JObject.Parse(html);
            int numResults = (int)obj["numResults"];
            int numPages = (int)obj["numPages"];
            Log.Info($"找到{numResults}个，{numPages}页");
            for (int i = 2; i <= numPages + 1; i++)
            {
                string inhtml = (string)obj["html"];
                var dom = htmlParser.Parse(inhtml);
                foreach (var ul in dom.Children[0].Children[1].Children)
                {
                    string uploadtime = ul.QuerySelectorAll("span").Where(a => a.GetAttribute("title") == "上传时间").First().TextContent.Replace("\t", "").Replace("\n", "");
                    if (DateTime.Parse(uploadtime) >= needFrom.Date)
                    {
                        string avnum = ul.QuerySelector(".title").GetAttribute("href");
                        avnum = Regex.Match(avnum, @"[aA][vV]\d+").Value;
                        re.Add(avnum);
                    }
                    else
                    {
                        i = 99999;
                        break;
                    }
                }

                url = "http://" + $"search.bilibili.com/ajax_api/video?keyword={keyword}&page={i}&order={order}&tids_1={tids_1}&tids_2={tids_2}";
                html = BiliInterface.GetHtml(url);
                obj = JObject.Parse(html);
            }

            return re;
        }
    }
}
