using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliRanking.Core
{
    //这个Model用来保存登录请求的access_key
    public class LoginModel
    {
        private string _access_key;
        public string access_key
        {
            get { return _access_key; }
            set { _access_key = value; }
        }
        public string mid { get; set; }
        public int code { get; set; }
        public string expires
        {
            get; set;
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

        public BiliVideoStat stat;

        public uint Fplay { get; set; }
        public uint Fcoins { get; set; }
        public uint Freview { get; set; }
        public uint Ffavorites { get; set; }
        public uint Fdefen { get; set; }
        public int Fpaiming { get; set; }
    }

    public class BiliVideoModel
    {
        public int code { get; set; }
        public object data { get; set; }

        //视频信息
        public string aid { get; set; }
        public string copyright { get; set; }
        public string pic { get; set; }
        public string title { get; set; }
        public string pubdate { get; set; }
        public string desc { get; set; }
        public string tname { get; set; }
        //UP信息
        public object owner { get; set; }
        public string mid { get; set; }
        public string name { get; set; }
        public string face { get; set; }
        //视频数据
        public object stat { get; set; }
        public string view { get; set; }
        public string danmaku { get; set; }
        public string reply { get; set; }
        public string favorite { get; set; }
        public string coin { get; set; }
        public string share { get; set; }
        //TAG
        public object tags { get; set; }
        //视频P
        public object pages { get; set; }
        public string cid { get; set; }
        public string page { get; set; }
        public string from { get; set; }
        public string part { get; set; }
        //番剧信息
        public object season { get; set; }
        public string season_id { get; set; }
        public string cover { get; set; }
        public int is_finish { get; set; }
        public int weekday { get; set; }
        public int total_count { get; set; }
        public string BanText
        {
            get
            {
                if (is_finish == 1)
                {
                    return string.Format("已完结,共{0}话", total_count);
                }
                else
                {
                    string we = string.Empty;
                    switch (weekday)
                    {
                        case 1:
                            we = "一";
                            break;
                        case 2:
                            we = "二";
                            break;
                        case 3:
                            we = "三";
                            break;
                        case 4:
                            we = "四";
                            break;
                        case 5:
                            we = "五";
                            break;
                        case 6:
                            we = "六";
                            break;
                        case 7:
                            we = "日";
                            break;
                        default:
                            break;
                    }
                    return string.Format("连载中,每周{0}更新", we);
                }
            }
        }
        //充电信息
        public object elec { get; set; }
        public bool show { get; set; }
        public int total { get; set; }
        public int count { get; set; }
        public object list { get; set; }
        public string pay_mid { get; set; }
        public int rank { get; set; }
        public string uname { get; set; }
        public string avatar { get; set; }
        public string message { get; set; }
        public int msg_deleted { get; set; }
        //视频关注信息
        public object req_user { get; set; }
        public int attention { get; set; }//关注Up主,-999为关注,1已关注
        //public int favorite { get; set; }//是否已经收藏，0为未收藏，1为已经收藏
        public object relates { get; set; }
        public string Play
        {
            get
            {
                if (Convert.ToInt32(view) > 10000)
                {
                    double d = (double)Convert.ToDouble(view) / 10000;
                    return d.ToString("0.0") + "万";
                }
                else
                {
                    return view;
                }
            }
        }
        public string Video_review
        {
            get
            {
                if (Convert.ToInt32(danmaku) > 10000)
                {
                    double d = (double)Convert.ToDouble(danmaku) / 10000;
                    return d.ToString("0.0") + "万";
                }
                else
                {
                    return danmaku;
                }
            }
        }
        public string Favorites
        {
            get
            {
                if (Convert.ToInt32(favorite) > 10000)
                {
                    double d = (double)Convert.ToDouble(favorite) / 10000;
                    return d.ToString("0.0") + "万";
                }
                else
                {
                    return favorite;
                }
            }
        }
        public string Coins
        {
            get
            {
                if (Convert.ToInt32(coin) > 10000)
                {
                    double d = (double)Convert.ToDouble(coin) / 10000;
                    return d.ToString("0.0") + "万";
                }
                else
                {
                    return coin;
                }
            }
        }

        public string Created_at
        {
            get
            {
                DateTime dtStart = new DateTime(1970, 1, 1, 8, 0, 0);
                long lTime = long.Parse(pubdate + "0000000");
                //如果使用9.0.1版的json.net需要按照下列方式书写，否则会因为多加的".0"报错
                //long lTime = Convert.ToInt64(Convert.ToDouble(pubdate).ToString("0") + "0000000");
                //已提交issue: https://github.com/JamesNK/Newtonsoft.Json/issues/1062 项目创始人回复不是bug，9.0.1出来就是这样
                TimeSpan toNow = new TimeSpan(lTime);

                DateTime dt = dtStart.Add(toNow);
                //if (dt.Date == DateTime.Now.Date)
                //{
                //    TimeSpan ts = DateTime.Now - dt;
                //    return ts.Hours + "小时前";
                //}
                //else
                //{
                    return dt.ToString("yyyy/MM/dd HH:mm");
                //}
            }
        }


        //public MovieModel movie { get; set; }

    }

    public class BiliVideoStat
    {
        public uint coin;
        public uint danmaku;
        public uint favorite;
        public uint his_rank;
        public uint now_rank;
        public uint reply;
        public uint share;
        public uint view;
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

    /// <summary>
    /// sdygx中保存的类，BiliInterface的壳子
    /// </summary>
    public class BiliShell
    {
        public int ver;
        public List<BiliInterfaceInfo> infos;
    }

    public class MessageModel
    {
        public int code { get; set; }
        public object data { get; set; }
        public string message { get; set; }

        public int reply_me { get; set; }
        public int praise_me { get; set; }
        public int notify_me { get; set; }
        public int at_me { get; set; }
        public int chat_me { get; set; }
    }
}
