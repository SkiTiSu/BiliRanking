using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text.RegularExpressions;

namespace BiliRanking.Core
{
    public class Fubang
    {
        Font f = new Font("微软雅黑", 48);
        Brush b = new SolidBrush(Color.Black);

        Graphics g;
        Image image;

        public void Gen(List<BiliInterfaceInfo> infos)
        {
            //Gen2(infos);
            //return;

            Log.Info("【副榜】开始生成" + infos.Count + "个视频的图片（每3个一张图）");
            foreach (BiliInterfaceInfo info in infos)
            {
                BiliInterface.GetPic(info);
            }
            for (int i = 0; i < infos.Count; i += 3)
            {
                image = Properties.Resources.fubang;//Image.FromFile("fubang.png");
                g = Graphics.FromImage(image);

                AddPic(infos[i], 1);
                if (i + 1 < infos.Count)
                { 
                    AddPic(infos[i + 1], 2);
                }
                if (i + 2 < infos.Count)
                {
                    AddPic(infos[i + 2],3);
                }
                    
                string url = Environment.CurrentDirectory + @"\pic\Rank" + infos[i].Fpaiming + "-" + (infos[i].Fpaiming + 2) + ".jpg";
                Log.Info("保存图片 " + url);
                image.Save(url);
            }
            
            Log.Info("副榜图片批量生成完成");
        }

        public void AddPic(BiliInterfaceInfo info, int n)
        {
            Log.Info("正在生成 - " + info.AVNUM);
            string picPath = Environment.CurrentDirectory + @"\pic\" + info.AVNUM + ".jpg";
            int nn = 190 + 292 * (n - 1);
            try
            {
                Image pic = Image.FromFile(picPath);
                g.DrawImage(pic, new Rectangle(250, nn - 15, 366, 218));
            }
            catch
            {
                Log.Error(info.AVNUM + " - 找不到封面文件，请在左侧窗格输入AV号尝试或手动获取！");
            }

            if(info.Fpaiming < 100)
            {
                g.DrawString(info.Fpaiming.ToString(), new Font("微软雅黑", 80, FontStyle.Bold), b, 110, nn - 30);
            }
            else
            {
                g.DrawString(info.Fpaiming.ToString(), new Font("微软雅黑", 60, FontStyle.Bold), b, 110, nn - 30);
            }
            

            g.DrawString(info.AVNUM.Substring(2), f, b, 725, nn);
            g.DrawString(info.Fdefen.ToString(), f, b, 1160, nn);
            if(info.author.Length <= 6)
            {
                g.DrawString(info.author, f, b, new RectangleF(1518, nn, 320, 320));
                //g.DrawString(info.up, f, b, 1518, nn);
            }
            else
            {
                g.DrawString(info.author, f, b, new RectangleF(1518, nn, 320, 320));
                //g.DrawString(info.up.Substring(0, 6), f, b, 1518, nn);
                //g.DrawString(info.up.Substring(6), f, b, 1518, nn + 74);
            }
            

            g.DrawString(info.created_at, f, b, 880, nn + 148);

            g.DrawString(info.title, new Font("微软雅黑", 32), b, 102, nn - 81);

        }

        public void Gen2(List<BiliInterfaceInfo> infos)
        {
            Log.Info("【副榜】开始生成" + infos.Count + "个视频的图片（每3个一张图）");
            foreach (BiliInterfaceInfo info in infos)
            {
                BiliInterface.GetPic(info);
            }
            for (int i = 0; i < infos.Count; i += 3)
            {
                image = Properties.Resources.fubang2;
                g = Graphics.FromImage(image);
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBilinear;

                AddPic2(infos[i], 1);
                if (i + 1 < infos.Count)
                {
                    AddPic2(infos[i + 1], 2);
                }
                if (i + 2 < infos.Count)
                {
                    AddPic2(infos[i + 2], 3);
                }

                string url = Environment.CurrentDirectory + @"\pic\Rank" + infos[i].Fpaiming + "-" + (infos[i].Fpaiming + 2) + ".png";
                Log.Info("保存图片 " + url);
                image.Save(url);
            }

            Log.Info("副榜图片批量生成完成");
        }

        public void AddPic2(BiliInterfaceInfo info, int n)
        {
            Log.Info("正在生成 - " + info.AVNUM);
            string picPath = Environment.CurrentDirectory + @"\pic\" + info.AVNUM + ".jpg";
            int x = 36;
            int y = 37 + 233 * (n - 1);
            try
            {
                Image pic = Image.FromFile(picPath);
                Image smallpic = resizeImage(pic, new Size(576, 360));
            //288 180
                Bitmap bmpFluffy = new Bitmap(smallpic);
                //bmpFluffy = new Bitmap(288, 180);
                Rectangle r = new Rectangle(Point.Empty, bmpFluffy.Size);

                using (Bitmap bmpMask = new Bitmap(r.Width, r.Height))
                using (Graphics g = Graphics.FromImage(bmpMask))
                using (GraphicsPath path = createRoundRect(
                    r.X, r.Y,
                    r.Width, r.Height,
                    8))
                {
                    g.FillRectangle(Brushes.Black, r);

                g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                g.FillPath(Brushes.White, path);
                
                    transferOneARGBChannelFromOneBitmapToAnother(
                        bmpMask,
                        bmpFluffy,
                        ChannelARGB.Blue,
                        ChannelARGB.Alpha);
                }
            // bmpFluffy is now powered up and ready to be used



            //Image roundedpic = RoundCorners(smallpic, 10);

            //g.DrawImage(roundedpic, 36, y);
            //g.DrawImageUnscaled(roundedpic, 36, y);
            g.DrawImage(bmpFluffy, new Rectangle(x, y, 288, 180));
                //g.DrawImage(pic, new Rectangle(250, nn - 15, 366, 218));
            }
            catch (Exception e)
            {
                //throw e;
                Log.Error(info.AVNUM + " - 封面文件获取失败，请在左侧窗格输入AV号尝试或手动获取！");
            }
            //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //Source Han Sans SC = 思源黑体
            g.DrawString(info.title.Replace('【', '[').Replace('】',']'), new Font("思源黑体 Light", 42, GraphicsUnit.Pixel), b, 340, y - 2);
            g.DrawString($"分数: {info.Fdefen}   UP: {info.author}", new Font("思源黑体 Normal", 30, GraphicsUnit.Pixel), b, 340, y + 56);
            g.DrawString($"{info.avnum} 投稿时间: {info.created_at}", new Font("思源黑体 Regular", 25, GraphicsUnit.Pixel), b, 340, y + 142);

            if (info.Fpaiming < 100)
            {
                g.DrawString(info.Fpaiming.ToString(), new Font("思源黑体 ExtraLight", 72, GraphicsUnit.Pixel), b, 1152, y + 87);
            }
            else
            {
                g.DrawString(info.Fpaiming.ToString(), new Font("思源黑体 ExtraLight", 72, GraphicsUnit.Pixel), b, 1114, y + 87);
            }

            /*
            if (info.Fpaiming < 100)
            {
                g.DrawString(info.Fpaiming.ToString(), new Font("微软雅黑", 80, FontStyle.Bold), b, 110, nn - 30);
            }
            else
            {
                g.DrawString(info.Fpaiming.ToString(), new Font("微软雅黑", 60, FontStyle.Bold), b, 110, nn - 30);
            }


            g.DrawString(info.AVNUM.Substring(2), f, b, 725, nn);
            g.DrawString(info.Fdefen.ToString(), f, b, 1160, nn);
            if (info.author.Length <= 6)
            {
                g.DrawString(info.author, f, b, new RectangleF(1518, nn, 320, 320));
                //g.DrawString(info.up, f, b, 1518, nn);
            }
            else
            {
                g.DrawString(info.author, f, b, new RectangleF(1518, nn, 320, 320));
                //g.DrawString(info.up.Substring(0, 6), f, b, 1518, nn);
                //g.DrawString(info.up.Substring(6), f, b, 1518, nn + 74);
            }


            g.DrawString(info.created_at, f, b, 880, nn + 148);

            g.DrawString(info.title, new Font("微软雅黑", 32), b, 102, nn - 81);
            */
        }

        public static void GenWithTemplate(IEnumerable<BiliInterfaceInfo> binfos, Image bg, string template, int repeat, float offset)
        {
            Zhubang zb = new Zhubang();
            foreach (BiliInterfaceInfo info in binfos)
            {
                BiliInterface.GetPic(info);
            }
            
            List<Zhubang.TemplateInfo> tinfos = new List<Zhubang.TemplateInfo>(); 
            for (int j = 0; j<= binfos.Count(); j += repeat)
            {
                Image image = (Image)bg.Clone();
                for (int i = 0; i < repeat; i++)
                {
                    if (j + i >= binfos.Count())
                        break;
                    tinfos = ParseTemplate(binfos.ElementAt(j + i), template);
                    foreach (var tinfo in tinfos)
                    {
                        if (tinfo.point != null)
                            tinfo.point.Y += offset * i;
                        if (tinfo.rectangle != null)
                            tinfo.rectangle.Y += offset * i;
                    }
                    image = zb.GenWithTemplate(image, tinfos);
                }
                string url = FileManager.currentPath + @"\pic\Rank" + binfos.ElementAt(j).Fpaiming + "-" + (binfos.ElementAt(j).Fpaiming + repeat - 1) + ".jpg";
                image.Save(url);
                image.Dispose();
            }
        }

        public static List<Zhubang.TemplateInfo> ParseTemplate(BiliInterfaceInfo info, string template)
        {
            List<Zhubang.TemplateInfo> tis = new List<Zhubang.TemplateInfo>();
            var lines = Regex.Split(template, "\r\n|\r|\n");
            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;
                var cells = Regex.Split(line, "[|]");
                Zhubang.TemplateInfo ti = new Zhubang.TemplateInfo()
                {
                    text = DoReplace(cells[0], info),
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
            return tis;
        }

        public static string DoReplace(string before, BiliInterfaceInfo info)
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
            after = after.Replace("{tag}", info.tag);
            after = after.Replace("{share}", info.share.ToString());
            after = after.Replace("{pic}", "{pic}" + info.AVNUM);

            return after;
        }

        public static Image resizeImage(Image imgToResize, Size size)
        {
            int destWidth = size.Width;
            int destHeight = size.Height;

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBilinear; //如果使用Bicubic会导致有白色边缘，感谢kite指导
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return (Image)b;
        }

        //https://danbystrom.se/2008/08/24/soft-edged-images-in-gdi/
        static public GraphicsPath createRoundRect(int x, int y, int width, int height, int radius)
        {
            GraphicsPath gp = new GraphicsPath();

            if (radius == 0)
                gp.AddRectangle(new Rectangle(x, y, width, height));
            else
            {
                radius *= 2;
                gp.AddLine(x + radius, y, x + width - radius, y);
                gp.AddArc(x + width - radius, y, radius, radius, 270, 90);
                gp.AddLine(x + width, y + radius, x + width, y + height - radius);
                gp.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90);
                gp.AddLine(x + width - radius, y + height, x + radius, y + height);
                gp.AddArc(x, y + height - radius, radius, radius, 90, 90);
                gp.AddLine(x, y + height - radius, x, y + radius);
                gp.AddArc(x, y, radius, radius, 180, 90);
                gp.CloseFigure();
            }
            return gp;
        }

        public enum ChannelARGB
        {
            Blue = 0,
            Green = 1,
            Red = 2,
            Alpha = 3
        }

        public static void transferOneARGBChannelFromOneBitmapToAnother(
            Bitmap source,
            Bitmap dest,
            ChannelARGB sourceChannel,
            ChannelARGB destChannel)
        {
            if (source.Size != dest.Size)
                throw new ArgumentException();
            Rectangle r = new Rectangle(Point.Empty, source.Size);
            BitmapData bdSrc = source.LockBits(r, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData bdDst = dest.LockBits(r, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* bpSrc = (byte*)bdSrc.Scan0.ToPointer();
                byte* bpDst = (byte*)bdDst.Scan0.ToPointer();
                bpSrc += (int)sourceChannel;
                bpDst += (int)destChannel;
                for (int i = r.Height * r.Width; i > 0; i--)
                {
                    *bpDst = *bpSrc;
                    bpSrc += 4;
                    bpDst += 4;
                }
            }
            source.UnlockBits(bdSrc);
            dest.UnlockBits(bdDst);
        }
    }
}
