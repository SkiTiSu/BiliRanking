using System;
using System.Collections.Generic;
using System.Text;

namespace BiliRanking.Core
{
    public static class Log
    {
        public static void Info(string s)
        {
            Console.WriteLine("[INFO] " + DateTime.Now.ToString("HH:mm:ss") + " " + s);
        }

        public static void Error(string s)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERRO] " + DateTime.Now.ToString("HH:mm:ss") + " " + s);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void Warn(string s)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[WARN] " + DateTime.Now.ToString("HH:mm:ss") + " " + s);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void Debug(string s)
        {
#if DEBUG
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[DEBU] " + DateTime.Now.ToString("HH:mm:ss") + " " + s);
            Console.ForegroundColor = ConsoleColor.Gray;
#endif
        }
    }
}
