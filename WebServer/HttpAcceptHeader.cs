using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    /// <summary>
    /// An object representing an Http Accept header request
    /// </summary>
    public class HttpAcceptHeader
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
                for (int i = 1; i < acceptParts.Count; i++)
                {
                    ret += "\r\n";
                    ret += acceptParts[i].ToString();
                }
            }
            return ret;
        }

        /// <summary>
        /// Are two HttpAcceptHeader objects the same?
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is HttpAcceptHeader)
            {
                HttpAcceptHeader other = (HttpAcceptHeader)obj;
                if (other.acceptParts.Count != acceptParts.Count) return false;
                for (int i=0; i<acceptParts.Count; i++)
                {
                    if (!acceptParts[i].Equals(other.acceptParts[i])) return false;
                }
                return true;
            }
            else {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            int hash = 13;
            for (int i = 0; i < acceptParts.Count; i++)
            {
                hash = (hash * 7) + acceptParts[i].GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Check to see if a file with the given extension and type would be accepted by the sender of this HttpHeader
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <returns>The weight (1 by default).  Anything above 0 is a yes.</returns>
        public double ExtensionAccepted(string extension)
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

        public static HttpAcceptHeader Parse(string acceptHeaderString)
        {
            HttpAcceptHeader ret = new HttpAcceptHeader();
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
                        qval = double.Parse(parts[1].Substring(parts[1].IndexOf("=") + 1).Trim());
                    }

                    // assign
                    ret.AddAccept(type, extension, qval);
                }
                catch (Exception ex)
                { }
            }
            return ret;
        }


        /// <summary>
        /// An Accept part of an Accept http line
        /// </summary>
        public class AcceptPart
        {
            public string Type { get; set; }
            public string Extension { get; set; }
            public double? QValue { get; set; }
            public override string ToString()
            {
                return Type + "/" + Extension + (QValue.HasValue ? ";q=" + QValue.Value.ToString("F1") : "");
            }

            public override bool Equals(object obj)
            {
                if (obj is AcceptPart)
                {
                    AcceptPart other = (AcceptPart)obj;
                    return (Type == other.Type && Extension == other.Extension && QValue == other.QValue);
                }
                else {
                    return base.Equals(obj);
                }
            }

            public override int GetHashCode()
            {
                int hash = 13;
                hash = (hash * 7) + Type.GetHashCode();
                hash = (hash * 7) + Extension.GetHashCode();
                hash = (hash * 7) + QValue.GetHashCode();
                return hash;
            }
        }
    }
}
