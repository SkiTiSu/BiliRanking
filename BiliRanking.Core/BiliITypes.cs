using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliRanking.Core
{
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

    /// <summary>
    /// sdygx中保存的类，BiliInterface的壳子
    /// </summary>
    public class BiliShell
    {
        public int ver;
        public List<BiliInterfaceInfo> infos;
    }
}
