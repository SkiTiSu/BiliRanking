using System;
using System.Collections.Generic;
using System.Text;

namespace BiliRanking
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
            Console.WriteLine("[ERROR] " + DateTime.Now.ToString("HH:mm:ss") + " " + s);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void Warn(string s)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[WARNING] " + DateTime.Now.ToString("HH:mm:ss") + " " + s);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
