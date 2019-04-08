using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace ConsoleApp1.General
{
    class LongPollServer
    {
        public static string[] getLongPollServer()
        {
            var data = new HttpRequest();
            data.UserAgent = Http.ChromeUserAgent();
            data.Cookies = new CookieDictionary();
            data.KeepAlive = true;
            string respone = data.Get("https://api.vk.com/method/"
                + "messages.getLongPollServer" + "?"
                + "&" + "need_pts=" + 0
                + "&" + "lp_version=" + 3
                + "&" + "access_token=" + GlobalSettings.token
                + "&" + "v=" + GlobalSettings.version
                + "&" + "group_id=85591845"
                ).ToString();
            JObject json = JObject.Parse(respone);
            //Console.WriteLine(json.ToString());
            string[] Data =
            {
                json["response"]["server"].ToString(),
                json["response"]["key"].ToString(),
                json["response"]["ts"].ToString()

            };
            Console.WriteLine(Data[0] + " " + Data[1] +  " " +Data[2]);
            return Data;
        }

        public static string longPollResponse()
        {
            string[] Data = getLongPollServer();
            string server = Data[0];
            string key = Data[1];
            string ts = Data[2];
            Random rnd = new Random();
            HttpRequest data = new HttpRequest();
            while (true)
            {
                string response = data.Get($"https://{server}?act=a_check&key={key}&ts={ts}&wait=25&mode=2&version=2")
                    .ToString();
                JObject json = JObject.Parse(response);
               // Console.WriteLine(response);
                if (response.Contains("failed"))
                {
                    switch (Convert.ToInt32(json["failed"]))
                    {
                        case 1:
                            ts = json["ts"].ToString();
                            break;
                        case 2:
                        case 3:
                            Data = getLongPollServer();
                            server = Data[0];
                            key = Data[1];
                            ts = Data[2];
                            break;
                    }
                    continue;
                }
                ts = json["ts"].ToString();
                for (int i = 0; i < json["updates"].Count(); ++i)
                {
                    //Console.WriteLine(json["updates"][i].Count());
                    //обрабатываем сообщения от игроков
                    if (json["updates"][i][0].ToString() == "4")
                    {
                        string text = json["updates"][i][5].ToString();
                        string[] words = text.Split(new char[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        string id = json["updates"][i][3].ToString();
                        //если игрока нет в бд
                        //Logger.WriteCommand(id + "\t" + text);
                        if (!GlobalSettings.players.ContainsKey(id))
                        {
                            var sendError = new HttpRequest();
                            sendError.Cookies = new CookieDictionary();
                            sendError.KeepAlive = true;
                            sendError.UserAgent = Http.ChromeUserAgent();
                            var reqErrorParams = new RequestParams();
                            reqErrorParams["message"] = "[System] Ошибка доступа: тебя нет в базе данных игроков";
                            reqErrorParams["peer_id"] = id;
                            reqErrorParams["access_token"] = GlobalSettings.token;
                            reqErrorParams["v"] = GlobalSettings.version;
                            reqErrorParams["random_id"] = rnd.Next();
                            string sendErrorResponse = sendError.Post("https://api.vk.com/method/"
                                  + "messages.send" + "?", reqErrorParams).ToString();

                            continue;
                        }
                        Player player = GlobalSettings.players[id];
                        // сообщение в общий чат
                        if (words.Length > 1 && (words[0].ToUpper() == "/ALL" || words[0].ToUpper() == "/A" || words[0].ToUpper() == "/А"))
                        {
                            GlobalSettings.players[id].mask++;
                            string ids = "";
                            foreach (var adresse in GlobalSettings.players)
                            {
                                // if (adresse.Key == id)
                                //  continue;
                                ids = ids + ", " + adresse.Key;
                            }
                            var send = new HttpRequest();
                            send.Cookies = new CookieDictionary();
                            send.KeepAlive = true;
                            send.UserAgent = Http.ChromeUserAgent();
                            var reqParams = new RequestParams();
                            reqParams["message"] = "[All] " + player.nick + ": " + text.Substring(words[0].Length + 1);
                            reqParams["user_ids"] = ids.Substring(1);
                            reqParams["access_token"] = GlobalSettings.token;
                            reqParams["v"] = GlobalSettings.version;
                            reqParams["random_id"] = rnd.Next();
                            string sendResponse = send.Post("https://api.vk.com/method/"
                                  + "messages.send" + "?", reqParams).ToString();


                        }
                        if (words.Length > 1 && (words[0].ToUpper() == "/FRIENDS" || words[0].ToUpper() == "/F" || words[0].ToUpper() == "/Ф"))
                        {
                            if (player.inFriends == 0 && player.prem < 5)
                            {
                                var send = new HttpRequest();
                                send.Cookies = new CookieDictionary();
                                send.KeepAlive = true;
                                send.UserAgent = Http.ChromeUserAgent();
                                var reqParams = new RequestParams();
                                reqParams["message"] = "[System] Ошибка: Недостаточно прав.";
                                reqParams["peer_id"] = id;
                                reqParams["access_token"] = GlobalSettings.token;
                                reqParams["v"] = GlobalSettings.version;
                                reqParams["random_id"] = rnd.Next();
                                string sendResponse = send.Post("https://api.vk.com/method/"
                                      + "messages.send" + "?", reqParams).ToString();
                            }
                            else
                            {
                                if (rnd.Next() % 100 > 65 )
                                    GlobalSettings.players[id].mask++;
                                string ids = "";
                                foreach (var adresse in GlobalSettings.players)
                                {
                                    if (adresse.Value.inFriends == 1 || adresse.Value.prem >= 5)
                                        ids = ids + ", " + adresse.Key;
                                }
                                var send = new HttpRequest();
                                send.Cookies = new CookieDictionary();
                                send.KeepAlive = true;
                                send.UserAgent = Http.ChromeUserAgent();
                                var reqParams = new RequestParams();
                                reqParams["message"] = "[Friends] " + player.nick + ": " + text.Substring(words[0].Length + 1);
                                reqParams["user_ids"] = ids.Substring(1);
                                reqParams["access_token"] = GlobalSettings.token;
                                reqParams["v"] = GlobalSettings.version;
                                reqParams["random_id"] = rnd.Next();
                                string sendResponse = send.Post("https://api.vk.com/method/"
                                      + "messages.send" + "?", reqParams).ToString();
                            }

                        }
                        else if (words.Length > 1 && (words[0].ToUpper() == "/CHANGENAME" || words[0].ToUpper() == "/CN"))
                        {
                            if (player.prem >= 1)
                            {
                                var nameErr = "";
                                string nameCheck = @"^[a-z0-9A-Z]{1,10}$";
                                if (words.Length < 2)
                                    nameErr = "Данный ник некорректен (необходимо от 1 до 10 латинских букв и/или цифр).";
                                else
                                if (!Regex.IsMatch(words[1], nameCheck, RegexOptions.IgnoreCase))
                                    nameErr = "Данный ник некорректен (необходимо от 1 до 10 латинских букв и/или цифр).";
                                else
                                    foreach (var adresse in GlobalSettings.players)
                                    {
                                        if (adresse.Value.nick.ToUpper() == words[1].ToUpper())
                                            nameErr = "Данный ник уже занят.";

                                    }
                                if (nameErr == "")
                                {
                                    player.nick = words[1];
                                    player.mask += 10;
                                    var send = new HttpRequest();
                                    send.Cookies = new CookieDictionary();
                                    send.KeepAlive = true;
                                    send.UserAgent = Http.ChromeUserAgent();
                                    var reqParams = new RequestParams();
                                    reqParams["message"] = "[System] Ник успешно изменен на " + player.nick;
                                    reqParams["peer_id"] = id;
                                    reqParams["access_token"] = GlobalSettings.token;
                                    reqParams["v"] = GlobalSettings.version;
                                    reqParams["random_id"] = rnd.Next();
                                    string sendErrorResponse = send.Post("https://api.vk.com/method/"
                                          + "messages.send" + "?", reqParams).ToString();
                                }
                                else
                                {
                                    var sendError = new HttpRequest();
                                    sendError.Cookies = new CookieDictionary();
                                    sendError.KeepAlive = true;
                                    sendError.UserAgent = Http.ChromeUserAgent();
                                    var reqErrorParams = new RequestParams();
                                    reqErrorParams["message"] = "[System] Ошибка: " + nameErr;
                                    reqErrorParams["peer_id"] = id;
                                    reqErrorParams["access_token"] = GlobalSettings.token;
                                    reqErrorParams["v"] = GlobalSettings.version;
                                    reqErrorParams["random_id"] = rnd.Next();
                                    string sendErrorResponse = sendError.Post("https://api.vk.com/method/"
                                          + "messages.send" + "?", reqErrorParams).ToString();
                                }
                            }
                            else
                            {
                                var sendError = new HttpRequest();
                                sendError.Cookies = new CookieDictionary();
                                sendError.KeepAlive = true;
                                sendError.UserAgent = Http.ChromeUserAgent();
                                var reqErrorParams = new RequestParams();
                                reqErrorParams["message"] = "[System] Ошибка доступа: недостаточно прав для этой команды.";
                                reqErrorParams["peer_id"] = id;
                                reqErrorParams["access_token"] = GlobalSettings.token;
                                reqErrorParams["v"] = GlobalSettings.version;
                                reqErrorParams["random_id"] = rnd.Next();
                                string sendErrorResponse = sendError.Post("https://api.vk.com/method/"
                                      + "messages.send" + "?", reqErrorParams).ToString();
                            }
                        }
                        else if (words.Length > 2 && (words[0].ToUpper() == "/P" || words[0].ToUpper() == "/PM" || words[0].ToUpper() == "/W"))
                        {
                            GlobalSettings.players[id].mask++;
                            string adrId = "-1";
                            string ids = "";
                            if (words.Length < 2)
                                continue;
                            foreach (var adresse in GlobalSettings.players)
                            {
                                if (adresse.Value.nick.ToUpper() == words[1].ToUpper()) adrId = adresse.Key;
                                if (adresse.Value.nick.ToUpper() == words[1].ToUpper()
                                    || adresse.Value.prem >= 10
                                    || adresse.Key == id)
                                    ids = ids + ", " + adresse.Key;
                            }
                            if (adrId == "-1")
                            {
                                var send = new HttpRequest();
                                send.Cookies = new CookieDictionary();
                                send.KeepAlive = true;
                                send.UserAgent = Http.ChromeUserAgent();
                                var reqParams = new RequestParams();
                                reqParams["message"] = "[System] Ошибка: Данный пользователь не найден.";
                                reqParams["peer_id"] = id;
                                reqParams["access_token"] = GlobalSettings.token;
                                reqParams["v"] = GlobalSettings.version;
                                reqParams["random_id"] = rnd.Next();
                                string sendResponse = send.Post("https://api.vk.com/method/"
                                      + "messages.send" + "?", reqParams).ToString();
                            }
                            else
                            {
                                var send = new HttpRequest();
                                send.Cookies = new CookieDictionary();
                                send.KeepAlive = true;
                                send.UserAgent = Http.ChromeUserAgent();
                                var reqParams = new RequestParams();
                                reqParams["message"] = "[Private] " + player.nick + "→" + GlobalSettings.players[adrId].nick + ": "
                                    + text.Trim().Substring(words[0].Length + words[1].Length + 1);
                                reqParams["user_ids"] = ids;
                                reqParams["access_token"] = GlobalSettings.token;
                                reqParams["v"] = GlobalSettings.version;
                                reqParams["random_id"] = rnd.Next();
                                string sendResponse = send.Post("https://api.vk.com/method/"
                                      + "messages.send" + "?", reqParams).ToString();
                            }

                        }
                        else if (words[0].ToUpper() == "/ME" || words[0].ToUpper() == "/INFO" || words[0].ToUpper() == "/HELP")
                        {
                            var send = new HttpRequest();
                            send.Cookies = new CookieDictionary();
                            send.KeepAlive = true;
                            send.UserAgent = Http.ChromeUserAgent();
                            var reqParams = new RequestParams();
                            reqParams["message"] = "Ник: " + player.nick + "\n" +
                                                   "Сегодня отправлено сообщений: " + player.mask + "\n" +
                                                   (player.inFriends == 1 ? @"Резидент чата ""Friends""" + "\n" : "") +
                                                   "Список команд: \n" +
                                                   "/all - сообщение в общий чат (/a) \n" +
                                                   "/p nick - личное сообщение пользователю nick (/p, /pm) \n" +
                                                   (player.inFriends == 1 ? @"/friends - сообщение в чат ""Friends"" (/f)" + "\n" : "") +
                                                   "/me - информация и подсказки (/help, /info) \n" +
                                                   "/list - список зарегистрированных пользователей \n" +
                                                   (player.prem >= 1 ? "/changename nick - смена ника на nick (/cn) \n" : "") +
                                                   (player.prem >= 5 ? "/invite nick - пригласить nick  в чат друзей (/i) \n" : "") +
                                                   (player.prem >= 5 ? "/kick nick - выгнать nick из чата друзей(/k) \n" : "") +
                                                   (player.prem >= 5 ? "/whoismyfriend - список пользователей чата друзей (/wimf) \n" : "") +
                                                   (player.prem >= 10 ? "/getstats - АДМИН показать статистику личины (/gs) \n" : "") +
                                                   (player.prem >= 11 ? "/clearstats - АДМИН сбросить статистику личины (/сs) \n" : "");
                            reqParams["peer_id"] = id;
                            reqParams["access_token"] = GlobalSettings.token;
                            reqParams["v"] = GlobalSettings.version;
                            reqParams["random_id"] = rnd.Next();
                            string sendResponse = send.Post("https://api.vk.com/method/"
                                  + "messages.send" + "?", reqParams).ToString();
                        }
                        else if (words[0].ToUpper() == "/LIST" || words[0].ToUpper() == "/WHO")
                        {
                            GlobalSettings.players[id].mask++;
                            var send = new HttpRequest();
                            send.Cookies = new CookieDictionary();
                            send.KeepAlive = true;
                            send.UserAgent = Http.ChromeUserAgent();
                            var reqParams = new RequestParams();
                            var ans = "Зарегистрированные пользователи: \n";
                            foreach (var user in GlobalSettings.players)
                            {
                                ans = ans + user.Value.nick + "\n";
                            }
                            reqParams["message"] = ans;
                            reqParams["peer_id"] = id;
                            reqParams["access_token"] = GlobalSettings.token;
                            reqParams["v"] = GlobalSettings.version;
                            reqParams["random_id"] = rnd.Next();
                            string sendResponse = send.Post("https://api.vk.com/method/"
                                  + "messages.send" + "?", reqParams).ToString();
                        }
                        else if (words[0].ToUpper() == "/WHOISMYFRIEND" || words[0].ToUpper() == "/WIMF")
                        {
                            if (player.inFriends == 0 && player.prem < 5)
                            {
                                var send = new HttpRequest();
                                send.Cookies = new CookieDictionary();
                                send.KeepAlive = true;
                                send.UserAgent = Http.ChromeUserAgent();
                                var reqParams = new RequestParams();
                                reqParams["message"] = "[System] Ошибка: Недостаточно прав.";
                                reqParams["peer_id"] = id;
                                reqParams["access_token"] = GlobalSettings.token;
                                reqParams["v"] = GlobalSettings.version;
                                reqParams["random_id"] = rnd.Next();
                                string sendResponse = send.Post("https://api.vk.com/method/"
                                      + "messages.send" + "?", reqParams).ToString();
                            }
                            else
                            {
                                var send = new HttpRequest();
                                if (rnd.Next() % 100 > 65)
                                    GlobalSettings.players[id].mask++;
                                send.Cookies = new CookieDictionary();
                                send.KeepAlive = true;
                                send.UserAgent = Http.ChromeUserAgent();
                                var reqParams = new RequestParams();
                                var ans = "Пользователи чата Friends: \n";
                                foreach (var user in GlobalSettings.players)
                                {
                                    if (user.Value.inFriends == 1)
                                        ans = ans + user.Value.nick + "\n";
                                }
                                reqParams["message"] = ans;
                                reqParams["peer_id"] = id;
                                reqParams["access_token"] = GlobalSettings.token;
                                reqParams["v"] = GlobalSettings.version;
                                reqParams["random_id"] = rnd.Next();
                                string sendResponse = send.Post("https://api.vk.com/method/"
                                      + "messages.send" + "?", reqParams).ToString();
                            }
                        }
                        else if (words[0].ToUpper() == "/GETSTATS" || words[0].ToUpper() == "/GS")
                        {
                            if (player.prem < 10)
                            {
                                var send = new HttpRequest();
                                send.Cookies = new CookieDictionary();
                                send.KeepAlive = true;
                                send.UserAgent = Http.ChromeUserAgent();
                                var reqParams = new RequestParams();
                                reqParams["message"] = "[System] Ошибка: Не лезь, блять!";
                                reqParams["peer_id"] = id;
                                reqParams["access_token"] = GlobalSettings.token;
                                reqParams["v"] = GlobalSettings.version;
                                reqParams["random_id"] = rnd.Next();
                                string sendResponse = send.Post("https://api.vk.com/method/"
                                      + "messages.send" + "?", reqParams).ToString();
                            }
                            else
                            {
                                var send = new HttpRequest();
                                send.Cookies = new CookieDictionary();
                                send.KeepAlive = true;
                                send.UserAgent = Http.ChromeUserAgent();
                                var reqParams = new RequestParams();
                                var ans = "Статистика: \n";
                                foreach (var user in GlobalSettings.players)
                                {
                                    var _c = Math.Max(0,(user.Value.mask - 6)) / 6;
                                    ans = ans + user.Value.name + "\t" + user.Value.nick + "\t" + user.Value.mask +
                                         "\t" + _c * (_c + 1) / 2 + "\n";
                                }
                                reqParams["message"] = ans;
                                reqParams["peer_id"] = id;
                                reqParams["access_token"] = GlobalSettings.token;
                                reqParams["v"] = GlobalSettings.version;
                                reqParams["random_id"] = rnd.Next();
                                string sendResponse = send.Post("https://api.vk.com/method/"
                                      + "messages.send" + "?", reqParams).ToString();
                            }
                        }
                        else if (words[0].ToUpper() == "/CLEARSTATS" || words[0].ToUpper() == "/CS")
                        {
                            if (player.prem < 11)
                            {
                                var send = new HttpRequest();
                                send.Cookies = new CookieDictionary();
                                send.KeepAlive = true;
                                send.UserAgent = Http.ChromeUserAgent();
                                var reqParams = new RequestParams();
                                reqParams["message"] = "[System] Ошибка: Не лезь, блять!";
                                reqParams["peer_id"] = id;
                                reqParams["access_token"] = GlobalSettings.token;
                                reqParams["v"] = GlobalSettings.version;
                                reqParams["random_id"] = rnd.Next();
                                string sendResponse = send.Post("https://api.vk.com/method/"
                                      + "messages.send" + "?", reqParams).ToString();
                            }
                            else
                            {
                                var send = new HttpRequest();
                                send.Cookies = new CookieDictionary();
                                send.KeepAlive = true;
                                send.UserAgent = Http.ChromeUserAgent();
                                var reqParams = new RequestParams();
                                var ans = "Статистика очищена: \n";
                                foreach (var user in GlobalSettings.players)
                                {
                                    var _c = Math.Max(0, (user.Value.mask - 6)) / 6;
                                    ans = ans + user.Value.name + "\t" + user.Value.nick + "\t" + user.Value.mask +
                                         "\t" + _c * (_c + 1) / 2 + "\n";
                                    user.Value.mask = 0;
                                }
                                reqParams["message"] = ans;
                                reqParams["peer_id"] = id;
                                reqParams["access_token"] = GlobalSettings.token;
                                reqParams["v"] = GlobalSettings.version;
                                reqParams["random_id"] = rnd.Next();
                                string sendResponse = send.Post("https://api.vk.com/method/"
                                      + "messages.send" + "?", reqParams).ToString();
                            }
                        }
                        else if (words.Length > 1 && (words[0].ToUpper() == "/INVITE" || words[0].ToUpper() == "/I"))
                        {
                            if (player.prem < 5)
                            {
                                var send = new HttpRequest();
                                send.Cookies = new CookieDictionary();
                                send.KeepAlive = true;
                                send.UserAgent = Http.ChromeUserAgent();
                                var reqParams = new RequestParams();
                                reqParams["message"] = "[System] Ошибка: Недостаточно прав.";
                                reqParams["peer_id"] = id;
                                reqParams["access_token"] = GlobalSettings.token;
                                reqParams["v"] = GlobalSettings.version;
                                reqParams["random_id"] = rnd.Next();
                                string sendResponse = send.Post("https://api.vk.com/method/"
                                      + "messages.send" + "?", reqParams).ToString();
                            }
                            else
                            {
                                GlobalSettings.players[id].mask += 3;
                                string adrId = "-1";
                                string ids = "";
                                foreach (var adresse in GlobalSettings.players)
                                {
                                    if (adresse.Value.nick.ToUpper() == words[1].ToUpper()) adrId = adresse.Key;

                                }
                                if (adrId == "-1")
                                {
                                    var send = new HttpRequest();
                                    send.Cookies = new CookieDictionary();
                                    send.KeepAlive = true;
                                    send.UserAgent = Http.ChromeUserAgent();
                                    var reqParams = new RequestParams();
                                    reqParams["message"] = "[System] Ошибка: Данный пользователь не найден.";
                                    reqParams["peer_id"] = id;
                                    reqParams["access_token"] = GlobalSettings.token;
                                    reqParams["v"] = GlobalSettings.version;
                                    reqParams["random_id"] = rnd.Next();
                                    string sendResponse = send.Post("https://api.vk.com/method/"
                                          + "messages.send" + "?", reqParams).ToString();
                                }
                                else
                                {
                                    ids = "";
                                    foreach (var adresse in GlobalSettings.players)
                                    {
                                        if ((adresse.Value.inFriends == 1 || adresse.Value.prem >= 5) && adresse.Key != words[1])
                                            ids = ids + ", " + adresse.Key;
                                    }
                                    var send = new HttpRequest();
                                    send = new HttpRequest();
                                    send.Cookies = new CookieDictionary();
                                    send.KeepAlive = true;
                                    send.UserAgent = Http.ChromeUserAgent();
                                    var reqParams = new RequestParams();
                                    reqParams["message"] = "[Friends] " + GlobalSettings.players[adrId].nick + " был приглашен в чат.";
                                    reqParams["user_ids"] = ids.Substring(1);
                                    reqParams["access_token"] = GlobalSettings.token;
                                    reqParams["v"] = GlobalSettings.version;
                                    reqParams["random_id"] = rnd.Next();
                                    string sendResponse = send.Post("https://api.vk.com/method/"
                                          + "messages.send" + "?", reqParams).ToString();
                                    //---
                                    ids = "";
                                    GlobalSettings.players[adrId].inFriends = 1;
                                    send = new HttpRequest();
                                    send.Cookies = new CookieDictionary();
                                    send.KeepAlive = true;
                                    send.UserAgent = Http.ChromeUserAgent();
                                    reqParams = new RequestParams();
                                    reqParams["message"] = "[Friends] " + "Привет, новичок.\n" +
                                        "Тебе повезло, ты попал в наш уютный чатик Friends. " +
                                        "Это место, где ты можешь обсуждать новости и найти новых друзей. " +
                                        "Помни, главное поддерживать дружескую атмосферу, поэтому всегда используй дружелюбный тон, " +
                                        "никаких оскорблений и ругани. Нарушители будут исключены. \n" +
                                        "Используй /friends (/f) и /whoismyfriend  (/wimf) ! \n" +
                                        "P.S. помоги нам сделать Friends ламповым местечком:)";
                                    reqParams["peer_id"] = adrId;
                                    reqParams["access_token"] = GlobalSettings.token;
                                    reqParams["v"] = GlobalSettings.version;
                                    reqParams["random_id"] = rnd.Next();
                                    sendResponse = send.Post("https://api.vk.com/method/"
                                          + "messages.send" + "?", reqParams).ToString();
                                }
                            }

                        }
                        else if (words.Length > 1 && (words[0].ToUpper() == "/KICK" || words[0].ToUpper() == "/K"))
                        {
                            if (player.prem < 5)
                            {
                                var send = new HttpRequest();
                                send.Cookies = new CookieDictionary();
                                send.KeepAlive = true;
                                send.UserAgent = Http.ChromeUserAgent();
                                var reqParams = new RequestParams();
                                reqParams["message"] = "[System] Ошибка: Недостаточно прав.";
                                reqParams["peer_id"] = id;
                                reqParams["access_token"] = GlobalSettings.token;
                                reqParams["v"] = GlobalSettings.version;
                                reqParams["random_id"] = rnd.Next();
                                string sendResponse = send.Post("https://api.vk.com/method/"
                                      + "messages.send" + "?", reqParams).ToString();
                            }
                            else
                            {
                                GlobalSettings.players[id].mask += 3;
                                string adrId = "-1";
                                string ids = "";
                                foreach (var adresse in GlobalSettings.players)
                                {
                                    if (adresse.Value.nick.ToUpper() == words[1].ToUpper()) adrId = adresse.Key;
                                }
                                if (adrId == "-1")
                                {
                                    var send = new HttpRequest();
                                    send.Cookies = new CookieDictionary();
                                    send.KeepAlive = true;
                                    send.UserAgent = Http.ChromeUserAgent();
                                    var reqParams = new RequestParams();
                                    reqParams["message"] = "[System] Ошибка: Данный пользователь не найден.";
                                    reqParams["peer_id"] = id;
                                    reqParams["access_token"] = GlobalSettings.token;
                                    reqParams["v"] = GlobalSettings.version;
                                    reqParams["random_id"] = rnd.Next();
                                    string sendResponse = send.Post("https://api.vk.com/method/"
                                          + "messages.send" + "?", reqParams).ToString();
                                }
                                else
                                {
                                    ids = "";
                                    foreach (var adresse in GlobalSettings.players)
                                    {
                                        if ((adresse.Value.inFriends == 1 || adresse.Value.prem >= 5) && adresse.Key != words[1])
                                            ids = ids + ", " + adresse.Key;
                                    }
                                    var send = new HttpRequest();
                                    send = new HttpRequest();
                                    send.Cookies = new CookieDictionary();
                                    send.KeepAlive = true;
                                    send.UserAgent = Http.ChromeUserAgent();
                                    var reqParams = new RequestParams();
                                    reqParams["message"] = "[Friends] " + GlobalSettings.players[adrId].nick + " был изгнан из чата.";
                                    reqParams["user_ids"] = ids.Substring(1);
                                    reqParams["access_token"] = GlobalSettings.token;
                                    reqParams["v"] = GlobalSettings.version;
                                    reqParams["random_id"] = rnd.Next();
                                    string sendResponse = send.Post("https://api.vk.com/method/"
                                          + "messages.send" + "?", reqParams).ToString();
                                    //---
                                    GlobalSettings.players[adrId].inFriends = 0;
                                    send = new HttpRequest();
                                    send.Cookies = new CookieDictionary();
                                    send.KeepAlive = true;
                                    send.UserAgent = Http.ChromeUserAgent();
                                    reqParams = new RequestParams();
                                    reqParams["message"] = "[Friends] " + "Для тебя больше нет места в этом чате. Пока-пока!";
                                    reqParams["peer_ids"] = adrId;
                                    reqParams["access_token"] = GlobalSettings.token;
                                    reqParams["v"] = GlobalSettings.version;
                                    reqParams["random_id"] = rnd.Next();
                                    sendResponse = send.Post("https://api.vk.com/method/"
                                          + "messages.send" + "?", reqParams).ToString();
                                }
                            }

                        }
                        else if (words[0].ToUpper() == "/SAVE")
                        {
                            if (player.prem < 11)
                            {
                                var send = new HttpRequest();
                                send.Cookies = new CookieDictionary();
                                send.KeepAlive = true;
                                send.UserAgent = Http.ChromeUserAgent();
                                var reqParams = new RequestParams();
                                reqParams["message"] = "[System] Ошибка: Недостаточно прав.";
                                reqParams["peer_id"] = id;
                                reqParams["access_token"] = GlobalSettings.token;
                                reqParams["v"] = GlobalSettings.version;
                                reqParams["random_id"] = rnd.Next();
                                string sendResponse = send.Post("https://api.vk.com/method/"
                                      + "messages.send" + "?", reqParams).ToString();
                            }
                            else
                            {
                                FileWorker.WriteDB();
                                var send = new HttpRequest();
                                send.Cookies = new CookieDictionary();
                                send.KeepAlive = true;
                                send.UserAgent = Http.ChromeUserAgent();
                                var reqParams = new RequestParams();
                                reqParams["message"] = "[System] Резервное сохранение базы данных произведено.";
                                reqParams["peer_id"] = id;
                                reqParams["access_token"] = GlobalSettings.token;
                                reqParams["v"] = GlobalSettings.version;
                                reqParams["random_id"] = rnd.Next();
                                string sendResponse = send.Post("https://api.vk.com/method/"
                                      + "messages.send" + "?", reqParams).ToString();
                            }
                        }
                        else if (words[0].ToUpper() == "/LOAD")
                        {
                            if (player.prem < 11)
                            {
                                var send = new HttpRequest();
                                send.Cookies = new CookieDictionary();
                                send.KeepAlive = true;
                                send.UserAgent = Http.ChromeUserAgent();
                                var reqParams = new RequestParams();
                                reqParams["message"] = "[System] Ошибка: Недостаточно прав.";
                                reqParams["peer_id"] = id;
                                reqParams["access_token"] = GlobalSettings.token;
                                reqParams["v"] = GlobalSettings.version;
                                reqParams["random_id"] = rnd.Next();
                                string sendResponse = send.Post("https://api.vk.com/method/"
                                      + "messages.send" + "?", reqParams).ToString();
                            }
                            else
                            {
                                FileWorker.ReadDB();
                                var send = new HttpRequest();
                                send.Cookies = new CookieDictionary();
                                send.KeepAlive = true;
                                send.UserAgent = Http.ChromeUserAgent();
                                var reqParams = new RequestParams();
                                reqParams["message"] = "[System] Резервная загрузка базы данных произведена.";
                                reqParams["peer_id"] = id;
                                reqParams["access_token"] = GlobalSettings.token;
                                reqParams["v"] = GlobalSettings.version;
                                reqParams["random_id"] = rnd.Next();
                                string sendResponse = send.Post("https://api.vk.com/method/"
                                      + "messages.send" + "?", reqParams).ToString();
                            }
                        }
                        else if (words[0].ToUpper() == "/EXIT")
                        {
                            if (player.prem >= 11)
                            {
                                return "";
                            }
                        }
                    }
                }
            }
        }
    }
}
