using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using HtmlAgilityPack;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AvitoParser
{
    class Program
    {
        static void Main(string[] args)
        {
            WebHeaderCollection Header = new WebHeaderCollection();
            Header.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:56.0) Gecko/20100101 Firefox/56.0");
            Header.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            Header.Add(HttpRequestHeader.AcceptLanguage, "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
            //Header.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            Header.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
            Header.Add(HttpRequestHeader.Referer, "https://www.google.ru");

            WebQuery avito = new WebQuery();
            avito.Headers = Header;
            avito.GET("https://www.avito.ru");
            string kvartiry = avito.GET("https://www.avito.ru" + avito.ResponceUri.AbsolutePath + "/" + "kvartiry");
            HtmlDocument hiddenFieldsString = new HtmlDocument();
            hiddenFieldsString.LoadHtml(kvartiry);

            NameValueCollection filter = new NameValueCollection();
            string sField = hiddenFieldsString.DocumentNode.SelectSingleNode("//input[@name='s']").GetAttributeValue("value","101");
            string tokenValueField = hiddenFieldsString.DocumentNode.SelectSingleNode("//input[@class='js-token']").GetAttributeValue("value", "0");
            string tokenNameField = hiddenFieldsString.DocumentNode.SelectSingleNode("//input[@class='js-token']").GetAttributeValue("name", "token[0]");
            string sgtdField = hiddenFieldsString.DocumentNode.SelectSingleNode("//input[@name='sgtd']").GetAttributeValue("value", "");
            filter.Add("s", sField);
            //filter.Add(Ke)
            filter.Add(tokenNameField, tokenValueField);
            filter.Add("sgtd", sgtdField);
            filter.Add("category_id", "24");
            filter.Add("location_id", "637640");//"641780"
            filter.Add("name", "");
            filter.Add("params[201]", "");
            filter.Add("pmin", "");
            filter.Add("pmax", "");
            filter.Add("bt", "on");
            string filt = avito.GetPostBody(filter);
            string aprtments = avito.POST("https://www.avito.ru/search", filter);
            Avito av = new Avito();
            //av.SetFilter("Квартиры");
            av.Query.Headers = Header;
            string w=av.GetFilterParams(24);
            av.Filter.Add("category_id", "24");
            av.Filter.Add("location_id", "641780");
            av.Filter.Add("name", "");
            av.Filter.Add("params[201]", "");
            av.Filter.Add("pmin", "");
            av.Filter.Add("pmax", "");

            string filt1= av.Query.GetPostBody(av.Filter.Params);
            string aprtments1 = avito.POST("https://www.avito.ru/search", av.Filter.Params);
            HtmlDocument pages = new HtmlDocument();
            pages.LoadHtml(aprtments1);
            int pageNumber = 0;
            HtmlNodeCollection maxPageNumber = pages.DocumentNode.SelectNodes("//a[@class='pagination-page']");
            pageNumber = maxPageNumber.Count == 0 ? 1 :Convert.ToInt32( Regex.Match(maxPageNumber[maxPageNumber.Count - 1].GetAttributeValue("href", "1"), @"(?<=[\s\S]*?p=)[\s\S]+").Value);
            string p2 = avito.GET(avito.ResponceUri + "?p=2");
            
            //for (int i=1;i<=)
        }
    }

    //Класс для работы с сайтом Avito.ru
    class Avito
    {
        //
        public IFilter Filter;

        int lastPage;
        int currentPage;

        int location;



        public int GetPagesNumber()
        {
            string postHtml = Query.POST("https://www.avito.ru/search", Filter.Params);
            HtmlDocument pages = new HtmlDocument();
            pages.LoadHtml(postHtml);
            int pageNumber = 0;
            HtmlNodeCollection maxPageNumber = pages.DocumentNode.SelectNodes("//a[@class='pagination-page']");
            pageNumber = maxPageNumber.Count == 0 ? 1 : Convert.ToInt32(Regex.Match(maxPageNumber[maxPageNumber.Count - 1].GetAttributeValue("href", "1"), @"(?<=[\s\S]*?p=)[\s\S]+").Value);
            return pageNumber;
        }

        //Класс для создания запросов, позволяет сохранять куки и заголовки от запроса к запросу
        public WebQuery Query;

        public Avito()
        {
            Query = new WebQuery();
            Filter = new CommonFilter();

        }

        public string GetFilterParams(int Category)
        {
            string jsonParams = Query.GET("https://www.avito.ru/search/filters/list?_=2&category_id=" + Category + "&params[201]=1059&location_id=641780&currentPage=catalog&filtersGroup=catalog");
            var obj = JsonConvert.DeserializeObject(jsonParams);
            return jsonParams;
        }
        //public string Get

        //Общий фильтр задающий основные функции
        //Планируется позже создать для каждого запроса свой класс, но неуверен пока
         public class CommonFilter:IFilter
        {
            
            public NameValueCollection _params;
            public CommonFilter()
            {
                _params = new NameValueCollection();            
            }
            public void SetHiddenParams(string Html)
            {
                HtmlDocument hiddenFieldsString = new HtmlDocument();
                hiddenFieldsString.LoadHtml(Html);

                string sFieldValue = hiddenFieldsString.DocumentNode.SelectSingleNode("//input[@name='s']").GetAttributeValue("value", "101");
                _params.Add("s", sFieldValue);

                string sgtdFieldValue = hiddenFieldsString.DocumentNode.SelectSingleNode("//input[@name='sgtd']").GetAttributeValue("value", "");
                _params.Add("sgtd", sgtdFieldValue);
                    
                string tokenValueField = hiddenFieldsString.DocumentNode.SelectSingleNode("//input[@class='js-token']").GetAttributeValue("value", "0");
                string tokenNameField = hiddenFieldsString.DocumentNode.SelectSingleNode("//input[@class='js-token']").GetAttributeValue("name", "token[0]");
                _params.Add(tokenNameField, tokenValueField);
            }
            public void Add(string key,string value)
            {
                _params.Add(key,value);
            }
            public NameValueCollection Params
            {
                get { return _params; }
            }

        }
        //Интерфейс фильтра поиска Avito 
        public interface IFilter
        {
            void SetHiddenParams(string html);
            void Add(string key,string value);
            NameValueCollection Params { get; }
        }
    
        //public static string GetRegions()
        //{

        //}

        //Возвращает json представляющий список всех  доступных населенных пунктов 
        public static string GetCities()
        {
            WebQuery avito = new WebQuery();
            string regionsHtml = avito.GET("https://www.avito.ru/js/locations?_=bd26ca8");
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(regionsHtml);
            HtmlNodeCollection regionCollection = document.DocumentNode.SelectNodes("//select/option");
            List<AvitoRegion> regionsList = new List<AvitoRegion>();
            for (int i = 1; i < regionCollection.Count; i++)
            {
                regionsList.Add(new AvitoRegion(Convert.ToInt32(regionCollection[i].Attributes[0].Value), regionCollection[i].InnerText));
            }
            List<AvitoCity> Cities = new List<AvitoCity>();
            foreach (AvitoRegion reg in regionsList)
            {
                string jsonRegionCities = avito.GET("https://www.avito.ru/js/locations?json=true&id=" + reg.id + "&_=bd26ca8");
                AvitoCity regionCities = new AvitoCity();
                dynamic obj = JsonConvert.DeserializeObject(jsonRegionCities);
                foreach (var cityJson in obj)
                {
                    regionCities.id = Convert.ToInt32(cityJson["id"].Value);
                    regionCities.name = cityJson["name"].Value;
                    regionCities.parentId = Convert.ToInt32(cityJson["parentId"].Value);
                    regionCities.metroMap = cityJson["metroMap"].Value;
                    regionCities.namePrepositional = cityJson["namePrepositional"].Value;
                    Cities.Add(regionCities);
                }

            }
            string jsonAllCities = JsonConvert.SerializeObject(Cities);
            return jsonAllCities;
        }
        //Структура для десериализации списка регионов
        [Serializable]
        struct AvitoRegion
        {
            public int id;
            public string name;
            public AvitoRegion(int Id, string Name)
            {
                id = Id;
                name = Name;
            }
        }
        //Структура для десериализации городов
        [Serializable]
        struct AvitoCity
        {
            public int id;
            public string name;
            public int parentId;
            public bool metroMap;
            public string namePrepositional;



        }
    }
  



















    //Класс представлющий комбинацию HTTPWebREquest+HTTPWebResponce
    //В классе содержатся данные о заголовках запроса и ответа, а также данные куки
    //Можно установить прокси сервер
    public class WebQuery
    {
        //Запрещает/ разрешает автоматический редирект
        bool _allowAutoRedirect;
        public bool AllowAutoRedirect
        {
            get { return _allowAutoRedirect; }
            set { _allowAutoRedirect = value; }
        }
      
        //Возвращает код ответа
        int _responceStatus;
        public int ResponceStatusCode
        {
            get { return _responceStatus; }
        }

        //Устанавливает прокси
        IWebProxy _proxy;
        public IWebProxy Proxy
        {
            set { _proxy = value; }
        }

        //Адрес ресурса в ответе
        Uri _responceUri;
        public Uri ResponceUri
        {
            get { return _responceUri; }
        }

        //Заголовки запроса
        WebHeaderCollection _headers;
        public WebHeaderCollection Headers
        {
            get { return _headers; }
            set
            {
                foreach (string key in value)
                {
                    var val = value[key];
                    Headers.Add(key, val);
                }
            }
        }

        //Заголовки ответа
        WebHeaderCollection _responceHeaders;
        public WebHeaderCollection ResponceHeaders
        {
            get { return _responceHeaders; }
            private set { _responceHeaders = value; }
        }

        //Куки запроса
        CookieCollection _cookie;

        
        public WebQuery()
        {
            //Инициализация контейнеров
            _headers = new WebHeaderCollection();
            _responceHeaders = new WebHeaderCollection();
            _cookie = new CookieCollection();
            _allowAutoRedirect = true;// редирект разрешен
        }

        //Установка заголовкой запроса
        //Для некоторых заголовков имеются выделенные свойства, которые нельзя добавлять через Add
        private void AddHeaders(HttpWebRequest request, WebHeaderCollection headers)
        {
            foreach (string header in headers)
            {
                var value = headers[header];
                switch (header)
                {
                    case "User-Agent":
                        request.UserAgent = value;
                        break;
                    case "Accept":
                        request.Accept = value;
                        break;
                    case "Content-Type":
                        request.ContentType = value;
                        break;
                    case "Referer":
                        request.Referer = value;
                        break;
                    default:
                        request.Headers.Add(header, value);
                        break;
                }
            }
        }
        void AddCookies(HttpWebRequest request)
        {
            request.CookieContainer = new CookieContainer();
            foreach (Cookie c in _cookie)
            {
                request.CookieContainer.Add(c);
            }
        }
        void SetAllowAutoRedirect(ref HttpWebRequest request)
        {
            request.AllowAutoRedirect = _allowAutoRedirect;
        }
        void SetProxy(ref HttpWebRequest request)
        {
            if(_proxy!=null)          
                request.Proxy= (WebProxy)_proxy;
        }
        //Обобщенный метод GET/POST запроса
        string Request(string address, string Method, string body = "")
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(address);
            request.Method = Method;
            SetProxy(ref request);
            SetAllowAutoRedirect(ref request);


            AddHeaders(request, Headers);
            AddCookies(request);
            switch (Method)
            {
                case "GET":
                    break;
                case "POST":
                    //Формирование тела POST запроса
                    UTF8Encoding encoding = new UTF8Encoding();/////
                    byte[] bytePostData = encoding.GetBytes(body);
                    request.ContentLength = bytePostData.Length;
                    using (Stream postStream = request.GetRequestStream())
                    {
                        postStream.Write(bytePostData, 0, bytePostData.Length);
                    }
                    break;
            }
            HttpWebResponse responce = (HttpWebResponse)request.GetResponse();

            Stream dataStream = responce.GetResponseStream();
            string HtmlResponse;
            using (StreamReader reader = new StreamReader(dataStream))
            {
                HtmlResponse = reader.ReadToEnd();
            }
            _responceUri = responce.ResponseUri;
            _responceStatus = (int)responce.StatusCode;

            //Обновление куки
            CookieCollection bufCookies = new CookieCollection();
            if(responce.Cookies.Count!=0)
            {
                foreach (Cookie cookieResponce in responce.Cookies)
                {
                    bool isNewCookie = true;

                    foreach (Cookie cookieRequest in _cookie)
                    {
                        if (cookieRequest.Name == cookieResponce.Name)
                        {
                            isNewCookie = false;
                            bufCookies.Add(cookieResponce);
                            break;
                        }

                    }
                    if (isNewCookie)
                    {
                        bufCookies.Add(cookieResponce);
                    }
                }
                _cookie = bufCookies;
            }

            //Установка заголовка Cookie 
            if (responce.Headers.Get("Set-Cookie") != null)
            {
                if (Headers.Get("Cookie") != null)
                {
                    Headers.Set(HttpRequestHeader.Cookie, responce.Headers.Get("Set-Cookie"));
                }
                else
                {
                    Headers.Add(HttpRequestHeader.Cookie, responce.Headers.Get("Set-Cookie"));
                }
            }
            ResponceHeaders = responce.Headers;
            responce.Close();

            return HtmlResponse;
        }
        public string GET(string Url)
        {
            return Request(Url, "GET");
        }
        public string POST(string Url, string body)
        {
            return Request(Url, "POST", body);
        }
        public string POST(string Url, NameValueCollection body)
        {
            string strBody = GetPostBody(body);
            return Request(Url, strBody);
        }

        //Вовращает строку тела POST запроса
        public string GetPostBody(NameValueCollection data)
        {
            string result = "";
            foreach (string name in data.Keys)
            {
                result += name + "=" + data.Get(name) + "&";
            }
            result = result.Substring(0, result.Length - 1);
            return result;
        }
    }
}
