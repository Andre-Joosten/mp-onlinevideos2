﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Net;
using System.Collections.Specialized;

namespace OnlineVideos.Sites
{
    public class AtresplayerUtil : GenericSiteUtil
    {

        [Category("OnlineVideosUserConfiguration"), Description("Email de tu cuenta de Atresplayer")]
        string email = null;
        [Category("OnlineVideosUserConfiguration"), Description("Password de tu cuenta de Atresplayer")]
        string password = null;

        [Category("OnlineVideosConfiguration")]
        protected string categoriasRegEx;

        [Category("OnlineVideosConfiguration")]
        protected string menuAZRegEx;

        [Category("OnlineVideosConfiguration")]
        protected string videoIDRegEx;

        [Category("OnlineVideosConfiguration")]
        protected string nombreMenuAZ;

        [Category("OnlineVideosConfiguration")]
        protected string todosURL;

        private static CookieContainer cc = new CookieContainer();

        protected String PREESTRENO = "PREESTRENO (DE PAGO)";
        protected String REGISTRO = "REQUIERE REGISTRO (GRATUITO)";
        protected String PREMIUM = "PREMIUM (DE PAGO)";

        internal enum CategoryType
        {
            None,
            menuAZ,
            subMenuAZ,
            general
        }

        public override int DiscoverDynamicCategories()
        {
            foreach (Category c in Settings.Categories)
            {
                if (c.Name != nombreMenuAZ)
                {
                    c.Other = CategoryType.general;
                    c.HasSubCategories = true;
                }
                else
                {
                    c.Other = CategoryType.menuAZ;
                    c.HasSubCategories = true;
                }
            }
            Settings.DynamicCategoriesDiscovered = true;

            return Settings.Categories.Count;
        }

        public override int DiscoverSubCategories(Category parentCategory)
        {
            List<Category> subCategories = null;

            switch ((CategoryType)parentCategory.Other)
            {
                case CategoryType.general:
                    subCategories = DiscoverGeneral(parentCategory as RssLink);
                    break;
                case CategoryType.menuAZ:
                    subCategories = DiscoverMenuAZ(parentCategory as RssLink);
                    break;
                case CategoryType.subMenuAZ:
                    subCategories = DiscoverSubMenuAZ(parentCategory as RssLink);
                    break;
            }

            parentCategory.SubCategories = subCategories;
            parentCategory.SubCategoriesDiscovered = true;
            parentCategory.HasSubCategories = subCategories == null ? false : subCategories.Count > 0;

            return parentCategory.HasSubCategories ? subCategories.Count : 0;
        }

        internal List<Category> DiscoverMenuAZ(RssLink parentCategory)
        {
            List<Category> result = new List<Category>();
            String programsData = GetWebData(parentCategory.Url);
            regEx_dynamicSubCategories = new Regex(menuAZRegEx, defaultRegexOptions);
            Match match = regEx_dynamicSubCategories.Match(programsData);
            while (match.Success)
            {
                String name = match.Groups["title"].Value;
                String url = match.Groups["url"].Value;
                result.Add(CreateCategory(name, url, "", CategoryType.subMenuAZ, "", parentCategory));
                match = match.NextMatch();
            }
            return result;
        }

        internal List<Category> DiscoverSubMenuAZ(RssLink parentCategory)
        {
            List<Category> result = new List<Category>();
            String programsData = GetWebData(todosURL);
            JObject jsonProgramas = JObject.Parse("{\"a\":" + programsData + "}");
            JArray programas = (JArray)jsonProgramas["a"];
            int ignoreMe;
            for (int k = 0; k < programas.Count; k++)
            {
                JToken programa = programas[k];
                String letra = (String)programa.SelectToken("letra");
                if ("".Equals(parentCategory.Url) || (("?" + letra).ToUpper().Equals(parentCategory.Url.ToUpper()) || "?0".Equals(parentCategory.Url) && int.TryParse(letra, out ignoreMe)))
                {
                    String name = (String)programa.SelectToken("titulo");
                    String url = baseUrl + (String)programa.SelectToken("href") + "/carousel.json";
                    String thumbUrl = baseUrl + (String)programa.SelectToken("img");
                    result.Add(CreateCategory(name, url, thumbUrl, CategoryType.None, "", parentCategory));
                }
            }
            return result;
        }

        internal List<Category> DiscoverGeneral(RssLink parentCategory)
        {
            List<Category> result = new List<Category>();
            String programsData = GetWebData(parentCategory.Url);
            regEx_dynamicSubCategories = new Regex(categoriasRegEx, defaultRegexOptions);
            Match match = regEx_dynamicSubCategories.Match(programsData);
            while (match.Success)
            {
                String name = match.Groups["title"].Value;
                String url =  match.Groups["url"].Value + "/carousel.json";
                String thumbUrl = baseUrl + match.Groups["thumb"].Value;
                result.Add(CreateCategory(name, url, thumbUrl, CategoryType.None, "", parentCategory));
                match = match.NextMatch();
            }
            return result;
        }

        internal RssLink CreateCategory(String name, String url, String thumbUrl, CategoryType categoryType, String description, Category parentCategory)
        {
            RssLink category = new RssLink();

            category.Name = name;
            category.Url = url;
            category.Thumb = thumbUrl;
            category.Other = categoryType;
            category.Description = description;
            category.HasSubCategories = categoryType != CategoryType.None;
            category.SubCategoriesDiscovered = false;
            category.ParentCategory = parentCategory;

            return category;
        }

        public override List<VideoInfo> getVideoList(Category category)
        {
            RssLink parentCategory = category as RssLink;
            List<VideoInfo> videoList = new List<VideoInfo>();
            String data = GetWebData(parentCategory.Url);
            JObject jsonCapitulos = JObject.Parse("{\"a\":" + data + "}");
            JArray capitulos = (JArray)jsonCapitulos["a"];
            for (int k = 0; k < capitulos.Count; k++)
            {
                JToken capitulo = capitulos[k];
                String tipo = (String)capitulo.SelectToken("icono");
                if (tipo.Equals("preestreno"))
                {
                    tipo = PREESTRENO;
                }
                else if (tipo.Equals("user"))
                {
                    tipo = REGISTRO;
                }
                else if (tipo.Equals("premium"))
                {
                    tipo = PREMIUM;
                }
                VideoInfo video = new VideoInfo();
                video.Title = (String)capitulo.SelectToken("title") + (tipo.Equals("") ? "" : " - " + tipo);
                video.VideoUrl = (String)capitulo.SelectToken("hrefHtml");
                video.ImageUrl = baseUrl + (String)capitulo.SelectToken("srcImage");
                video.Description = tipo;
                video.Other = tipo;
                videoList.Add(video);

            }
            return videoList;
        }

        public override string getUrl(VideoInfo video)
        {
            if (REGISTRO.Equals(video.Other) || PREMIUM.Equals(video.Other) || PREESTRENO.Equals(video.Other))
            {
                login("https://servicios.atresplayer.com/j_spring_security_check", String.Format("j_username={0}&j_password={1}", HttpUtility.UrlEncode(email), HttpUtility.UrlEncode(password)));
            }
            String videoURL = "";
            String data = GetWebData(video.VideoUrl);
            regEx_dynamicSubCategories = new Regex(videoIDRegEx, defaultRegexOptions);
            Match match = regEx_dynamicSubCategories.Match(data);
            if (match.Success)
            {
                String videoID = match.Groups["videoID"].Value;
                String token = HttpUtility.UrlEncode(getToken(videoID, "puessepavuestramerced"));
                String auxURL = String.Format("https://servicios.atresplayer.com/api/urlVideo/{0}/{1}/{2}", videoID, "android_tablet", token);
                String datos = GetWebData(auxURL,cc);
                JObject datosJSON = JObject.Parse(datos);
                // {"result": 0, "resultDes": "OK", "resultObject": {"es": "http://urldelvideo"}}
                // {"result": 2,"resultDes": "El usuario no tiene permisos para ver el vídeo"}
                videoURL = (String)datosJSON["resultObject"]["es"];
            }
            return videoURL;
        }

        public String getToken(String videoID, String password)
        {
            long l = 3000L + getApiTime();
            String hash = getHash(videoID + l, password);
            return videoID + ("|" + l + "|" + hash).ToLower();
            
        }

        public long getApiTime()
        {
            String stime = GetWebData("http://servicios.atresplayer.com/api/admin/time");
            return long.Parse(stime) / 1000L;
        }

        public String getHash(String s, String password)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(password);
            HMACMD5 hmacmd5 = new HMACMD5(keyByte);
            byte[] messageBytes = encoding.GetBytes(s);
            byte[] hashmessage = hmacmd5.ComputeHash(messageBytes);
            string HMACMd5Value = ByteToString(hashmessage);
            return HMACMd5Value;
        }

        private static string ByteToString(byte[] buff)
        {
            string sbinary = "";
            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("X2");
            }
            return (sbinary);
        }

        public void login(string url, string postData)
        {
            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(password))
            {
                Log.Error("Atresplayer: Debes indicar tu email y password para ver los vídeos que requieren registro, los premium y los preestrenos");
                throw new OnlineVideosException("Debes indicar tu email y password para ver los vídeos que requieren registro, los premium y los preestrenos");
            }
            else
            {
                GetWebData(url, postData, null, cc, null, false, false, null, false);
                String datosCookie = GetWebData("https://servicios.atresplayer.com/connected", cc);
                Log.Debug("Atresplayer: Datos obtenidos del login: {0}", datosCookie);
                //{"iduser":0000000,"connected":true,"username":"xxxxxx","subscriber":false,"keywords":""} // login OK
                //{"connected": false} // Login error
                JObject datosCookieJSON = JObject.Parse(datosCookie);
                if ((bool)datosCookieJSON["connected"])
                {
                    DateTime expireTime = DateTime.Now.AddHours(3);
                    Cookie c1 = new Cookie();
                    c1.Name = "comunidad";
                    c1.Value = datosCookieJSON["username"].ToString();
                    c1.Path = "/";
                    c1.Domain = new Uri(url).Host;
                    c1.Expires = expireTime;
                    Cookie c2 = new Cookie();
                    c2.Name = "suscriber";
                    c2.Value = datosCookieJSON["subscriber"].ToString();
                    c2.Path = "/";
                    c2.Domain = "atresplayer.com";
                    c2.Expires = expireTime;
                    Cookie c3 = new Cookie();
                    c3.Name = "comunidadIdentifier";
                    c3.Value = datosCookieJSON["iduser"].ToString();
                    c3.Path = "/";
                    c3.Domain = "atresplayer.com";
                    c3.Expires = expireTime;
                    Cookie c4 = new Cookie();
                    c4.Name = "keyworsForTarget";
                    c4.Value = datosCookieJSON["keywords"].ToString();
                    c4.Path = "/";
                    c4.Domain = "atresplayer.com";
                    c4.Expires = expireTime;
                    cc.Add(c1);
                    cc.Add(c2);
                    cc.Add(c3);
                    cc.Add(c4);
                }
                else
                {
                    Log.Error("Atresplayer: Datos de login erróneos, asegúrate de haberlos introducido correctamente");
                    throw new OnlineVideosException("Datos de login erróneos, asegúrate de haberlos introducido correctamente");
                }
            }
        }

    }
}
