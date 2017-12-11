using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BiliRanking.Core
{
    public static class FileManager
    {
        public static string currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
    }
}
