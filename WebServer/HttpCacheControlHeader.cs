using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    /// <summary>
    /// A structure representing a the Cache-Control parameter value of a HTTP request/response
    /// </summary>
    public class HttpCacheControlHeader
    {
        public bool NoCache { get; set; }
        public bool NoStore { get; set; }
        public int? MaxAge { get; set; }
        public bool? MaxStaleAccept { get; set; }
        public int? MaxStaleValue { get; set; }
        public int? MinFresh { get; set; }
        public bool NoTransform { get; set; }
        public bool OnlyIfCached { get; set; }

        public HttpCacheControlHeader()
        {
            NoCache = false;
            NoStore = false;
            MaxAge = null;
            MaxStaleAccept = null;
            MaxStaleValue = null;
            MinFresh = null;
            NoTransform = false;
            OnlyIfCached = false;
        }

        public override bool Equals(object obj)
        {
            if (obj is HttpCacheControlHeader)
            {
                HttpCacheControlHeader other = (HttpCacheControlHeader)obj;
                return (NoCache == other.NoCache && NoStore == other.NoStore && MaxAge == other.MaxAge && MaxStaleAccept == other.MaxStaleAccept && MaxStaleValue == other.MaxStaleValue && MinFresh == other.MinFresh && NoTransform == other.NoTransform && OnlyIfCached == other.OnlyIfCached);
            }
            else {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + NoCache.GetHashCode();
            hash = (hash * 7) + NoStore.GetHashCode();
            hash = (hash * 7) + MaxAge.GetHashCode();
            hash = (hash * 7) + MaxStaleAccept.GetHashCode();
            hash = (hash * 7) + MaxStaleValue.GetHashCode();
            hash = (hash * 7) + MinFresh.GetHashCode();
            hash = (hash * 7) + NoTransform.GetHashCode();
            hash = (hash * 7) + OnlyIfCached.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            string ret = "";

            List<string> bits = new List<string>();
            if (NoCache) bits.Add("no-cache");
            if (NoStore) bits.Add("no-store");
            if (MaxAge != null) bits.Add("max-age=" + MaxAge);
            if (NoStore) bits.Add("no-store");
            if (MaxStaleAccept ?? false)
            {
                string maxStaleAccept = "max-stale";
                if (MaxStaleValue != null) maxStaleAccept += "=" + MaxStaleValue.Value;
                bits.Add(maxStaleAccept);
            }
            if (MinFresh != null) bits.Add("min-fresh=" + MinFresh);
            if (NoTransform) bits.Add("no-transform");
            if (OnlyIfCached) bits.Add("only-if-cached");

            ret = string.Join(",", bits);
            return ret;
        }

        /// <summary>
        /// Parse
        /// </summary>
        /// <param name="line">The Cache-Control parameter value string</param>
        /// <returns></returns>
        public static HttpCacheControlHeader Parse(string line)
        {
            HttpCacheControlHeader ret = new HttpCacheControlHeader();

            // split by comma
            string[] parts = line.Split(',').Select(s => s.Trim()).ToArray();
            for (int i = 0; i < parts.Length; i++)
            {
                // split by equals if there is
                string[] bits = parts[i].Split('=');
                if (StrComp(bits[0], "no-cache"))
                {
                    ret.NoCache = true;
                }
                else if (StrComp(bits[0], "no-store"))
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

}
