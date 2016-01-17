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
        static private TcpListener tcpListener;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            // how many threads in pool?
            ThreadPool.SetMinThreads(3, 3);

            // set up port
            tcpListener = new TcpListener(IPAddress.Loopback, port);
            tcpListener.ExclusiveAddressUse = false;
            tcpListener.Start();

            // start listening loop
            while (!quit)
            {
                //TcpClient socket = tcpListener.AcceptTcpClient();
                Socket socket = tcpListener.AcceptSocket();
                ThreadPool.QueueUserWorkItem(ServeRequest, socket);
            }
        }

        // serve request
        private static void ServeRequest(object context)
        {
            Socket socket = (Socket)context;
            //TcpClient socket = (TcpClient)context;

            try
            {

                //using (Stream inputStream = new BufferedStream(socket.GetStream()))
                //using (StreamWriter outputStream = new StreamWriter(new BufferedStream(socket.GetStream())))
                //{

                    byte[] buffer = new byte[1024];
                    //inputStream.Read(buffer, 0, buffer.Length);

                    // keep reading whilst kept alive
                    bool keepAlive = true;
                    while (socket.Connected)
                    {
                        //inputStream.Read(buffer, 0, buffer.Length);
                        socket.Receive(buffer);

                        string request = System.Text.Encoding.UTF8.GetString(buffer);
                        HTTPHeader header = HTTPHeaderParser.Parse(request);

                        // asked for what?
                        if (header.RequestURI == "/")
                        {
                            // form response
                            string responseBody = @"<!DOCTYPE html>
                <html><body>Hello Chrome!! The time is {0}
                </br>
                <img src=""image.jpg"">
            </body></html>";
                            responseBody = String.Format(responseBody, DateTime.Now.ToShortTimeString());

                            string responseString = "HTTP/1.1 200 OK\r\n";
                            responseString += "Content-Type: text/html\r\n";
                            responseString += "Content-Length: " + responseBody.Length.ToString() + "\r\n";
                            responseString += "Date: " + DateTime.Now.ToString("R") + "\r\n";
                            responseString += "\r\n";
                            responseString += responseBody;

                            byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(responseString);
                            socket.Send(responseBytes);
                            //outputStream.WriteLine(responseString);
                            //outputStream.Flush();

                        }
                        else if (header.RequestURI == "/image.jpg")
                        {
                            FileInfo finfo = new FileInfo("image.jpg");
                            FileStream fstream = finfo.OpenRead();
                            long fsize = finfo.Length;

                            string responseString = "HTTP/1.1 200 OK\r\n";
                            responseString += "Content-Type: image/jpeg\r\n";
                            responseString += "Content-Length: " + fsize + "\r\n";
                            responseString += "Date: " + DateTime.Now.ToString("R") + "\r\n";
                            responseString += "\r\n";

                            //outputStream.WriteLine(responseString);
                            byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(responseString);
                            socket.Send(responseBytes);
                            socket.SendFile("image.jpg");
                        }
                        else
                        {
                            string responseString = "HTTP/1.1 404 Not Found";
                            responseString += "\r\n\r\n";
                            responseString += "404 Error: Couldn't find " + header.RequestURI;
                            byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(responseString);
                            socket.Send(responseBytes);
                        }

                        // keep alive?
                        // true by default 
                        keepAlive = header.Headers.Connection.KeepAlive ?? true;

                    }

                socket.Close();

            }
            catch (Exception ex)
            { }
        }
    }
}
