using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Passmo
{
    //------------------------------------------------------
    // HttpServer
    //------------------------------------------------------
    class HttpServer {
        public string Static
        {
            get { return this.Router.StaticDirectory; }
            set { this.Router.StaticDirectory = value; }
        }
        public string DocumentRoot
        {
            get { return this.Router.DocumentRoot; }
            set { this.Router.DocumentRoot = value; }
        }
		private string RootUrl = "http://+:80";
		private string MountUrl = "/Temporary_Listen_Addresses";
        private Router Router = new Router();
        private static HttpServer Instance = new HttpServer();
        private HttpServer()
        {
            this.Router.DocumentRoot = Directory.GetCurrentDirectory();
            this.Router.StaticDirectory = "/public";
        }

        public static HttpServer GetInstance()
        {
            return Instance;
        }

        public void Start()
        {
            try
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add(this.RootUrl + this.MountUrl + "/");
                listener.Start();
                Console.WriteLine("Server Listening " + this.RootUrl + this.MountUrl + "/");
                while (true)
                {
                    HttpListenerContext context = listener.GetContext();
                    var req = new Request(context.Request, this.MountUrl);
                    var res = new Response(context.Response);
                    Console.WriteLine(req.RawUrl);
                    this.Router.Route(req, res);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Detect Error");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ReadKey();
            }

        }

        public void AddHandler(string url, Type handlerType)
        {
            this.Router.AddHandler(url, handlerType);
        }
    }

    //------------------------------------------------------
    // RequestHandler
    //------------------------------------------------------
    class RequestHandler
    {
        public virtual void Get(Request req, Response res) {}
        public virtual void Post(Request req, Response res) {}
        public string Url { get; protected set; }
    }
}
