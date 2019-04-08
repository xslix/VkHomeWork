using System;
using xNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApp1.General
{
    class LongPollServer
    {

        public static void longPollResponse(string userId)
        {
            HttpRequest userRequest = new HttpRequest();
            userRequest.UserAgent = Http.ChromeUserAgent();
            userRequest.Cookies = new CookieDictionary();
            userRequest.KeepAlive = true;
            string response = userRequest.Get($"https://api.vk.com/method/users.getSubscriptions?user_id={userId}&v={Settings.v}&access_token={Settings.token}")
                    .ToString();
            // Console.WriteLine(response);
            JObject userJson = JObject.Parse(response);
            string ids = "";
            foreach (var a in userJson["response"]["groups"]["items"])
            {
                ids += a.ToString() + ",";
            }
            ids = ids.Substring(0, ids.Length - 1);
            HttpRequest groupRequest = new HttpRequest();
            groupRequest.UserAgent = Http.ChromeUserAgent();
            groupRequest.Cookies = new CookieDictionary();
            groupRequest.KeepAlive = true;
            string _response = userRequest.Get($"https://api.vk.com/method/groups.getById?group_ids={ids}&v={Settings.v}&access_token={Settings.token}")
                    .ToString();

            JObject groupJson = JObject.Parse(_response);
            //Console.WriteLine(groupJson.ToString());
            Console.WriteLine();
            Console.WriteLine("Groups:");
            foreach (var a in groupJson["response"])
            {
                Console.WriteLine(a["name"].ToString());
            }
            Console.WriteLine();
        }

    }
}
