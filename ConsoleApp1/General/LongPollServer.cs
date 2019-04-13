using System;
using xNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ConsoleApp1.General
{
    class LongPollServer
    {

        private static JObject getResponse(string str)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.UserAgent = Http.ChromeUserAgent();
            httpRequest.Cookies = new CookieDictionary();
            httpRequest.KeepAlive = true;
            return JObject.Parse(httpRequest.Get(str).ToString());
        }
        public static List<(string, int, int)> longPollResponse(string userId)
        {
            List<(string, int, int)> ans = new List<(string, int, int)>();
            JObject userJson = getResponse($"https://api.vk.com/method/users.getSubscriptions?user_id={userId}&v={Settings.v}&access_token={Settings.token}");
            // Console.WriteLine(response);
             Console.WriteLine("//Всего групп: "+ userJson["response"]["groups"]["count"].ToString());
            foreach (var group in userJson["response"]["groups"]["items"])
            {
                
                int likesCount = 0;
                int commentsCount = 0;
                JObject groupJson = getResponse($"https://api.vk.com/method/groups.getById?group_id={group}&v={Settings.v}&access_token={Settings.token}");
                JObject wallResponse = getResponse($"https://api.vk.com/method/wall.get?owner_id=-{group}&v={Settings.v}&access_token={Settings.token}");
                //Console.WriteLine(wallResponse);
                Console.WriteLine("//Анализ группы " + groupJson["response"][0]["name"].ToString());
                foreach (var post in wallResponse["response"]["items"])
                {
                    JObject likeResponse = getResponse($"https://api.vk.com/method/likes.getList?type=post&owner_id=-{group}&item_id={post["id"].ToString()}&v={Settings.v}&access_token={Settings.token}&count=1000");
                    //  Console.WriteLine(likeResponse);
                    int offset = 0;
                    
                    while (offset < likeResponse["response"]["count"].Value<Int64>())
                    {
                        foreach (var user in likeResponse["response"]["items"])
                            if (user.ToString() == userId)
                            {
                                ++likesCount;
                                //Console.WriteLine("YES");
                            }
                        offset += 1000;
                        likeResponse = getResponse($"https://api.vk.com/method/likes.getList?type=post&owner_id=-{group}&item_id={post["id"].ToString()}&v={Settings.v}&access_token={Settings.token}&count=1000&offset={offset.ToString()}");
                       // Console.WriteLine(likeResponse);

                    }
                    //-----
                    JObject commentResponse = getResponse($"https://api.vk.com/method/wall.getComments?&owner_id=-{group}&post_id={post["id"].ToString()}&v={Settings.v}&access_token={Settings.token}&count=100");
                    //  Console.WriteLine(commentResponse);
                    offset = 0;

                    while (offset < commentResponse["response"]["count"].Value<Int64>())
                    {
                        foreach (var comment in commentResponse["response"]["items"])
                        {
                            if (comment["from_id"].ToString() == userId)
                                ++commentsCount;
                          //  Console.WriteLine(comment["from_id"].ToString());
                        }
                        offset += 100;
                        commentResponse = getResponse($"https://api.vk.com/method/wall.getComments?&owner_id=-{group}&post_id={post["id"].ToString()}&v={Settings.v}&access_token={Settings.token}&count=100&offset={offset}");
                        // Console.WriteLine(commentResponse);



                    }
                    //-----
                    //Console.WriteLine(commentsCount);
                    //Console.ReadKey();
                }
                //Console.WriteLine(groupJson["response"][0]["name"].ToString() + " " + likesCount + " " + commentsCount);
                ans.Add((groupJson["response"][0]["name"].ToString(), likesCount, commentsCount));
            }
            ans.Sort( (x, y)=>(y.Item2 + y.Item3).CompareTo(x.Item2 + x.Item3) );
            return ans;
        }

    }
}
