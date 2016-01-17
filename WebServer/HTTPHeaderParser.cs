using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    public abstract class HTTPHeaderParser
    {
        public static HTTPHeader Parse(string header)
        {
            HTTPHeader ret = new HTTPHeader();

            // split into lines
            string[] lines = header.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            // request line is first
            string[] parts = lines[0].Split(' ');
            ret.Method = parts[0];
            ret.RequestURI = parts[1];
            ret.HTTPVersionString = parts[2];

            // go through the rest of the strings and get data from them
            // other header fields are in 5.3 of https://www.w3.org/Protocols/rfc2616/rfc2616-sec5.html
            for (int i = 1; i < lines.Length; i++)
            {
                // if the line doen't contain a colon then finish
                if (lines[i].IndexOf(':') < 0) break;

                // get header field name and value
                parts = lines[i].Split(':');
                string fieldName = parts[0];
                string value = parts[1].Trim();

                // add into the dictionary
                ret.HeaderFields.Add(fieldName, value);

                // what is it? (case-insensitive https://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2)
                if (fieldName.Equals("Accept", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.Accept = value.Split(',').Select(s => s.Trim()).ToArray();
                }
                else if (fieldName.Equals("Accept-Charset", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.AcceptCharset = value.Split(',').Select(s => s.Trim()).ToArray();
                }
                else if (fieldName.Equals("Accept-Encoding", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.AcceptEncoding = value.Split(',').Select(s => s.Trim()).ToArray();
                }
                else if (fieldName.Equals("Accept-Language", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.AcceptLanguage = value.Split(',').Select(s => s.Trim()).ToArray();
                }
                else if (fieldName.Equals("Authorization", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.Authorization = value;
                }
                else if (fieldName.Equals("Expect", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.Expect = value;
                }
                else if (fieldName.Equals("From", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.From = value;
                }
                else if (fieldName.Equals("Host", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.Host = value;
                }
                else if (fieldName.Equals("If-Match", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.IfMatch = value;
                }
                else if (StrComp(fieldName, "Cache-Control"))
                {
                    ret.Headers.CacheControl = CacheControlParser.Parse(value);
                }
                else if (StrComp(fieldName, "User-Agent"))
                {
                    ret.Headers.UserAgent = value;
                }
                else if (StrComp(fieldName, "Connection"))
                {
                    ret.Headers.Connection = value;
                }
                else if (StrComp(fieldName, "Content-Length"))
                {
                    ret.Headers.ContentLength = int.Parse(value);
                }
            }

            return ret;
        }


        private static bool StrComp(string one, string two)
        {
            return one.Equals(two, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public class HTTPHeader
    {
        public string Method { get; set; }
        public string RequestURI { get; set; }
        public string HTTPVersionString { get; set; }
        public string Host { get; set; }
        public Dictionary<string, string> HeaderFields { get; set; }

        public RequestHeaders Headers { get; set; }

        public HTTPHeader()
        {
            HeaderFields = new Dictionary<string, string>();
            Headers = new RequestHeaders();
        }

        public class RequestHeaders
        {
            public string[] Accept { get; set; }
            public string[] AcceptCharset { get; set; }
            public string[] AcceptEncoding { get; set; }
            public string[] AcceptLanguage { get; set; }
            public string Authorization { get; set; }
            public string Expect { get; set; }
            public string From { get; set; }
            public string Host { get; set; }
            public string IfMatch { get; set; }
            public string UserAgent { get; set; }
            public string Connection { get; set; }
            public int? ContentLength { get; set; }
            public CacheControlStruct CacheControl { get; set; }

        }

    }


    public abstract class CacheControlParser
    {
        public static CacheControlStruct Parse (string line)
        {
            CacheControlStruct ret = new CacheControlStruct();

            // split by comma
            string[] parts = line.Split(',').Select(s => s.Trim()).ToArray();
            for (int i=0; i<parts.Length; i++)
            {
                // split by equals if there is
                string[] bits = parts[i].Split('=');
                if (StrComp(bits[0], "no-cache"))
                {
                    ret.NoCache = true;
                } else if  (StrComp(bits[0], "no-store"))
                {
                    ret.NoStore = true;
                }
                else if (StrComp(bits[0], "max-age"))
                {
                    if (bits.Length > 1) ret.MaxAge = int.Parse(bits[1]);
                }
                else if (StrComp(bits[0], "max-stale"))
                {
                    ret.MaxStaleAccept = true;
                    if (bits.Length > 1) ret.MaxStaleValue = int.Parse(bits[1]);
                }
                else if (StrComp(bits[0], "min-fresh"))
                {
                    if (bits.Length > 1) ret.MinFresh = int.Parse(bits[1]);
                }
                else if (StrComp(bits[0], "no-transform"))
                {
                    ret.NoTransform = true;
                }
                else if (StrComp(bits[0], "only-if-cached"))
                {
                    ret.OnlyIfCached = true;
                }
            }

            return ret;
        }

        private static bool StrComp(string one, string two)
        {
            return one.Equals(two, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public struct CacheControlStruct
    {
        public bool? NoCache { get; set; }
        public bool? NoStore { get; set; }
        public int? MaxAge { get; set; }
        public bool? MaxStaleAccept { get; set; }
        public int? MaxStaleValue { get; set; }
        public int? MinFresh { get; set; }
        public bool? NoTransform { get; set; }
        public bool? OnlyIfCached { get; set; }
    }
}
