using BiliRanking.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
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

namespace BiliRanking.WPF.View
{
    /// <summary>
    /// viewFubang.xaml 的交互逻辑
    /// </summary>
    public partial class viewFubang : UserControl
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public viewFubang()
        {
            InitializeComponent();
            //imageBrushFubang2.ImageSource = Convert(BiliRanking.Core.Properties.Resources.fubang2);
            
        }

        private void textBoxFrom_TextChanged(object sender, TextChangedEventArgs e)
        {
            //int only 注意需要禁用IME否则会导致崩溃
            TextBox textBox = sender as TextBox;
            Int32 selectionStart = textBox.SelectionStart;
            Int32 selectionLength = textBox.SelectionLength;
            String newText = String.Empty;
            foreach (Char c in textBox.Text.ToCharArray())
            {
                if (Char.IsDigit(c) || Char.IsControl(c))
                {
                    newText += c;
                }
            }
            textBox.Text = newText;
            textBox.SelectionStart = selectionStart <= textBox.Text.Length ? selectionStart : textBox.Text.Length;
        }

        private void buttonGenFubang2_Click(object sender, RoutedEventArgs e)
        {
            List<BiliInterfaceInfo> linfo = new List<BiliInterfaceInfo>();
            int start = int.Parse(textBoxFrom.Text);

            foreach (BiliInterfaceInfo i in (List<BiliInterfaceInfo>)SharedData.Infos)
            {
                if (i.Fpaiming >= start)
                    linfo.Add(i);
            }

            //TODO: 再次排序
            Fubang fu = new Fubang();
            Task.Run(() => fu.Gen2(linfo));
        }
    }

    [ValueConversion(typeof(System.Drawing.Bitmap), typeof(ImageSource))]
    public class BitmapToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bmp = value as Bitmap;
            if (bmp == null)
                return null;
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        bmp.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
