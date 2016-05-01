using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace BiliRanking.Web.Controllers
{
    [Route("api/[controller]")]
    public class RankController : Controller
    {
        // GET: api/values
        [HttpGet("list")]
        public List<BiliInterfaceInfo> List()
        {
            return BiliInterface.NowList;
        }

        [HttpGet("fuck")]
        public string Fuck()
        {
            BiliInterface.UpdateRanking();
            return "Oh! Fucking, coming!";
        }

        [HttpGet("status")]
        public string status()
        {
            return BiliInterface.UpdateStatus;
        }
    }
}
