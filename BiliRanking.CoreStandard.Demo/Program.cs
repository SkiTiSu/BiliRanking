using System;
using BiliRanking.CoreStandard;

namespace BiliRanking.CoreStandard.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            BiliInterface.GetTagSort(30, "SHARPKEY中文曲", order: "new");
            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
}