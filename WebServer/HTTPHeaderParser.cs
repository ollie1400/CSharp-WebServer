using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    public abstract class HTTPHeaderParser
    {
        public static HttpHeader Parse(string header)
        {
            HttpHeader ret = new HttpHeader();

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
                //ret.HeaderFields.Add(fieldName, value);

                // what is it? (case-insensitive https://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2)
                if (fieldName.Equals("Accept", StringComparison.InvariantCultureIgnoreCase))
                {
                    ret.Headers.Accept = AcceptHeaderParser.Parse(value);
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
                    ret.Headers.Connection = ConnectionParser.Parse(value);
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


    /// <summary>
    /// A class holding details of an HTTP Header
    /// </summary>
    public class HttpHeader
    {
        virtual public string Method { get; set; }
        virtual public string RequestURI { get; set; }
        public string HTTPVersionString { get; set; }
        //public Dictionary<string, string> HeaderFields { get; set; }
        public string NewLine { get; set; }

        public HttpHeaders Headers { get; set; }

        public HttpHeader()
        {
            //HeaderFields = new Dictionary<string, string>();
            Headers = new HttpHeaders();
            NewLine = "\r\n";
        }

        public class HttpHeaders
        {
            public AcceptHeader Accept { get; set; }
            public string[] AcceptCharset { get; set; }
            public string[] AcceptEncoding { get; set; }
            public string[] AcceptLanguage { get; set; }
            public string Authorization { get; set; }
            public string Expect { get; set; }
            public string From { get; set; }
            public string Host { get; set; }
            public string IfMatch { get; set; }
            public string UserAgent { get; set; }
            public long? ContentLength { get; set; }
            public CacheControlStruct CacheControl { get; set; }
            public ConnectionStruct Connection { get; set; }
            public string ContentType { get; set; }
            public DateTime? LastModified { get; set; }

            public HttpHeaders()
            {
                CacheControl = new CacheControlStruct();
                Connection = new ConnectionStruct();
            }
        }

    }


    public abstract class AcceptHeaderParser
    {
        public static AcceptHeader Parse(string acceptHeaderString)
        {
            AcceptHeader ret = new AcceptHeader();
            foreach (string part in acceptHeaderString.Split(',').Select(s => s.Trim()).ToArray())
            {
                try
                {
                    // q value?
                    string[] parts = part.Split(';');
                    string[] fileParts = parts[0].Split('/');
                    string type = fileParts[0];
                    string extension = fileParts[1];
                    double? qval = null;
                    if (parts.Length > 1)
                    {
                        qval = double.Parse(parts[1].Substring(parts[1].IndexOf("=")+1).Trim());
                    }

                    // assign
                    ret.AddAccept(type, extension, qval);
                }
                catch (Exception ex)
                { }
            }
            return ret;
        }
    }

    public class AcceptHeader
    {
        public AcceptPart[] AcceptParts { get; private set; }

        private List<AcceptPart> acceptParts = new List<AcceptPart>();

        public void AddAccept(string type, string extension, double? weight)
        {
            acceptParts.Add(new AcceptPart() { Type = type, Extension = extension, QValue = weight });
            AcceptParts = acceptParts.ToArray();
        }

        public void Clear()
        {
            acceptParts.Clear();
            AcceptParts = AcceptParts.ToArray();
        }

        public override string ToString()
        {
            string ret = "";
            if (acceptParts.Count > 0)
            {
                ret += acceptParts[0].ToString();
                for (int i=1; i< acceptParts.Count; i++)
                {
                    ret += "\r\n";
                    ret += acceptParts[i].ToString();
                }
            }
            return ret;
        }

        /// <summary>
        /// Check to see if a file with the given extension and type would be accepted by the sender of this HttpHeader
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <returns>The weight (1 by default).  Anything above 0 is a yes.</returns>
        public double ExtensionAccepted(string type, string extension)
        {
            double ret = 0.0;

            // if the header list contains "*/*" then anything is accepted
            if (acceptParts.Any(a => a.Extension == "*" && a.Type == "*"))
            {
                return 1.0;
            }
            AcceptPart[] ap = acceptParts.Where(a => a.Extension.IndexOf(extension, StringComparison.InvariantCultureIgnoreCase) >= 0).OrderByDescending(a => a.QValue ?? 1.0).ToArray();
            if (ap.Length > 0)
            {
                return ap[0].QValue ?? 1.0;
            }

            return ret;
        }

        public class AcceptPart
        {
            public string Type { get; set; }
            public string Extension { get; set; }
            public double? QValue { get; set; }
            public override string ToString()
            {
                return Type + "/" + Extension + (QValue.HasValue ? ";q=" + QValue.Value.ToString("F1") : "");
            }
        }
    }

    public class HttpResponse : HttpHeader
    {
        public int ReturnCode { get; set; }
        public object Response { get; set; }

        /// <summary>
        /// Add an extra NewLine string at the end of the returned response header string (or byte array).  False by default.  ONLY added if Response == null
        /// </summary>
        public bool AddExtraNewLine { get; set; }

        public HttpResponse ()
        {
            HTTPVersionString = "HTTP/1.1";
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
            ret += HTTPVersionString + " " + ReturnCode + NewLine;
            if (Headers.ContentType != null)
            {
                ret += "Content-Type: " + Headers.ContentType + NewLine;
            }
            if (Headers.ContentLength != null)
            {
                ret += "Content-Length: " + Headers.ContentLength.Value + NewLine;
            }
            if (Headers.Connection != null)
            {
                if (Headers.Connection.Close != null || Headers.Connection.KeepAlive != null)
                {
                    ListMaker connectionList = new ListMaker();
                    if (Headers.Connection.Close ?? false) connectionList.Add("close");
                    if (Headers.Connection.KeepAlive ?? false) connectionList.Add("keep-alive");
                    if (connectionList.Count > 0)
                    {
                        ret += "Connection: " + connectionList.MakeList() + NewLine;
                    }
                }
            }
            if (Headers.CacheControl != null)
            {
                string cacheControlString = Headers.CacheControl.ToString();
                if (cacheControlString.Length > 0) ret += "Cache-Control: " + cacheControlString + NewLine;
            }
            ret += "Date: " + DateTime.Now.ToString("R") + NewLine;
            if (Headers.LastModified != null)
            {
                ret += "Last-Modified: " + Headers.LastModified.Value.ToString("R") + NewLine;
            }

            // what is the response?
            // if it's a string, add the new line separator and put it in now
            if (Response is string)
            {
                if (!String.IsNullOrEmpty(Response as string))
                {
                    ret += NewLine + Response as string;
                }
                else if (AddExtraNewLine)
                {
                    ret += NewLine;
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
                headerBytes = Encoding.UTF8.GetBytes(ToString() + NewLine);

                // combine
                totalResponse = new byte[headerBytes.Length + contentBytes.Length];
                Buffer.BlockCopy(headerBytes, 0, totalResponse, 0, headerBytes.Length);
                Buffer.BlockCopy(contentBytes, 0, totalResponse, headerBytes.Length, contentBytes.Length);
            }else
            {
                headerBytes = Encoding.UTF8.GetBytes(ToString());
                totalResponse = headerBytes;
            }

            return totalResponse;
        }
    }

    /// <summary>
    /// Simple helper class that takes a list of Elements and 
    /// </summary>
    public class ListMaker : List<string>
    {
        public string Separator = ", ";
        public ListMaker()
        {
        }
        public string MakeList()
        {
            if (Count == 0) return "";
            StringBuilder ret = new StringBuilder();
            ret.Append(this[0]);
            if (Count > 1)
            {
                ret.Append(Separator);
                for (int i=1; i<Count-1; i++)
                {
                    ret.Append(this[i]);
                    ret.Append(Separator);                   
                }
                ret.Append(this[Count-1]);
            }
            return ret.ToString();
        }
    }



    /// <summary>
    /// Parse a http value for the Cache-Control parameter into a CacheControlStruct object.
    /// </summary>
    public abstract class CacheControlParser
    {
        /// <summary>
        /// Parse
        /// </summary>
        /// <param name="line">The Cache-Control parameter value string</param>
        /// <returns></returns>
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

    /// <summary>
    /// A structure representing a the Cache-Control parameter value of a HTTP request/response
    /// </summary>
    public class CacheControlStruct
    {
        public bool? NoCache { get; set; }
        public bool? NoStore { get; set; }
        public int? MaxAge { get; set; }
        public bool? MaxStaleAccept { get; set; }
        public int? MaxStaleValue { get; set; }
        public int? MinFresh { get; set; }
        public bool? NoTransform { get; set; }
        public bool? OnlyIfCached { get; set; }

        public override string ToString()
        {
            string ret = "";

            ListMaker list = new ListMaker();
            if (NoCache ?? false) list.Add("no-cache");
            if (NoStore ?? false) list.Add("no-store");
            if (MaxAge != null) list.Add("max-age="+MaxAge);
            if (NoStore ?? false) list.Add("no-store");
            if (MaxStaleAccept ?? false)
            {
                list.Add("max-stale-accept");
                if (MaxStaleValue != null) list.Add("=" + MaxStaleValue.Value);
            }
            if (MinFresh != null) list.Add("min-fresh=" + MinFresh);
            if (NoTransform ?? false) list.Add("no-transform");
            if (OnlyIfCached ?? false) list.Add("only-if-cached");

            ret = list.MakeList();
            return ret;
        }
    }

    public abstract class ConnectionParser
    {
        public static ConnectionStruct Parse (string line)
        {
            ConnectionStruct ret = new ConnectionStruct();

            // split by comma
            string[] parts = line.Split(',').Select(s => s.Trim()).ToArray();
            for (int i = 0; i < parts.Length; i++)
            {
                // split by equals if there is
                string[] bits = parts[i].Split('=');
                if (StrComp(bits[0], "keep-alive"))
                {
                    ret.KeepAlive = true;
                } else if (StrComp(bits[0], "close"))
                {
                    ret.Close = true;
                }
            }

            return ret;
        }

        private static bool StrComp(string one, string two)
        {
            return one.Equals(two, StringComparison.InvariantCultureIgnoreCase);
        }
    }

    /// <summary>
    /// A structure representing the value of a Connection parameter in an HTTP request/response
    /// </summary>
    public class ConnectionStruct
    {
        public bool? KeepAlive { get; set; }
        public bool? Close { get; set; }
    }
}
