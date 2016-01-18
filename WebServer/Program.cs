using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Web;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Http.Headers;
namespace WebServer
{
    static class Program
    {
        static private bool quit = false;
        static private int port = 1425;
        static private int SocketTimeOut = 0;
        static private TcpListener tcpListener;
        static private Socket socket;
        static private ManualResetEvent connected = new ManualResetEvent(false);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            // set up port
            tcpListener = new TcpListener(IPAddress.Loopback, port);
            tcpListener.ExclusiveAddressUse = false;
            tcpListener.Start();
            

            // start listening loop
            while (!quit)
            {
                //TcpClient socket = tcpListener.AcceptTcpClient();
                
                tcpListener.BeginAcceptSocket(ServeRequest, tcpListener);
                connected.WaitOne();
                //ThreadPool.QueueUserWorkItem(ServeRequest, socket);
            }
        }

        // serve request
        private static void ServeRequest(IAsyncResult ar)
        {
            Console.WriteLine("Starting to serve request on thread " + Thread.CurrentThread.ManagedThreadId);
            TcpListener listener = (TcpListener)ar.AsyncState;
            Socket socket = listener.EndAcceptSocket(ar);

            // allow main thread to pass
            connected.Set();
            connected.Reset();
            
            socket.ReceiveTimeout = SocketTimeOut;
            try
            {

                byte[] buffer = new byte[1024];
                //inputStream.Read(buffer, 0, buffer.Length);

                // keep reading whilst kept alive
                bool keepAlive = true;
                while (socket.Connected && keepAlive)
                {
                    //inputStream.Read(buffer, 0, buffer.Length);
                    socket.Receive(buffer);

                    string request = System.Text.Encoding.UTF8.GetString(buffer);
                    HttpHeader header = HTTPHeaderParser.Parse(request);

                    Console.WriteLine("Serving request for " + header.RequestURI + " on thread " + Thread.CurrentThread.ManagedThreadId);

                    // keep alive?
                    // true by default, though might change if we decide to close it later
                    keepAlive = header.Headers.Connection.KeepAlive ?? true;

                    // asked for what?
                    if (header.RequestURI == "/")
                    {
                        // form response
                        string responseBody = @"<!DOCTYPE html>
                        <html><body>Hello Chrome!! The time is {0}
                        </br>
                        <img src=""image.jpg"">
                        </body></html>";
                        responseBody = String.Format(responseBody, DateTime.Now.ToLongTimeString());

                        // form response
                        HttpResponse response = new HttpResponse();
                        response.ReturnCode = 200;
                        response.Headers.ContentType = "text/html";
                        response.Headers.ContentLength = responseBody.Length;
                        response.Headers.CacheControl.NoCache = true;
                        response.Headers.CacheControl.MaxAge = 0;
                        response.Response = responseBody;

                        //response.Headers.Connection.Close = true;
                        //keepAlive = false;

                        byte[] responseBytes = response.GetResponseBytes();
                        socket.Send(responseBytes);

                    }
                    else if (header.RequestURI == "/image.jpg")
                    {
                        FileInfo finfo = new FileInfo("image.jpg");
                        FileStream fstream = finfo.OpenRead();
                        long fsize = finfo.Length;

                        // form response
                        HttpResponse response = new HttpResponse();
                        response.ReturnCode = 200;
                        response.Headers.ContentType = "image/jpeg";
                        response.Headers.ContentLength = fsize;
                        response.Headers.CacheControl.NoCache = true;
                        response.Response = null;
                        response.AddExtraNewLine = true;
                             
                        
                        socket.Send(response.GetResponseBytes());
                        socket.SendFile("image.jpg");
                    }
                    else
                    {
                        HttpResponse response = new HttpResponse();
                        response.ReturnCode = 404;
                        response.Headers.ContentType = "text/html";
                        response.Response = "<p>404 Error: Couldn't find " + header.RequestURI + "</p>";
                        response.Headers.ContentLength = response.Response.Length;
                        response.Headers.CacheControl.NoCache = true;

                        socket.Send(response.GetResponseBytes());
                    }

                }

                socket.Close();

            }
            catch (Exception ex)
            { }

            Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId + " closing");
        }
    }
}
