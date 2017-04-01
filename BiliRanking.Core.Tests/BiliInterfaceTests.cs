using Microsoft.VisualStudio.TestTools.UnitTesting;
using BiliRanking.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BiliRanking.Core.Tests
{
    [TestClass()]
    public class BiliInterfaceTests
    {
        [TestMethod()]
        public void EnvironmentTest()
        {
            Console.WriteLine("【运行环境检查】");
            Console.WriteLine("逻辑处理器数量：" + Environment.ProcessorCount);
        }

        [TestMethod()]
        public void GetInfoAsyncTest()
        {
            BiliInterfaceInfo info = new BiliInterfaceInfo();
            info = AsyncHelper.RunSync(() => BiliInterface.GetInfoAsync("av2680512"));

            Assert.AreEqual(info.title, "【创刊号】哔哩哔哩月刊鬼畜排行榜#001");
        }

        [TestMethod()]
        public void GetFlvInfoTest()
        {
            BiliInterfaceInfo info = new BiliInterfaceInfo();
            //info = BiliInterface.GetFlvInfo("av3153761");
            //TestContext.WriteLine(info.flvurl);
            //Debug.WriteLine(info.flvurl);
            info = BiliInterface.GetFlvInfo("av9472850");
            Assert.IsTrue(info.flvurl.Contains(".flv"));
        }

        [TestMethod()]
        public void GetFlvUrlsTest()
        {
            var c = BiliInterface.GetFlvUrls(15657517);
            Assert.IsTrue(c.Count() > 0);
        }
    }
}