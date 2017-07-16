using BiliRanking.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
    /// Download.xaml 的交互逻辑
    /// </summary>
    public partial class Download : UserControl
    {
        private static NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public Download()
        {
            InitializeComponent();
        }

        private void buttonGet_Click(object sender, RoutedEventArgs e)
        {
            var avs = SharedData.SortedAVs;
            string re = "";
            string renames = "";
            foreach (string item in avs)
            {
                var urls = BiliInterface.GetAVFlvUrl(item, out var rename, SharedData.Infos.ToList());
                if (urls != null)
                {
                    foreach (string u in urls)
                    {
                        re += u + "\r\n";
                    }
                }
                if (rename != null)
                {
                    foreach (string renn in rename)
                    {
                        renames += renn + "\r\n";
                    }
                }
                re += "\r\n";
            }
            textBox.Text = re;
            textBoxRename.Text = renames;
        }

        private void buttonRename_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "flv文件|*.flv";
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == true)
            {
                log.Info("开始替换文件名");
                Dictionary<string, string> relist = new Dictionary<string, string>();
                var lines = Regex.Split(textBoxRename.Text, "\r\n|\r|\n");
                foreach (var line in lines)
                {
                    if (line != string.Empty)
                    {
                        var keyvalue = Regex.Split(line, @"\$///\$");
                        relist.Add(keyvalue[0], keyvalue[1]);
                    }
                }
                foreach (string file in dlg.FileNames)
                {
                    FileInfo fi = new FileInfo(file);
                    if (relist.ContainsKey(fi.Name))
                    {
                        fi.MoveTo(System.IO.Path.Combine(fi.DirectoryName, relist[fi.Name]));
                        log.Info($"{fi.Name} -> {relist[fi.Name]}");
                    }
                    else
                    {
                        log.Info($"未找到{fi.Name}对应的替换名");
                    }
                }
                log.Info("替换文件名结束");
            }
        }
    }
}
