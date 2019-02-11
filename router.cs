using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Passmo
{
    //------------------------------------------------------
    // Router
    //------------------------------------------------------
    class Router
    {
		public string StaticDirectory = "";
        public string DocumentRoot = "";
        private Dictionary<string, Type> RequestHandlers = new Dictionary<string, Type>();
        private Request Request;
        private Response Response;
        public void AddHandler(string url, Type handlerType)
        {
            this.RequestHandlers[url] = handlerType;
        }

        public bool Route(Request req, Response res)
        {
            this.Request = req;
            this.Response = res;
            return !(!this.TryDynamicRouting() && !this.TryStaticRouting());
        }

        private bool TryDynamicRouting()
        {
            foreach (KeyValuePair<string, Type> pair in this.RequestHandlers)
            {
                if (!this.CanHandle(pair.Key))
                {
                    this.Request.Params.Clear();
                }
                else
                {
                    var handler = (RequestHandler)Activator.CreateInstance(pair.Value);
                    switch (this.Request.Method)
                    {
                        case "GET":
                        handler.Get(this.Request, this.Response);
                        break;

                        case "POST":
                        handler.Post(this.Request, this.Response);
                        break;
                    }
                    return true;
                }
            }
            return false;
        }

        private bool CanHandle(string path)
        {
            var routerUrlSegments = UrlSegmentSpliter.Split(path);
            var requestUrlSegments = UrlSegmentSpliter.Split(this.Request.OriginalUrl);
            for (var i = 0; i < routerUrlSegments.Count; i++)
            {
                if (requestUrlSegments.Count <= i) return false;
                var segment = new UrlSegment(routerUrlSegments[i]);
                var result = segment.IsMatch(requestUrlSegments[i]);
                if (!result.match) return false;
                if (result.key != "") this.Request.Params[result.key] = result.capture;
            }
            return true;
        }

        private bool TryStaticRouting()
        {
            string localUri = this.DocumentRoot + this.StaticDirectory + this.Request.OriginalUrl;
            if (File.Exists(localUri)) 
            {
                this.Response.SendFile(localUri);
                return true;
            }
            else
            {
                this.Response.SendStatus(404);
                return false;
            }
        }
    }

    //------------------------------------------------------
    // UrlSegmentSpliter
    //------------------------------------------------------
    class UrlSegmentSpliter
    {
        public static List<string> Split(string url)
        {
            var rawSegments = url.Split(new string[] {"/"}, StringSplitOptions.None);
            var segments = new List<string>();
            foreach (var segment in rawSegments)
            {
                if (segment != "")
                {
                    segments.Add(segment);
                }
            }
            return segments;
        }
    }

    //------------------------------------------------------
    // UrlSegment
    //------------------------------------------------------
    class UrlSegment
    {
        string Segment;
        public UrlSegment(string segment)
        {
            this.Segment = segment;
        }

        public UrlSegmentMatchResult IsMatch(string segment)
        {
            var result = new UrlSegmentMatchResult();
            result.match = false;
            result.key = "";
            result.capture = "";
            if (this.Segment == "*")
            {
                result.match = true;
            }
            var res = Regex.Match(this.Segment, "^:(?<param>[^\\(]+)");
            if (res.Groups["param"].Value != "")
            {
                var param = res.Groups["param"].Value;
                res = Regex.Match(this.Segment, "\\((?<regex>.+)\\)$");
                if (res.Groups["regex"].Value != "" && !Regex.IsMatch(segment, res.Groups["regex"].Value))
                {
                    // ex. this.Segment = ":param(\d+)" and segment = "hoge"
                    result.match = false;
                }
                else
                {
                    // ex. this.Segment = ":param(\d+)" and segment = "123"
                    // ex. this.Segment = ":param" and segment = "123"
                    result.match = true;
                    result.key = param;
                    result.capture = segment;
                }
            }
            else
            {
                // ex. this.Segment = "\d+" and segment = "123"
                if (Regex.IsMatch(segment, this.Segment))
                {
                    result.match = true;
                }
            }
            return result;
        }

        public struct UrlSegmentMatchResult
        {
            public bool match;
            public string key;
            public string capture;
        }
    }
}