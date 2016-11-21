using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliRanking.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var installed_fonts = new InstalledFontCollection();
            var families = installed_fonts.Families;
            var allFonts = new List<string>();
            foreach (FontFamily ff in families)
            {
                allFonts.Add(ff.Name);
            }
            allFonts.Sort();
            foreach (String s in allFonts)
            {
                Console.WriteLine(s);
            }
            Console.ReadKey();
        }
    }
}
