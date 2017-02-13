using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliRanking.Core
{
    public class BiliUser
    {
        public async Task<UserInfoModel> GetMyUserInfo()
        {
            if (IsLogin())
            {
                try
                {
                    string url = string.Format("http://account.bilibili.com/api/myinfo?access_key={0}&appkey={1}&platform=wp&type=json", BiliApiHelper.access_key, BiliApiHelper._appKey);
                    url += "&sign=" + BiliApiHelper.GetSign(url);
                    string results = await BiliInterface.GetHtmlAsync(url);
                    UserInfoModel model = JsonConvert.DeserializeObject<UserInfoModel>(results);
                    //AttentionList = model.attentions;
                    return model;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public bool IsLogin()
        {
            return true;
        }
    }
}
