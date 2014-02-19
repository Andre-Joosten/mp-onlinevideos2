﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using OnlineVideos.Hoster.Base;
using OnlineVideos.Sites;
using System.Net;

namespace OnlineVideos.Sites.DavidCalder
{
    public class Uploadc : HosterBase
    {
        public override string getHosterUrl()
        {
            return "uploadc.com";
        }

        public override string getVideoUrls(string url)
        {
            CookieContainer cc = new CookieContainer();
            string postData = String.Empty;
            string[] parts = url.Split(new[] { "/" }, StringSplitOptions.None);
            string Referer = "http://uploadc.com/player/6.6/jwplayer.flash.swf";

            string data = SiteUtilBase.GetWebData(url, cc);
            if (!string.IsNullOrEmpty(data))
            {
                Match match = Regex.Match(data, @"<input\stype=""hidden""\sname=""(?<name>[^""]+)""\svalue=""(?<value>[^""]+)"">");

                while (match.Success)
                {
                    if (!String.IsNullOrEmpty(postData))
                        postData += "&";
                    postData += match.Groups["name"].Value + "=" + match.Groups["value"].Value;
                    match = match.NextMatch();
                }

                data = SiteUtilBase.GetWebDataFromPost(url, postData, cc, url);
                System.Threading.Thread.Sleep(Convert.ToInt32(5) * 1001);

                Match n1 = Regex.Match(data, @"""sources""\s:\s\[\s*{\s*""file""\s:\s""(?<url>[^""]+)"",\s*""default""\s:\strue,\s*""label""\s:\s""720""\s*}\s*\]");
                if (n1.Success)

                    return SiteUtilBase.GetRedirectedUrl(n1.Groups["url"].Value, Referer);

                Match n = Regex.Match(data, @"s1.addVariable\('file','(?<url>[^']*)'\)");
                if (n.Success)

                    return SiteUtilBase.GetRedirectedUrl(n.Groups["url"].Value, Referer);
            }
            return String.Empty;
        }
    }

    public class Megavideoz : HosterBase
    {
        public override string getHosterUrl()
        {
            return "megavideoz.eu";
        }

        public override string getVideoUrls(string url)
        {
            string webData = SiteUtilBase.GetWebData(url);
            string tmp = GetSubString(webData, @"var normal_video_file = '", @"';");
            return SiteUtilBase.GetRedirectedUrl(tmp);
        }
    }

    public class Allmyvideos : HosterBase
    {
        public override string getHosterUrl()
        {
            return "allmyvideos.net";
        }

        public override string getVideoUrls(string url)
        {
            CookieContainer cc = new CookieContainer();
            string postData = String.Empty;
            string Referer = "http://allmyvideos.net/player/6.6/jwplayer.flash.swf";

            string data = SiteUtilBase.GetWebData(url, cc);
            if (!string.IsNullOrEmpty(data))
            {
                Match match = Regex.Match(data, @"<input\stype=""hidden""\sname=""(?<name>[^""]+)""\svalue=""(?<value>[^""]+)"">");

                while (match.Success)
                {
                    if (!String.IsNullOrEmpty(postData))
                        postData += "&";
                    postData += match.Groups["name"].Value + "=" + match.Groups["value"].Value;
                    match = match.NextMatch();
                }

                System.Threading.Thread.Sleep(Convert.ToInt32(5) * 1001);
                data = SiteUtilBase.GetWebDataFromPost(url, postData, cc, url);

                Match n = Regex.Match(data, @"""sources""\s:\s\[\s*{\s*""file""\s:\s""(?<url>[^""]+)"",\s*""default""\s:\strue,\s*""label""\s:\s""720""\s*}\s*\]");
                if (n.Success)

                    return SiteUtilBase.GetRedirectedUrl(n.Groups["url"].Value, Referer);
            }
            return String.Empty;
        }
    }

    public class Vidspot : HosterBase
    {
        public override string getHosterUrl()
        {
            return "vidspot.net";
        }

        public override string getVideoUrls(string url)
        {
            CookieContainer cc = new CookieContainer();
            string postData = String.Empty;
            string Referer = "http://vidspot.net/player/6.6/jwplayer.flash.swf";

            string data = SiteUtilBase.GetWebData(url, cc);
            if (!string.IsNullOrEmpty(data))
            {
                Match match = Regex.Match(data, @"<input\stype=""hidden""\sname=""(?<name>[^""]+)""\svalue=""(?<value>[^""]+)"">");

                while (match.Success)
                {
                    if (!String.IsNullOrEmpty(postData))
                        postData += "&";
                    postData += match.Groups["name"].Value + "=" + match.Groups["value"].Value;
                    match = match.NextMatch();
                }

                System.Threading.Thread.Sleep(Convert.ToInt32(5) * 1001);
                data = SiteUtilBase.GetWebDataFromPost(url, postData, cc, url);
                Match n = Regex.Match(data, @"""sources""\s:\s\[\s*{\s*""file""\s:\s""(?<url>[^""]+)"",\s*""default""\s:\strue,\s*""label""\s:\s""720""\s*}\s*\]");
                if (n.Success)

                    return SiteUtilBase.GetRedirectedUrl(n.Groups["url"].Value, Referer);

            }
            return String.Empty;
        }
    }

    public class Mightyupload : HosterBase
    {
        public override string getHosterUrl()
        {
            return "mightyupload.com";
        }

        public override string getVideoUrls(string url)
        {
            CookieContainer cc = new CookieContainer();
            string postData = String.Empty;
            string Referer = url;

            string data = SiteUtilBase.GetWebData(url, cc);
            if (!string.IsNullOrEmpty(data))
            {
                Match match = Regex.Match(data, @"<input\stype=""hidden""\sname=""(?<name>[^""]+)""\svalue=""(?<value>[^""]+)"">");

                while (match.Success)
                {
                    if (!String.IsNullOrEmpty(postData))
                        postData += "&";
                    postData += match.Groups["name"].Value + "=" + match.Groups["value"].Value;
                    match = match.NextMatch();
                }
                System.Threading.Thread.Sleep(Convert.ToInt32(5) * 1001);
                data = SiteUtilBase.GetWebDataFromPost(url, postData, cc, url);
                

                Match n = Regex.Match(data, @"<a\shref=""(?<url>[^""]+)"">Download\sthe\sfile</a>");
                if (n.Success)

                    return SiteUtilBase.GetRedirectedUrl(n.Groups["url"].Value, Referer);

            }
            return String.Empty;
        }
    }

    public class Vshare : HosterBase
    {
        public override string getHosterUrl()
        {
            return "vshare.eu";
        }

        public override string getVideoUrls(string url)
        {
            CookieContainer cc = new CookieContainer();
            string postData = String.Empty;
            string Referer = "http://vshare.eu/player/6.6/jwplayer.flash.swf";

            string data = SiteUtilBase.GetWebData(url, cc);
            if (!string.IsNullOrEmpty(data))
            {
                Match match = Regex.Match(data, @"<input\stype=""hidden""\sname=""(?<name>[^""]+)""\svalue=""(?<value>[^""]+)"">");

                while (match.Success)
                {
                    if (!String.IsNullOrEmpty(postData))
                        postData += "&";
                    postData += match.Groups["name"].Value + "=" + match.Groups["value"].Value;
                    match = match.NextMatch();
                }
                postData += "&referer=&method_free=Continue";
                System.Threading.Thread.Sleep(Convert.ToInt32(5) * 1001);
                data = SiteUtilBase.GetWebDataFromPost(url, postData, cc, url);
                
                Match n = Regex.Match(data, @"config:{file:'(?<url>[^']+)','provider':'http'}");
                if (n.Success)

                    return SiteUtilBase.GetRedirectedUrl(n.Groups["url"].Value, Referer);

            }
            return String.Empty;
        }
    }

    public class Played : HosterBase
    {
        public override string getHosterUrl()
        {
            return "played.to";
        }

        public override string getVideoUrls(string url)
        {
            CookieContainer cc = new CookieContainer();
            string postData = String.Empty;
            string Referer = "http://played.to/player/6.6/jwplayer.flash.swf";

            string data = SiteUtilBase.GetWebData(url, cc);
            if (!string.IsNullOrEmpty(data))
            {
                Match match = Regex.Match(data, @"<input\stype=""hidden""\sname=""(?<name>[^""]+)""\svalue=""(?<value>[^""]+)"">");

                while (match.Success)
                {
                    if (!String.IsNullOrEmpty(postData))
                        postData += "&";
                    postData += match.Groups["name"].Value + "=" + match.Groups["value"].Value;
                    match = match.NextMatch();
                }
                postData += "&imhuman=Continue+to+Video";
                System.Threading.Thread.Sleep(Convert.ToInt32(5) * 1001);
                data = SiteUtilBase.GetWebDataFromPost(url, postData, cc, url);
                
                Match n = Regex.Match(data, @"{\s*file:\s""(?<url>[^""]+)"",\s*image:");
                if (n.Success)

                    return SiteUtilBase.GetRedirectedUrl(n.Groups["url"].Value, Referer);

            }
            return String.Empty;
        }
    }

    public class Vodlocker : HosterBase
    {
        public override string getHosterUrl()
        {
            return "vodlocker.com";
        }

        public override string getVideoUrls(string url)
        {
            CookieContainer cc = new CookieContainer();
            string postData = String.Empty;
            string Referer = "http://vodlocker.com/player/6.6/jwplayer.flash.swf";

            string data = SiteUtilBase.GetWebData(url, cc);

            if (!string.IsNullOrEmpty(data))
            {
                Match match = Regex.Match(data, @"<input\stype=""hidden""\sname=""(?<name>[^""]+)""\svalue=""(?<value>[^""]+)"">");

                while (match.Success)
                {
                    if (!String.IsNullOrEmpty(postData))
                        postData += "&";
                    postData += match.Groups["name"].Value + "=" + match.Groups["value"].Value;
                    match = match.NextMatch();
                }

                postData = postData.Replace("op=search&op=download1&", "op=download1&usr_login=&");
                postData = postData.Insert(postData.IndexOf("&hash="), "&referer=" + Referer);
                postData += "&imhuman=Proceed+to+video";

                data = SiteUtilBase.GetWebDataFromPost(url, postData, cc, url);
                System.Threading.Thread.Sleep(Convert.ToInt32(5) * 1001);

                Match n = Regex.Match(data, @"file:\s""(?<url>[^""]+)""");
                if (n.Success)

                    return SiteUtilBase.GetRedirectedUrl(n.Groups["url"].Value, Referer);

            }
            return String.Empty;
        }
    }
}
