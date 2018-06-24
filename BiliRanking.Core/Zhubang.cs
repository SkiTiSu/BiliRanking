using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

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

                Log.Info("正在生成 - " + infos[i].AVNUM);

                g.DrawString(infos[i].title, f, b, 350, 1000);
                g.DrawString(infos[i].AVNUM, f, b, 375, 900);
                g.DrawString(infos[i].created_at.Substring(0, infos[i].created_at.IndexOf(" ")), f, b, 900, 900);
                g.DrawString("UP:" + infos[i].author, f, b, 330, 800);

                Pen pp = new Pen(Color.Yellow, 3.5f);
                GraphicsPath pth = new GraphicsPath();
                pth.AddString(infos[i].Fpaiming.Value.ToString("D2"), new FontFamily("微软雅黑"), (int)FontStyle.Bold, 180, new Point(1750, 0), sf);
                g.FillPath(new SolidBrush(Color.White), pth);
                g.DrawPath(pp, pth);

                AddKongxin(infos[i].Fdefen.ToString(), 160, 960, 70);
                AddKongxin(infos[i].play.ToString(), 1625, 500, 70);
                AddKongxin(infos[i].review.ToString(), 1820, 330, 50);
                AddKongxin(infos[i].coins.ToString(), 1810, 710, 50);
                AddKongxin(infos[i].favorites.ToString(), 1600, 825, 60);

                string url = Environment.CurrentDirectory + @"\pic\Rank" + infos[i].Fpaiming + ".png";
                Log.Info("保存图片 " + url);
                image.Save(url);
            }

            Log.Info("主榜图片批量生成完成");
        }

        Pen p = new Pen(Color.Black, 3.5f);

        void AddKongxin(string s, int x, int y, int size)
        {
            GraphicsPath pth = new GraphicsPath();
            pth.AddString(s, new FontFamily("微软雅黑"), (int)FontStyle.Bold, size, new Point(x, y), sf);

            g.FillPath(new SolidBrush(Color.White), pth);
            g.DrawPath(p, pth);

        }

        public void GenStardust(List<BiliInterfaceInfo> infos, string template)
        {
            Log.Info("【主榜】开始生成" + infos.Count + "个图片");

 

            for (int i = 0; i < infos.Count; i++)
            {
                Log.Info("正在生成 - " + infos[i].AVNUM);

                List<TemplateInfo> tis = new List<TemplateInfo>();
                string text = template;
                text = DoReplace(text, infos[i]);
                var lines = Regex.Split(text, "\r\n|\r|\n");
                foreach (string line in lines)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    var cells = Regex.Split(line, "[|]");
                    TemplateInfo ti = new TemplateInfo()
                    {
                        text = cells[0],
                        font = new Font(cells[1], Convert.ToSingle(cells[2]), GraphicsUnit.Point),
                        brush = new SolidBrush(ColorTranslator.FromHtml(cells[3])),
                        isRightAlign = bool.Parse(cells[4])
                    };
                    if (!ti.isRightAlign)
                    {
                        ti.point = new PointF(Convert.ToSingle(cells[5]), Convert.ToSingle(cells[6]));
                    }
                    else
                    {
                        ti.rectangle = new RectangleF(Convert.ToSingle(cells[7]), Convert.ToSingle(cells[8]), Convert.ToSingle(cells[9]), Convert.ToSingle(cells[10]));
                    }
                    if (cells.Length >= 12)
                    {
                        ti.autoOffsetY = bool.Parse(cells[11]);
                    }
                    if (cells.Length >= 13)
                    {
                        ti.maxWidth = Convert.ToSingle(cells[12]);
                    }
                    tis.Add(ti);
                }

                Image img = GenStardustTemplate(tis);

                string url = Environment.CurrentDirectory + @"\pic\Rank" + infos[i].Fpaiming + ".png";
                Log.Info("保存图片 " + url);
                img.Save(url);

            }

            Log.Info("主榜图片批量生成完成");
        }

        public string DoReplace(string before, BiliInterfaceInfo info)
        {
            string after = before;

            after = after.Replace("<", "{");
            after = after.Replace(">", "}");

            after = after.Replace("{}", "\r\n");

            after = after.Replace("{huanhang}", "\r\n");
            after = after.Replace("{title}", info.title);
            after = after.Replace("{time}", info.created_at.Replace(" ", "　")); //半角换成全角
            after = after.Replace("{created_at}", info.created_at);
            after = after.Replace("{AVNUM}", info.AVNUM);
            after = after.Replace("{avnum}", info.avnum);
            after = after.Replace("{author}", info.author);
            after = after.Replace("{zongfen}", info.Fdefen.ToString());
            after = after.Replace("{paiming}", info.Fpaiming.Value.ToString("00"));
            after = after.Replace("{bofang}", info.play.ToString());
            after = after.Replace("{yingbi}", info.coins.ToString());
            after = after.Replace("{shoucang}", info.favorites.ToString());
            after = after.Replace("{danmu}", info.video_review.ToString());
            after = after.Replace("{pinglun}", info.review.ToString());
            after = after.Replace("{tag}", info.tag.ToString());
            after = after.Replace("{share}", info.share.ToString());

            return after;
        }

        public Image GenStardustTemplate(List<TemplateInfo> infos)
        {
            return GenWithTemplate(Properties.Resources.zhubang_stardust, infos);
        }

        public Image GenWithTemplate(Image bg, List<TemplateInfo> infos)
        {
            using (Graphics gp = Graphics.FromImage(bg))
            {
                int offsetY;
                gp.TextRenderingHint = TextRenderingHint.AntiAlias;
                gp.InterpolationMode = InterpolationMode.HighQualityBilinear;
                gp.CompositingQuality = CompositingQuality.HighQuality;
                gp.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gp.SmoothingMode = SmoothingMode.AntiAlias;

                foreach (TemplateInfo info in infos)
                {
                    if (info.text.StartsWith("{pic}"))
                    {
                        string picPath = FileManager.currentPath + @"\pic\" + info.text.Substring(5) + ".jpg";
                        try
                        {
                            using (Image pic = Image.FromFile(picPath))
                            {
                                gp.DrawImage(pic, info.rectangle);
                            }
                        }
                        catch (System.IO.FileNotFoundException e)
                        {
                            Log.Error("未找到该封面！请尝试重新获取：" + picPath);
                        }
                        catch (Exception e)
                        {
                            Log.Error("生成时发生未知错误：" + e.Message);
                        }

                    }
                    else
                    {
                        offsetY = CalOffsetY(info.font);
                        if (!info.isRightAlign)
                        {
                            if (info.autoOffsetY)
                                info.point.Y -= offsetY;
                            if (info.maxWidth > 0)
                            {
                                SizeF sf = gp.MeasureString(info.text, info.font);
                                while (sf.Width > info.maxWidth)
                                {
                                    info.text = info.text.Remove(info.text.Length - 1, 1);
                                    sf = gp.MeasureString(info.text, info.font);
                                }

                            }
                            gp.DrawString(info.text, info.font, info.brush, info.point);
                        }
                        else
                        {
                            if (info.autoOffsetY)
                                info.rectangle.Y -= offsetY;
                            gp.DrawString(info.text, info.font, info.brush, info.rectangle, new StringFormat() { Alignment = StringAlignment.Far });
                        }
                    }
                }
            }
            return bg;
        }

        public class TemplateInfo
        {
            public string text;
            public Font font;
            public Brush brush;
            public bool isRightAlign = false;
            public PointF point;
            public RectangleF rectangle;
            public bool autoOffsetY = true;
            public float maxWidth = 0; 
        }

        public static int CalOffsetY(Font f)
        {
            LOGFONT lg = new LOGFONT();
            f.ToLogFont(lg);
            Log.Debug($"Name:{f.Name} Font.Height: {f.Height} lfHeight: {lg.lfHeight}");
            return f.Height - Math.Abs(lg.lfHeight);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class LOGFONT
        {
            public int lfHeight = 0;
            public int lfWidth = 0;
            public int lfEscapement = 0;
            public int lfOrientation = 0;
            public int lfWeight = 0;
            public byte lfItalic = 0;
            public byte lfUnderline = 0;
            public byte lfStrikeOut = 0;
            public byte lfCharSet = 0;
            public byte lfOutPrecision = 0;
            public byte lfClipPrecision = 0;
            public byte lfQuality = 0;
            public byte lfPitchAndFamily = 0;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string lfFaceName = string.Empty;
        }
    }
}
