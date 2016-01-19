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
        static private ManualResetEvent connected = new ManualResetEvent(false);

        static private string siteBase = @"C:\Users\otlg1\Documents\Projects\WebServer\www";
        static private long MAX_FILE_SIZE = (long)(1024 * 1024 * 1024 * 0.5); // 500Mb

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
                tcpListener.BeginAcceptSocket(ServeRequest, tcpListener);
                connected.WaitOne();
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
                    // TODO: bug? will we always receive the whole HTTP message? (not including chunked)
                    socket.Receive(buffer);

                    string request = System.Text.Encoding.UTF8.GetString(buffer);
                    HttpHeader header = HttpHeader.Parse(request);

                    Console.WriteLine("Serving request for " + header.RequestURI + " on thread " + Thread.CurrentThread.ManagedThreadId);

                    // keep alive?
                    // true by default, though might change if we decide to close it later
                    keepAlive = header.Headers.Connection != null ? header.Headers.Connection.KeepAlive : true;

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
                        response.Header.Headers.ContentType = "text/html";
                        response.Header.Headers.ContentLength = responseBody.Length;
                        response.Header.Headers.CacheControl.NoCache = true;
                        response.Header.Headers.CacheControl.MaxAge = 0;
                        response.Response = responseBody;

                        //response.Headers.Connection.Close = true;
                        //keepAlive = false;

                        byte[] responseBytes = response.GetResponseBytes();
                        socket.Send(responseBytes);

                    }
                    else
                    {
                        // try to resolve the URI
                        string uri = header.RequestURI;
                        uri = uri.Replace("/", @"\");
                        string fullPath = siteBase + uri;

                        // reponse
                        HttpResponse response = new HttpResponse();

                        // is it a file or folder?
                        if (File.Exists(fullPath))
                        {
                            // it's a file!
                            FileInfo finfo = new FileInfo(fullPath);
                            FileStream fstream = finfo.OpenRead();
                            long fsize = finfo.Length;
                            string fname = finfo.Name;
                            string extension = finfo.Extension.Length > 1 ? finfo.Extension.Substring(1) : "";   // remove preceding "." from extension
                            string mime = MimeMapping.GetMimeMapping(fname);

                            // is file size ok?
                            if (fsize <= MAX_FILE_SIZE)
                            {
                                // will the client accept this type?
                                double qval = header.Headers.Accept.ExtensionAccepted(extension);

                                // form response
                                response.ReturnCode = 200;

                                // what type of file?
                                response.Header.Headers.LastModified = finfo.LastWriteTimeUtc;
                                response.Header.Headers.ContentType = mime;
                                response.Header.Headers.ContentLength = fsize;
                                response.Header.Headers.CacheControl.NoCache = true;
                                response.Response = File.ReadAllBytes(fullPath);

                            } else
                            {
                                // form response
                                response.ReturnCode = 501;
                                response.Header.Headers.ContentType = "text/html";
                                response.Response = "<p>501 Error: File is too big to download...</p>";
                                response.Header.Headers.ContentLength = ((string)response.Response).Length;
                                response.Header.Headers.CacheControl.NoCache = true;
                            }

                            socket.Send(response.GetResponseBytes());
                        }
                        else if (Directory.Exists(fullPath))
                        {
                            // it's a directory
                            response.ReturnCode = 501;
                            response.Header.Headers.ContentType = "text/html";
                            response.Response = "<p>501 Error: Can't browse direcories yet...</p>";
                            response.Header.Headers.ContentLength = ((string)response.Response).Length;
                            response.Header.Headers.CacheControl.NoCache = true;

                            socket.Send(response.GetResponseBytes());
                        } else
                        {
                            response.ReturnCode = 404;
                            response.Header.Headers.ContentType = "text/html";
                            response.Response = "<p>404 Error: Couldn't find " + header.RequestURI + "</p>";
                            response.Header.Headers.ContentLength = ((string)response.Response).Length;
                            response.Header.Headers.CacheControl.NoCache = true;

                            socket.Send(response.GetResponseBytes());
                        }
                        

                       
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
