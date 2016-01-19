using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    /// <summary>
    /// A structure representing the value of a Connection parameter in an HTTP request/response
    /// </summary>
    public class HttpConnectionHeader
    {
        public bool KeepAlive { get; set; }
        public bool Close { get; set; }


        /// <summary>
        /// Parse the Connection value from a http request
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static HttpConnectionHeader Parse(string line)
        {
            HttpConnectionHeader ret = new HttpConnectionHeader();

            // split by comma
            string[] parts = line.Split(',').Select(s => s.Trim()).ToArray();
            for (int i = 0; i < parts.Length; i++)
            {
                // split by equals if there is
                string[] bits = parts[i].Split('=');
                if (StrComp(bits[0], "keep-alive"))
                {
                    ret.KeepAlive = true;
                }
                else if (StrComp(bits[0], "close"))
                {
                    ret.Close = true;
                }
            }

            return ret;
        }

        public override string ToString()
        {
            List<string> bits = new List<string>();
            if (KeepAlive) bits.Add("keep-alive");
            if (Close) bits.Add("close");
            return string.Join(",", bits);
        }

        private static bool StrComp(string one, string two)
        {
            return one.Equals(two, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (obj is HttpConnectionHeader)
            {
                HttpConnectionHeader other = (HttpConnectionHeader)obj;
                return (KeepAlive == other.KeepAlive && Close == other.Close);
            }
            else {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + KeepAlive.GetHashCode();
            hash = (hash * 7) + Close.GetHashCode();
            return hash;
        }
    }
}
