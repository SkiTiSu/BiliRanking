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
            BiliInterfaceInfo bi = BiliInterface.GetInfo(textBoxAv.Text);
            BiliInterface.GetFace(bi);
            BiliInterface.GetPic(bi);

            //text|font|ptSize|#colorcode|RightAlign TrueOrFalse|pointX|pointY|rectX|rectY|rectWidth|rectHeight|AutoOffsetY TrueOrFalse|MaxWidth

            List<TemplateInfo> tis = new List<TemplateInfo>();
            string text = textBoxInfo.Text;
            text = Fubang.DoReplace(text, bi);
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
                re = zb.GenWithTemplate((System.Drawing.Image)bgimg.Clone(), tis);
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
