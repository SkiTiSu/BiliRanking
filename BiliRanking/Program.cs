using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace BiliRanking
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "鬼畜榜生成器";

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("[WELCOME] 欢迎使用四季天书制作的鬼畜榜生成器！请在群里反馈使用建议~");
            Console.WriteLine("[WELCOME] 所有状态都会显示在这里哦~千万不要把伦家关掉！");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
