using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
        public const string _appSecret_dl = "1c15888dc316e05a15fdd0a02ed6584f";

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
            stringBuilder.Append(_appSecret_IOS);
            using (var md5 = MD5.Create())
            {
                result = BitConverter.ToString(md5.ComputeHash(Encoding.ASCII.GetBytes(stringBuilder.ToString()))).Replace("-", "").ToLower();
            }
            return result;
        }

        public static async Task<string> PostResults(Uri url, string PostContent)
        {
            try
            {
                using (HttpClient hc = new HttpClient())
                {
                    hc.DefaultRequestHeaders.Referrer = new Uri("http://www.bilibili.com/");
                    var response = await hc.PostAsync(url, new StringContent(PostContent, Encoding.UTF8, "application/x-www-form-urlencoded"));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
        public static async Task<string> GetEncryptedPassword(string passWord)
        {
            string base64String = "";
            try
            {
                string url = "https://secure.bilibili.com/login?act=getkey&rnd=4928";
                //https://passport.bilibili.com/login?act=getkey&rnd=4928
                //string url = string.Format(" https://passport.bilibili.com/api/oauth2/getKey?appkey={0}&build=411005&mobi_app=android&platform=wp&ts={1}000", _appKey, GetTimeSpan);
                //url += "&sign=" + GetSign(url);

                string stringAsync = await PostResults(new Uri(url), "");
                JObject jObjects = JObject.Parse(stringAsync);
                string str = jObjects["hash"].ToString();
                string str1 = jObjects["key"].ToString();
                string str2 = string.Concat(str, passWord);
                string str3 = Regex.Match(str1, "BEGIN PUBLIC KEY-----(?<key>[\\s\\S]+)-----END PUBLIC KEY").Groups["key"].Value.Trim().Replace("\n", "");
                byte[] numArray = Convert.FromBase64String(str3);
                //AsymmetricKeyAlgorithmProvider asymmetricKeyAlgorithmProvider = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaPkcs1);
                //CryptographicKey cryptographicKey = asymmetricKeyAlgorithmProvider.ImportPublicKey(WindowsRuntimeBufferExtensions.AsBuffer(numArray), 0);
                //IBuffer buffer = CryptographicEngine.Encrypt(cryptographicKey, WindowsRuntimeBufferExtensions.AsBuffer(Encoding.UTF8.GetBytes(str2)), null);
                //base64String = Convert.ToBase64String(WindowsRuntimeBufferExtensions.ToArray(buffer));
                RSACryptoServiceProvider csp = new RSACryptoServiceProvider();

                var pubKey = csp.ExportParameters(false);

                //converting the public key into a string representation
                string pubKeyString = str3;
                {
                    //we need some buffer
                    var sw = new System.IO.StringWriter();
                    //we need a serializer
                    var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                    //serialize the key into the stream
                    xs.Serialize(sw, pubKey);
                    //get the string from the stream
                    pubKeyString = sw.ToString();
                }

                //converting it back
                {
                    //get a stream from the string
                    var sr = new System.IO.StringReader(pubKeyString);
                    //we need a deserializer
                    var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                    //get the object back from the stream
                    pubKey = (RSAParameters)xs.Deserialize(sr);
                }

                csp = new RSACryptoServiceProvider();
                csp.ImportParameters(pubKey);

                //apply pkcs#1.5 padding and encrypt our data 
                var bytesCypherText = csp.Encrypt(Encoding.UTF8.GetBytes(str2), false);

                //we might want a string representation of our cypher text... base64 will do
                var cypherText = Convert.ToBase64String(bytesCypherText);


                base64String = cypherText;
            }
            catch (Exception)
            {
                //throw;
                base64String = passWord;
            }
            return base64String;
        }
        public static async Task<string> LoginBilibili(string UserName, string Password)
        {
            try
            {
                //https://api.bilibili.com/login?appkey=422fd9d7289a1dd9&platform=wp&pwd=JPJclVQpH4jwouRcSnngNnuPEq1S1rizxVJjLTg%2FtdqkKOizeIjS4CeRZsQg4%2F500Oye7IP4gWXhCRfHT6pDrboBNNkYywcrAhbOPtdx35ETcPfbjXNGSxteVDXw9Xq1ng0pcP1burNnAYtNRSayEKC1jiugi1LKyWbXpYE6VaM%3D&type=json&userid=xiaoyaocz&sign=74e4c872ec7b9d83d3a8a714e7e3b4b3
                //发送第一次请求，得到access_key
                string url = "https://passport.bilibili.com/api/oauth2/login"; //+ WebUtility.UrlEncode(await GetEncryptedPassword(Password)) + " &type=json&userid=" + WebUtility.UrlEncode(UserName);
                //url += "&sign=" + GetSign(url);

                string content = string.Format("appkey={0}&build=411005&mobi_app=android&password={1}&platform=wp&ts={2}000&username={3}", _appKey, WebUtility.UrlEncode(await GetEncryptedPassword(Password)), GetTimeSpan, WebUtility.UrlEncode(UserName));
                content += "&sign=" + GetSign(content);
                string results = await PostResults(new Uri(url), content);
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

                if (model.code == 0)
                {
                    access_key = model.data.access_token;
                    //string urlgo = "http://api.bilibili.com/login/sso?gourl=http%3A%2F%2Fwww.bilibili.com&access_key=" + model.data.access_token + "&appkey=422fd9d7289a1dd9&platform=android&scale=xhdpi";
                    //urlgo += "&sign=" + GetSign(urlgo);
                    //try
                    //{
                    //    await WebClientClass.GetResults(new Uri(urlgo));
                    //}
                    //catch (Exception)
                    //{
                    //}
                    //SettingHelper.Set_Access_key(model.data.access_token);
                    return model.data.access_token;
                }
                return "失败";
                //看看存不存在Cookie
                //HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
                //HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));

                //List<string> ls = new List<string>();
                //foreach (HttpCookie item in cookieCollection)
                //{
                //    ls.Add(item.Name);
                //}
                //if (ls.Contains("DedeUserID"))
                //{
                //    return "登录成功";
                //}
                //else
                //{
                //    return "登录失败";
                //}
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

        public static long GetTimeSpan
        {
            get { return Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds); }
        }
    }
}
