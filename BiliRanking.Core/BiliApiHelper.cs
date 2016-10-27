using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BiliRanking.Core
{
    //感谢https://github.com/xiaoyaocz/BiliBili-UWP！
    public class BiliApiHelper
    {
        public const string _appSecret_Wp = "ba3a4e554e9a6e15dc4d1d70c2b154e3";//Wp
        public const string _appSecret_IOS = "8cb98205e9b2ad3669aad0fce12a4c13";//Ios
        public const string _appSecret_Android = "ea85624dfcf12d7cc7b2b3a94fac1f2c";//Android
        public const string _appSecret_DONTNOT = "2ad42749773c441109bdc0191257a664";//Android

        public const string _appKey = "422fd9d7289a1dd9";
        public const string _appKey_IOS = "4ebafd7c4951b366";
        public const string _appKey_Android = "c1b107428d337928";
        public const string _appkey_DONTNOT = "85eb6835b0a1034e";

        public static string access_key = "";

        public static string GetSign(string url)
        {
            string result;
            string str = url.Substring(url.IndexOf("?", 4) + 1);
            List<string> list = str.Split('&').ToList();
            list.Sort();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string str1 in list)
            {
                stringBuilder.Append((stringBuilder.Length > 0 ? "&" : string.Empty));
                stringBuilder.Append(str1);
            }
            stringBuilder.Append(_appSecret_Wp);
            using (var md5 = MD5.Create())
            {
                result = BitConverter.ToString(md5.ComputeHash(Encoding.ASCII.GetBytes(stringBuilder.ToString()))).Replace("-", "").ToLower();
            }
            return result;
        }

        public static async Task<string> LoginBilibili(string UserName, string Password)
        {
            try
            {
                //发送第一次请求，得到access_key
                WebClient wc = new WebClient();
                string url = "https://api.bilibili.com/login?appkey=422fd9d7289a1dd9&platform=wp&pwd=" + WebUtility.UrlEncode(Password) + "&type=json&userid=" + WebUtility.UrlEncode(UserName);
                //url += "&sign=" + BiliApiHelper.GetSign(url);

                string results = await wc.DownloadStringTaskAsync(new Uri(url));
                //Json解析及数据判断
                LoginModel model = new LoginModel();
                model = JsonConvert.DeserializeObject<LoginModel>(results);
                if (model.code == -627)
                {
                    return "登录失败，密码错误！";
                }
                if (model.code == -626)
                {
                    return "登录失败，账号不存在！";
                }
                if (model.code == -625)
                {
                    return "密码错误多次";
                }
                if (model.code == -628)
                {
                    return "未知错误";
                }
                if (model.code == -1)
                {
                    return "登录失败，程序注册失败！请联系作者！";
                }
                HttpClient hc = new HttpClient();
                if (model.code == 0)
                {
                    access_key = model.access_key;
                    return model.access_key;
                    /*
                    
                    Windows.Web.Http.HttpResponseMessage hr2 = await hc.GetAsync(new Uri("http://api.bilibili.com/login/sso?&access_key=" + model.access_key + "&appkey=422fd9d7289a1dd9&platform=wp"));
                    hr2.EnsureSuccessStatusCode();
                    StorageFolder folder = ApplicationData.Current.LocalFolder;
                    StorageFile file = await folder.CreateFileAsync("us.bili", CreationCollisionOption.OpenIfExists);
                    await FileIO.WriteTextAsync(file, model.access_key);
                    */
                }

                return "err";
                //看看存不存在Cookie
                //TODO: 读取cookie http://www.cnblogs.com/suger/p/3359146.html
                /*
                hb = new HttpBaseProtocolFilter();
                HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));

                List<string> ls = new List<string>();
                foreach (HttpCookie item in cookieCollection)
                {
                    ls.Add(item.Name);
                }
                if (ls.Contains("DedeUserID"))
                {
                    return "登录成功";
                }
                else
                {
                    return "登录失败";
                }
                */
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147012867)
                {
                    return "登录失败，检查你的网络连接！";
                }
                else
                {
                    return "登录发生错误";
                }

            }
        }

        public static long GetTimeSpen
        {
            get { return Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds); }
        }
    }
}
