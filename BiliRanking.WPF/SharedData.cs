using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BiliRanking.WPF
{
    public static class SharedData
    {
        public static event EventHandler AVsChanged;

        private static string aVs = "";
        public static string AVs
        {
            get
            {
                return aVs;
            }
            set
            {
                if (value != aVs)
                {
                    aVs = value;
                    AVsChanged?.Invoke(null, EventArgs.Empty);
                }
            }
        }

        public static IEnumerable<string> SortedAVs
        {
            get => from s in Regex.Split((AVs != null) ? AVs : "" , "\r\n|\r|\n").ToList()
                   where s != ""
                   select s;
            set => AVs = String.Join("\r\n", value);
        }
    }
}
