using System;
using System.Collections.Generic;
using System.Drawing;

namespace BiliRanking
{
    public class Fubang
    {
        Font f = new Font("微软雅黑", 48);
        Brush b = new SolidBrush(Color.Black);

        Graphics g;
        Image image;

        public void Gen(List<BiliInterfaceInfo> infos)
        {
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
                Log.Error(info.AVNUM + " - 封面获取失败（已更改？），请在左侧窗格输入AV号尝试或手动获取！");
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

    }
}
