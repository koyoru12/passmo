using System.Net;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Passmo
{
    //------------------------------------------------------
    // Response
    //------------------------------------------------------
    class Response
    {
        private HttpListenerResponse Context;
        public Response(HttpListenerResponse res)
        {
            this.Context = res;
        }

        public void SendStatus(int statusCode)
        {
            this.Context.StatusCode = statusCode;
            this.Context.Close();
        }

        public void Send(string message)
        {
            byte[] content = Encoding.UTF8.GetBytes(message);
            this.Context.StatusCode = 200;
            this.Context.OutputStream.Write(content, 0, content.Length);
            this.Context.Close();            
        }

        public void Send<T>(T dataObject)
        {
            var ms = new MemoryStream();
            var serializer = new DataContractJsonSerializer(dataObject.GetType());
            serializer.WriteObject(ms, dataObject);
            var json = Encoding.UTF8.GetString(ms.ToArray());
            this.Send(json);
        }

        public void SendFile(string url)
        {
            this.Context.StatusCode = 200;
            byte[] content = File.ReadAllBytes(url);
            this.Context.OutputStream.Write(content, 0, content.Length);
            this.Context.Close();            
        }
    }
}