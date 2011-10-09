﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace OnlineVideos.Sites.georgius
{
    public sealed class ApetitTvUtil : SiteUtilBase
    {
        #region Private fields

        private static String baseUrl = @"http://www.apetitonline.cz/apetit-tv";
        private static String videoBaseUrl = @"http://www.apetitonline.cz";

        private static String showEpisodesStart = @"<div class=""dalsi-videa-obsah"">";
        private static String showEpisodesEnd = @"<div id=""sidebar"">";
        private static String showEpisodeStart = @"<div class=""dalsi-video"">";
        private static String showEpisodeThumbRegex = @"<img src=""(?<showEpisodeThumbUrl>[^""]+)"" alt=""[^""]*"" width=""[^""]*"" />";
        private static String showEpisodeUrlAndTitleRegex = @"<a href=""(?<showEpisodeUrl>[^""]+)"">(?<showEpisodeTitle>[^<]+)</a>";

        private static String showEpisodeNextPageRegex = @"<dd class=""next""><a href=""(?<nextPageUrl>[^""]+)"">Další strana</a></dd>";

        private static String showVideoDescriptionRegex = @"&file=/apetit-tv/(?<showVideoDescription>[^&]+)";
        private static String showVideoUrlsRegex = @"<ref href=""(?<showVideoUrl>[^""]+)"" />";

        // the number of show episodes per page
        private static int pageSize = 28;

        private int currentStartIndex = 0;
        private Boolean hasNextPage = false;

        private List<VideoInfo> loadedEpisodes = new List<VideoInfo>();
        private String nextPageUrl = String.Empty;

        private RssLink currentCategory = new RssLink();

        #endregion

        #region Constructors

        public ApetitTvUtil()
            : base()
        {
        }

        #endregion

        #region Properties
        #endregion

        #region Methods

        public override void Initialize(SiteSettings siteSettings)
        {
            base.Initialize(siteSettings);
        }

        public override int DiscoverDynamicCategories()
        {
            int dynamicCategoriesCount = 1;

            this.Settings.Categories.Add(
                new RssLink()
                {
                    Name = "Apetit.tv",
                    Url = ApetitTvUtil.baseUrl
                });

            this.Settings.DynamicCategoriesDiscovered = true;
            return dynamicCategoriesCount;
        }

        private List<VideoInfo> GetPageVideos(String pageUrl)
        {
            List<VideoInfo> pageVideos = new List<VideoInfo>();

            if (!String.IsNullOrEmpty(pageUrl))
            {
                String baseWebData = SiteUtilBase.GetWebData(pageUrl, null, null, null, true);

                int index = baseWebData.IndexOf(ApetitTvUtil.showEpisodesStart);
                if (index > 0)
                {
                    baseWebData = baseWebData.Substring(index);

                    index = baseWebData.IndexOf(ApetitTvUtil.showEpisodesEnd);

                    if (index > 0)
                    {
                        baseWebData = baseWebData.Substring(0, index);

                        while (true)
                        {
                            index = baseWebData.IndexOf(ApetitTvUtil.showEpisodeStart);

                            if (index > 0)
                            {
                                baseWebData = baseWebData.Substring(index);

                                String showEpisodeUrl = String.Empty;
                                String showEpisodeTitle = String.Empty;
                                String showEpisodeThumb = String.Empty;

                                Match match = Regex.Match(baseWebData, ApetitTvUtil.showEpisodeThumbRegex);
                                if (match.Success)
                                {
                                    showEpisodeThumb = match.Groups["showEpisodeThumbUrl"].Value;
                                    baseWebData = baseWebData.Substring(match.Index + match.Length);
                                }

                                match = Regex.Match(baseWebData, ApetitTvUtil.showEpisodeUrlAndTitleRegex);
                                if (match.Success)
                                {
                                    showEpisodeUrl = HttpUtility.UrlDecode(match.Groups["showEpisodeUrl"].Value);
                                    showEpisodeTitle = HttpUtility.HtmlDecode(match.Groups["showEpisodeTitle"].Value);
                                    baseWebData = baseWebData.Substring(match.Index + match.Length);
                                }

                                if ((String.IsNullOrEmpty(showEpisodeUrl)) && (String.IsNullOrEmpty(showEpisodeThumb)) && (String.IsNullOrEmpty(showEpisodeTitle)))
                                {
                                    break;
                                }

                                VideoInfo videoInfo = new VideoInfo()
                                {
                                    ImageUrl = Utils.FormatAbsoluteUrl(showEpisodeThumb, ApetitTvUtil.baseUrl),
                                    Title = showEpisodeTitle,
                                    VideoUrl = Utils.FormatAbsoluteUrl(showEpisodeUrl, ApetitTvUtil.baseUrl)
                                };

                                pageVideos.Add(videoInfo);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    Match nextPageMatch = Regex.Match(baseWebData, ApetitTvUtil.showEpisodeNextPageRegex);
                    this.nextPageUrl = Utils.FormatAbsoluteUrl(nextPageMatch.Groups["nextPageUrl"].Value, ApetitTvUtil.baseUrl);
                }
            }

            return pageVideos;
        }

        private List<VideoInfo> GetVideoList(Category category, int videoCount)
        {
            hasNextPage = false;
            String baseWebData = String.Empty;
            RssLink parentCategory = (RssLink)category;
            List<VideoInfo> videoList = new List<VideoInfo>();

            if (parentCategory.Name != this.currentCategory.Name)
            {
                this.currentStartIndex = 0;
                this.nextPageUrl = parentCategory.Url;
                this.loadedEpisodes.Clear();
            }

            if ((parentCategory.Other != null) && (this.currentCategory.Other != null))
            {
                String parentCategoryOther = (String)parentCategory.Other;
                String currentCategoryOther = (String)currentCategory.Other;

                if (parentCategoryOther != currentCategoryOther)
                {
                    this.currentStartIndex = 0;
                    this.nextPageUrl = parentCategory.Url;
                    this.loadedEpisodes.Clear();
                }
            }

            this.currentCategory = parentCategory;
            int addedVideos = 0;

            while (true)
            {
                while (((this.currentStartIndex + addedVideos) < this.loadedEpisodes.Count()) && (addedVideos < videoCount))
                {
                    videoList.Add(this.loadedEpisodes[this.currentStartIndex + addedVideos]);
                    addedVideos++;
                }

                if (addedVideos < videoCount)
                {
                    List<VideoInfo> loadedVideos = this.GetPageVideos(this.nextPageUrl);

                    if (loadedVideos.Count == 0)
                    {
                        break;
                    }
                    else
                    {
                        this.loadedEpisodes.AddRange(loadedVideos);
                    }
                }
                else
                {
                    break;
                }
            }

            if (((this.currentStartIndex + addedVideos) < this.loadedEpisodes.Count()) || (!String.IsNullOrEmpty(this.nextPageUrl)))
            {
                hasNextPage = true;
            }

            this.currentStartIndex += addedVideos;

            return videoList;
        }

        public override List<VideoInfo> getVideoList(Category category)
        {
            this.currentStartIndex = 0;
            return this.GetVideoList(category, ApetitTvUtil.pageSize - 2);
        }

        public override List<VideoInfo> getNextPageVideos()
        {
            return this.GetVideoList(this.currentCategory, ApetitTvUtil.pageSize);
        }

        public override bool HasNextPage
        {
            get
            {
                return this.hasNextPage;
            }
            protected set
            {
                this.hasNextPage = value;
            }
        }

        public override string getUrl(VideoInfo video)
        {
            String baseWebData = SiteUtilBase.GetWebData(video.VideoUrl, null, null, null, true);
            Match match = Regex.Match(baseWebData, ApetitTvUtil.showVideoDescriptionRegex);
            if (match.Success)
            {
                baseWebData = SiteUtilBase.GetWebData(Utils.FormatAbsoluteUrl(match.Groups["showVideoDescription"].Value, ApetitTvUtil.baseUrl), null, null, null, true);
                match = Regex.Match(baseWebData, ApetitTvUtil.showVideoUrlsRegex);
                if (match.Success)
                {
                    return Utils.FormatAbsoluteUrl(match.Groups["showVideoUrl"].Value, ApetitTvUtil.videoBaseUrl);
                }
            }

            return String.Empty;
        }

        #endregion
    }
}
