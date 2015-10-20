using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace BiliRanking
{
    public class BiliParse
    {
        public enum SortType
        {
            [Description("播放数")]
            hot,
            [Description("收藏")]
            stow,
            [Description("评论数")]
            review,
            [Description("硬币数")]
            promote,
            [Description("用户评分")]
            comment,
            [Description("弹幕数")]
            damku
        }

        public static List<string> GetList(SortType type, int zone,int page,DateTime from,DateTime to)
        {
            Log.Info("正在获取排行 - 依据" + type.ToString() + "/分区" + zone + "/分页" + page + "/时间" + from.ToString("yyyy-MM-dd") + "~" + to.ToString("yyyy-MM-dd"));
            string url = "http://www.bilibili.com/list/" + type.ToString() + "-" + zone + "-" + page + "-" + from.ToString("yyyy-MM-dd") + "~" + to.ToString("yyyy-MM-dd") + ".html";
            string html = BiliInterface.GetHtml(url);
            if (html == null) return null;
            int p = 0;
            List<string> r = new List<string>();
            while (html.IndexOf("/av", p) > 0)
            {
                p = html.IndexOf("/av", p);
                string s = html.Substring(p + 1, html.IndexOf("/", p + 1) - p - 1);
                if (!r.Contains(s))
                    r.Add(s);

                p += 3;
            }
            return r;
        }
    }
}
