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
            info = BiliInterface.GetFlvInfo("av3153761");
            //TestContext.WriteLine(info.flvurl);
            //Debug.WriteLine(info.flvurl);

            Assert.IsTrue(info.flvurl.Contains(".flv"));
        }
    }
}