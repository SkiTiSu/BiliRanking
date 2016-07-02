using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace BiliRanking.Core
{
    public class Zhubang
    {
        Font f = new Font("微软雅黑", 52, FontStyle.Bold);
        Brush b = new SolidBrush(Color.Black);
        StringFormat sf = new StringFormat
        {
            Alignment = StringAlignment.Center
        };

        Graphics g;
        Image image;

        public void Gen(List<BiliInterfaceInfo> infos)
        {
            Log.Info("【主榜】开始生成" + infos.Count + "个图片");

            for (int i = 0; i < infos.Count; i++)
            {
                image = Properties.Resources.zhubang;
                g = Graphics.FromImage(image);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                ////
                Log.Info("正在生成 - " + infos[i].AVNUM);

                g.DrawString(infos[i].title, f, b, 350, 1000);
                g.DrawString(infos[i].AVNUM, f, b, 375, 900);
                g.DrawString(infos[i].created_at.Substring(0, infos[i].created_at.IndexOf(" ")), f, b, 900, 900);
                g.DrawString("UP:" + infos[i].author, f, b, 330, 800);

                Pen pp = new Pen(Color.Yellow, 3.5f);
                GraphicsPath pth = new GraphicsPath();
                pth.AddString(infos[i].Fpaiming.ToString("D2"), new FontFamily("微软雅黑"), (int)FontStyle.Bold, 180, new Point(1750, 0), sf);
                g.FillPath(new SolidBrush(Color.White), pth);
                g.DrawPath(pp, pth);

                AddKongxin(infos[i].Fdefen.ToString(), 160, 960, 70);
                AddKongxin(infos[i].play.ToString(), 1625, 500, 70);
                AddKongxin(infos[i].review.ToString(), 1820, 330, 50);
                AddKongxin(infos[i].coins.ToString(), 1810, 710, 50);
                AddKongxin(infos[i].favorites.ToString(), 1600, 825, 60);
                
                /*
                g.DrawString(infos[i].Fpaiming.ToString(), new Font("微软雅黑", 60, FontStyle.Bold), b, 110, nn - 30);


                g.DrawString(infos[i].AVNUM.Substring(2), f, b, 725, nn);
                g.DrawString(infos[i].Fdefen.ToString(), f, b, 1160, nn);
                if (infos[i].author.Length <= 6)
                {
                    g.DrawString(infos[i].author, f, b, new RectangleF(1518, nn, 320, 320));
                    //g.DrawString(infos[i].up, f, b, 1518, nn);
                }
                else
                {
                    g.DrawString(infos[i].author, f, b, new RectangleF(1518, nn, 320, 320));
                    //g.DrawString(infos[i].up.Substring(0, 6), f, b, 1518, nn);
                    //g.DrawString(infos[i].up.Substring(6), f, b, 1518, nn + 74);
                }


                g.DrawString(infos[i].created_at, f, b, 880, nn + 148);
                */
                

                ////

                string url = Environment.CurrentDirectory + @"\pic\Rank" + infos[i].Fpaiming + ".png";
                Log.Info("保存图片 " + url);
                image.Save(url);
            }

            Log.Info("主榜图片批量生成完成");
        }

        Pen p = new Pen(Color.Black, 3.5f);

        void AddKongxin(string s,int x,int y,int size)
        {
            GraphicsPath pth = new GraphicsPath();
            pth.AddString(s, new FontFamily("微软雅黑"), (int)FontStyle.Bold, size, new Point(x, y), sf);
            
            g.FillPath(new SolidBrush(Color.White), pth);
            g.DrawPath(p, pth);

            //g.DrawString(s, new Font("微软雅黑", size + 5, FontStyle.Bold), new SolidBrush(Color.Black), x, y, sf);
            //g.DrawString(s, new Font("微软雅黑", size, FontStyle.Bold), new SolidBrush(Color.White), x, y, sf);
        }
    }
}
