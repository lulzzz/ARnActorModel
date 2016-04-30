﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Actor.Server
{

    class HttpContextComm : IContextComm
    {
        private HttpListenerContext fContext;
        public HttpContextComm(HttpListenerContext context)
        {
            fContext = context;
        }

        public Stream ReceiveStream()
        {
            return fContext.Request.InputStream;
        }

        public void Acknowledge()
        {
            HttpListenerResponse response = fContext.Response;
            response.Close();
        }

        public void SendStream(string uri, Stream stream)
        {
            using (StreamReader srDebug = new StreamReader(stream))
            {
                while (!srDebug.EndOfStream)
                    Debug.Print(srDebug.ReadLine());

                stream.Seek(0, SeekOrigin.Begin);
                using (var client = new HttpClient())
                {
                    using (var hc = new StreamContent(stream))
                    {
                        Uri lUri = new Uri(uri);
                        client.PostAsync(uri, hc).Wait();
                    }
                }
            }
        }

    }
}