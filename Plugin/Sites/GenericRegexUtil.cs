﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;

namespace OnlineVideos.Sites
{
    public class GenericRegexUtil : SiteUtilBase
    {
        [Category("OnlineVideosConfiguration"), Description("Regular Expression used to parse the baseUrl for dynamic categories. Group names: 'url', 'title'. Will not be used if not set.")]
        string dynamicCategoriesRegEx;
        [Category("OnlineVideosConfiguration"), Description("Regular Expression used to parse a html page for dynamic categories. Group names: 'url', 'title'. Will be used on the web pages resulting from the links from the dynamicCategoriesRegEx. Will not be used if not set.")]
        string dynamicSubCategoriesRegEx;
        [Category("OnlineVideosConfiguration"), Description("Regular Expression used to parse a html page for videos. Group names: 'VideoUrl', 'ImageUrl', 'Title', 'Duration', 'Description'.")]
        string videoListRegEx;
        [Category("OnlineVideosConfiguration"), Description("Format string applied to the result of the 'VideoUrl' match of the videoListRegEx.")]
        string videoUrlFormatString = "{0}";
        [Category("OnlineVideosConfiguration"), Description("Regular Expression used to parse a html page for a next page link. Group should be named 'url'.")]
        string nextPageRegEx;
        [Category("OnlineVideosConfiguration"), Description("Regular Expression used to parse a html page for a previous page link. Group should be named 'url'.")]
        string prevPageRegEx;
        [Category("OnlineVideosConfiguration"), Description("Regular Expression used to parse a html page for a link that points to another file holding the actual playback url. Group should be named 'url'. If this is not set, the fileUrlRegEx will be used directly, otherwise first this and afterwards the fileUrlRegEx on the result.")]
        string playlistUrlRegEx;
        [Category("OnlineVideosConfiguration"), Description("Regular Expression used to parse a html page for the playback url. Groups should be named 'm0', 'm1' and so on.")]
        string fileUrlRegEx;
        [Category("OnlineVideosConfiguration"), Description("Format string used used with the groups of the regex matches of the fileUrlRegEx to create the Url for playback.")]
        string fileUrlFormatString;
        [Category("OnlineVideosConfiguration"), Description("Format string used as Url for getting the results of a search. {0} will be replaced with the query.")]
        string searchUrl;
        [Category("OnlineVideosConfiguration"), Description("Format string that should be sent as post data for getting the results of a search. {0} will be replaced with the query. If this is not set, search will be executed normal as GET.")]
        string searchPostString;
        [Category("OnlineVideosConfiguration"), Description("Url used for prepending relative links.")]
        string baseUrl;
        [Category("OnlineVideosConfiguration"), Description("Cookies that need to be send with each request. Comma-separated list of name=value. Domain will be taken from the base url.")]
        string cookies;

        Regex regEx_dynamicCategories, regEx_dynamicSubCategories, regEx_VideoList, regEx_NextPage, regEx_PrevPage, regEx_PlaylistUrl, regEx_FileUrl;

        public override void Initialize(SiteSettings siteSettings)
        {
            base.Initialize(siteSettings);

            if (!string.IsNullOrEmpty(dynamicCategoriesRegEx)) regEx_dynamicCategories = new Regex(dynamicCategoriesRegEx, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);
            if (!string.IsNullOrEmpty(dynamicSubCategoriesRegEx)) regEx_dynamicSubCategories = new Regex(dynamicSubCategoriesRegEx, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);
            if (!string.IsNullOrEmpty(videoListRegEx)) regEx_VideoList = new Regex(videoListRegEx, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);
            if (!string.IsNullOrEmpty(nextPageRegEx)) regEx_NextPage = new Regex(nextPageRegEx, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);
            if (!string.IsNullOrEmpty(prevPageRegEx)) regEx_PrevPage = new Regex(prevPageRegEx, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);
            if (!string.IsNullOrEmpty(playlistUrlRegEx)) regEx_PlaylistUrl = new Regex(playlistUrlRegEx, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);
            if (!string.IsNullOrEmpty(fileUrlRegEx)) regEx_FileUrl = new Regex(fileUrlRegEx, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);
        }

        public override int DiscoverDynamicCategories()
        {
            if (regEx_dynamicCategories == null)
            {
                Settings.DynamicCategoriesDiscovered = true;
            }
            else
            {
                string data = GetWebData(baseUrl, GetCookie());
                if (!string.IsNullOrEmpty(data))
                {
                    Settings.Categories.Clear();
                    Match m = regEx_dynamicCategories.Match(data);
                    while (m.Success)
                    {
                        RssLink cat = new RssLink();
                        cat.Url = m.Groups["url"].Value;
                        if (!Uri.IsWellFormedUriString(cat.Url, System.UriKind.Absolute)) cat.Url = new Uri(new Uri(baseUrl), cat.Url).AbsoluteUri;
                        cat.Name = System.Web.HttpUtility.HtmlDecode(m.Groups["title"].Value.Trim());
                        if (regEx_dynamicSubCategories != null) cat.HasSubCategories = true;
                        Settings.Categories.Add(cat);
                        m = m.NextMatch();                        
                    }
                    Settings.DynamicCategoriesDiscovered = true;
                }
            }
            return Settings.Categories.Count;
        }

        public override int DiscoverSubCategories(Category parentCategory)
        {
            string data = GetWebData((parentCategory as RssLink).Url, GetCookie());
            if (!string.IsNullOrEmpty(data))
            {
                parentCategory.SubCategories = new List<Category>();
                Match m = regEx_dynamicSubCategories.Match(data);
                while (m.Success)
                {
                    RssLink cat = new RssLink();
                    cat.Url = m.Groups["url"].Value;
                    if (!Uri.IsWellFormedUriString(cat.Url, System.UriKind.Absolute)) cat.Url = new Uri(new Uri(baseUrl), cat.Url).AbsoluteUri;
                    cat.Name = HttpUtility.HtmlDecode(m.Groups["title"].Value.Trim());
                    cat.ParentCategory = parentCategory;
                    parentCategory.SubCategories.Add(cat);
                    m = m.NextMatch();
                }
                parentCategory.SubCategoriesDiscovered = true;
            }
            return parentCategory.SubCategories == null ? 0 : parentCategory.SubCategories.Count;
        }

        public override List<VideoInfo> getVideoList(Category category)
        {
            return Parse(((RssLink)category).Url, null);
        }

        public override string getUrl(VideoInfo video)
        {
            if (!Uri.IsWellFormedUriString(video.VideoUrl, System.UriKind.Absolute)) video.VideoUrl = new Uri(new Uri(baseUrl), video.VideoUrl).AbsoluteUri;
            if (regEx_PlaylistUrl != null || regEx_FileUrl != null)
            {
                string dataPage = GetWebData(video.VideoUrl, GetCookie());
                // extra step to get a playlist file if needed
                if (regEx_PlaylistUrl != null)
                {
                    Match mPlaylist = regEx_PlaylistUrl.Match(dataPage);
                    if (mPlaylist.Success)
                    {
                        dataPage = GetWebData(HttpUtility.UrlDecode(mPlaylist.Groups["url"].Value), GetCookie());
                    }
                    else return "";
                }
                if (regEx_FileUrl != null)
                {
                    Match m = regEx_FileUrl.Match(dataPage);
                    if (m.Success)
                    {
                        List<string> groupValues = new List<string>();
                        for (int i = 0; i < m.Groups.Count; i++) if (m.Groups["m" + i.ToString()].Success) groupValues.Add(System.Web.HttpUtility.UrlDecode(m.Groups["m" + i.ToString()].Value));
                        return string.Format(fileUrlFormatString, groupValues.ToArray());
                    }
                    else return "";
                }
            }
            return video.VideoUrl;
        }

        List<VideoInfo> Parse(string url, string data)
        {
            List<VideoInfo> videoList = new List<VideoInfo>();
            if (string.IsNullOrEmpty(data)) data = GetWebData(url, GetCookie());
            if (data.Length > 0)
            {
                Match m = regEx_VideoList.Match(data);
                while (m.Success)
                {
                    VideoInfo videoInfo = new VideoInfo();
                    videoInfo.Title = System.Web.HttpUtility.HtmlDecode(m.Groups["Title"].Value);
                    videoInfo.VideoUrl = string.Format(videoUrlFormatString, m.Groups["VideoUrl"].Value);
                    videoInfo.ImageUrl = m.Groups["ImageUrl"].Value;
                    videoInfo.Length = Regex.Replace(m.Groups["Duration"].Value, "(<[^>]+>)", "");
                    videoInfo.Description = m.Groups["Description"].Value;
                    videoList.Add(videoInfo);
                    m = m.NextMatch();
                }

                if (regEx_PrevPage != null)
                {
                    // check for previous page link
                    Match mPrev = regEx_PrevPage.Match(data);
                    if (mPrev.Success)
                    {
                        previousPageAvailable = true;
                        previousPageUrl = mPrev.Groups["url"].Value;

                        if (!Uri.IsWellFormedUriString(previousPageUrl, System.UriKind.Absolute))
                        {
                            Uri uri = null;
                            if (Uri.TryCreate(new Uri(url), previousPageUrl, out uri))
                            {
                                previousPageUrl = uri.ToString();
                            }
                            else
                            {
                                previousPageAvailable = false;
                                previousPageUrl = "";
                            }
                        }
                    }
                    else
                    {
                        previousPageAvailable = false;
                        previousPageUrl = "";
                    }
                }

                if (regEx_NextPage != null)
                {
                    // check for next page link
                    Match mNext = regEx_NextPage.Match(data);
                    if (mNext.Success)
                    {
                        nextPageAvailable = true;
                        nextPageUrl = mNext.Groups["url"].Value;

                        if (!Uri.IsWellFormedUriString(nextPageUrl, System.UriKind.Absolute))
                        {
                            Uri uri = null;
                            if (Uri.TryCreate(new Uri(url), nextPageUrl, out uri))
                            {
                                nextPageUrl = uri.ToString();
                            }
                            else
                            {
                                previousPageAvailable = false;
                                nextPageUrl = "";
                            }
                        }
                    }
                    else
                    {
                        nextPageAvailable = false;
                        nextPageUrl = "";
                    }
                }
            }

            return videoList;
        }

        #region Next/Previous Page
        
        string nextPageUrl = "";
        bool nextPageAvailable = false;
        public override bool HasNextPage
        {
            get { return nextPageAvailable; }
        }

        string previousPageUrl = "";
        bool previousPageAvailable = false;
        public override bool HasPreviousPage
        {
            get { return previousPageAvailable; }
        }

        public override List<VideoInfo> getNextPageVideos()
        {
            return Parse(nextPageUrl, null);
        }

        public override List<VideoInfo> getPreviousPageVideos()
        {
            return Parse(previousPageUrl, null);
        }

        #endregion

        #region Search

        public override bool CanSearch { get { return !string.IsNullOrEmpty(searchUrl); } }

        public override List<VideoInfo> Search(string query)
        {
            if (string.IsNullOrEmpty(searchPostString))
            {
                return Parse(string.Format(searchUrl, query), null);
            }
            else
            {
                return Parse(searchUrl, GetWebDataFromPost(searchUrl, string.Format(searchPostString, query)));
            }
        }

        #endregion

        CookieContainer GetCookie()
        {
            if (string.IsNullOrEmpty(cookies)) return null;

            CookieContainer cc = new CookieContainer();
            string[] myCookies = cookies.Split(',');
            foreach (string aCookie in myCookies)
            {
                string[] name_value = aCookie.Split('=');
                Cookie c = new Cookie();
                c.Name = name_value[0];
                c.Value = name_value[1];
                c.Expires = DateTime.Now.AddHours(1);
                c.Domain = new Uri(baseUrl).Host;
                cc.Add(c);
            }
            return cc;
        }
    }
}
