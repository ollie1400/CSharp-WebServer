using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    public class HttpResponse
    {
        public int ReturnCode { get; set; }
        public object Response { get; set; }
        public HttpHeader Header { get; set; }

        /// <summary>
        /// Add an extra NewLine string at the end of the returned response header string (or byte array).  False by default.  ONLY added if Response == null
        /// </summary>
        public bool AddExtraNewLine { get; set; }

        public HttpResponse()
        {
            Header = new HttpHeader();
            Header.HTTPVersionString = "HTTP/1.1";
            ReturnCode = 200;
            AddExtraNewLine = false;
        }

        /// <summary>
        /// Make the response string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string ret = "";
            ret += Header.HTTPVersionString + " " + ReturnCode + Header.NewLine;
            
            // if we need to set anything in the header, do it before this.
            ret += Header.ToString();

            // what is the response?
            // if it's a string, add the new line separator and put it in now
            if (Response is string)
            {
                if (!String.IsNullOrEmpty(Response as string))
                {
                    ret += Header.NewLine + Response as string;
                }
                else if (AddExtraNewLine)
                {
                    ret += Header.NewLine;
                }
            }

            return ret;

        }

        public byte[] GetResponseBytes()
        {
            byte[] headerBytes;

            // if given a Response object and it is a byte[] add it on
            byte[] contentBytes = new byte[0];
            byte[] totalResponse = null;
            if (!(Response is string))
            {
                if (Response is byte[])
                {
                    contentBytes = (byte[])Response;
                }
                else
                {
                    throw new Exception("Unsupported Response object");
                }

                // get header with extra character
                headerBytes = Encoding.UTF8.GetBytes(ToString() + Header.NewLine);

                // combine
                totalResponse = new byte[headerBytes.Length + contentBytes.Length];
                Buffer.BlockCopy(headerBytes, 0, totalResponse, 0, headerBytes.Length);
                Buffer.BlockCopy(contentBytes, 0, totalResponse, headerBytes.Length, contentBytes.Length);
            }
            else
            {
                headerBytes = Encoding.UTF8.GetBytes(ToString());
                totalResponse = headerBytes;
            }

            return totalResponse;
        }
    }
}
