using BiliRanking.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static BiliRanking.Core.Zhubang;

namespace BiliRanking.WPF.View
{
    /// <summary>
    /// ImageTemplateEditor.xaml 的交互逻辑
    /// </summary>
    public partial class ImageTemplateEditor : UserControl
    {
        public ImageTemplateEditor()
        {
            InitializeComponent();
        }

        private void buttenGen_Click(object sender, RoutedEventArgs e)
        {
            BiliInterfaceInfo bi = new BiliInterfaceInfo()
            {
                title = "【星尘二专收录曲】D!scolor【PV】",
                play = 61531,
                video_review = 683,
                coins = 2784,
                favorites = 5386,
                review = 806,
                share = 261,
                Fdefen = 390014,
                author = "星尘Official",
                created_at = "2017/05/26 10:12",
                aid = 10830407,
                tag = "",
                Fpaiming = 1
            };

            //text|font|ptSize|#colorcode|RightAlign TrueOrFalse|pointX|pointY|rectX|rectY|rectWidth|rectHeight|AutoOffsetY TrueOrFalse|MaxWidth

            List<TemplateInfo> tis = new List<TemplateInfo>();
            string text = textBoxInfo.Text;
            text = DoReplace(text, bi);
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
            Zhubang zb = new Zhubang();
            System.Drawing.Image re;
            if (bgimg == null)
            {
                re = zb.GenStardustTemplate(tis);
            }
            else
            {
                re = zb.GenWithTemplate(bgimg, tis);
            }
            imageMain.Source = ConvertDrawingImage2MediaImageSource(re);
        }

        public static System.Windows.Media.ImageSource ConvertDrawingImage2MediaImageSource(System.Drawing.Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                var bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();
                return bi;
            }
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

        System.Drawing.Image bgimg = null;

        private void buttonOpenImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            var imageExtensions = string.Join(";", System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders().Select(ici => ici.FilenameExtension));
            dlg.Filter = $"图片文件|{imageExtensions}|所有文件|*.*";
            if (dlg.ShowDialog() == true)
            {
                imageMain.Source = new BitmapImage(new Uri(dlg.FileName)); ;
                bgimg = System.Drawing.Image.FromFile(dlg.FileName);
            }
        }
    }
}
