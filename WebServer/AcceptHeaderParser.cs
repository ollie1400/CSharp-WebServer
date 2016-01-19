using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    public abstract class AcceptHeaderParser
    {
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
    }
}
