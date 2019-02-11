using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Passmo
{
    //------------------------------------------------------
    // Request
    //------------------------------------------------------
    // RawUrl...Temprary_listen/api/users
    // OriginalUrl.../api/users
    class Request
    {
        public string Method { get; private set; }
        public string RawUrl { get; private set; }
        public string OriginalUrl { get; private set; }
        public object Body { get; private set; }
        public Dictionary<string, string> Query { get; private set; }
        public Dictionary<string, string> Params = new Dictionary<string, string>();
        private HttpListenerRequest Context;
        private string RawBody;

        public Request(HttpListenerRequest req, string MountUrl)
        {
            this.Context = req;
            this.Method = req.HttpMethod;
            UrlParser urlParser = new UrlParser(MountUrl);
            UrlSpec urlSpec = urlParser.ParseUrl(this.Context);
            this.RawUrl = urlSpec.RawUrl;
            this.OriginalUrl = urlSpec.OriginalUrl;
            this.Query = urlSpec.Query;
            this.ReadInputStream();
        }

        private void ReadInputStream()
        {
            if (this.Context.HasEntityBody)
            {
                var body = this.Context.InputStream;
                var encoding = this.Context.ContentEncoding;
                var reader = new StreamReader(body, encoding);
                var s = reader.ReadToEnd();
                this.RawBody = s;
            }
        }

        public T ParseBody<T>()
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(this.RawBody));
            var serializer = new DataContractJsonSerializer(typeof(T));
            this.Body = (T)serializer.ReadObject(ms);
            return (T)Body;
        }

        //------------------------------------------------------
        // UrlParser
        //------------------------------------------------------
        class UrlParser
        {
            public string MountUrl;
            private UrlSpec UrlStatus;
            private Uri OrgUri;
            private HttpListenerRequest Request;
            
            public UrlParser(string mountUrl)
            {
                this.MountUrl = mountUrl;
            }

            public UrlSpec ParseUrl(HttpListenerRequest req)
            {
                this.OrgUri = req.Url;
                this.Request = req;
                this.UrlStatus = new UrlSpec();
                this.ParsePath();
                this.ParseQuery();
                return this.UrlStatus;
            }

            private void ParsePath()
            {
                this.UrlStatus.RawUrl = this.OrgUri.AbsolutePath;
                string[] list = this.OrgUri.AbsolutePath.Split(new string[] {this.MountUrl}, StringSplitOptions.None);
                this.UrlStatus.OriginalUrl = list[list.Length - 1];
            }

            private void ParseQuery()
            {
                string[] keys = this.Request.QueryString.AllKeys;
                foreach (string key in keys)
                {
                    this.UrlStatus.Query[key] = this.Request.QueryString.GetValues(key)[0];
                }
            }
        }

        //------------------------------------------------------
        // UrlSpec
        //------------------------------------------------------
        // http://localhost:80/temporary/users/insert?name=taro
        // rawurl: /temporary/users/insert
        // originalurl: /users/insert
        // params: hash{ name: taro }
        class UrlSpec
        {
            public string RawUrl;
            public string OriginalUrl;
            public Dictionary<string, string> Query = new Dictionary<string, string>();
        }
    }
}