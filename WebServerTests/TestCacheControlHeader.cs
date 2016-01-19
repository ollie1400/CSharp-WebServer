using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebServer;

namespace WebServerTests
{
    [TestClass]
    public class TestCacheControlHeader
    {
        [TestMethod]
        public void TestCacheControlHeaderEquality()
        {
            // Arrange
            HttpCacheControlHeader a = new HttpCacheControlHeader();
            a.MaxAge = 3;
            a.MaxStaleAccept = true;
            a.MaxStaleValue = 392;
            a.MinFresh = 0;
            a.NoCache = false;
            a.NoTransform = true;
            a.OnlyIfCached = false;

            HttpCacheControlHeader b = new HttpCacheControlHeader();
            b.MaxAge = 3;
            b.MaxStaleAccept = true;
            b.MaxStaleValue = 392;
            b.MinFresh = 0;
            b.NoCache = false;
            b.NoTransform = true;
            b.OnlyIfCached = false;

            // Assert
            Assert.AreEqual(a, b);
        }

        [TestMethod]
        public void TestCacheControlHeaderHashEquality()
        {
            // Arrange
            HttpCacheControlHeader a = new HttpCacheControlHeader();
            a.MaxAge = 3;
            a.MaxStaleAccept = true;
            a.MaxStaleValue = 392;
            a.MinFresh = 0;
            a.NoCache = false;
            a.NoTransform = true;
            a.OnlyIfCached = false;

            HttpCacheControlHeader b = new HttpCacheControlHeader();
            b.MaxAge = 3;
            b.MaxStaleAccept = true;
            b.MaxStaleValue = 392;
            b.MinFresh = 0;
            b.NoCache = false;
            b.NoTransform = true;
            b.OnlyIfCached = false;

            // Act
            int ha = a.GetHashCode();
            int hb = b.GetHashCode();

            // Assert
            Assert.AreEqual(ha, hb);
        }

        [TestMethod]
        public void TestCacheControlHeaderParser()
        {
            // Arrange
            string accept = "max-age=3,max-stale=392,min-fresh=0,no-transform";
            HttpCacheControlHeader expected = new HttpCacheControlHeader();
            expected.MaxAge = 3;
            expected.MaxStaleAccept = true;
            expected.MaxStaleValue = 392;
            expected.MinFresh = 0;
            expected.NoCache = false;
            expected.NoTransform = true;
            expected.OnlyIfCached = false;

            // Act
            HttpCacheControlHeader header = HttpCacheControlHeader.Parse(accept);

            // Assert
            Assert.AreEqual(expected, header);
        }

        [TestMethod]
        public void TestCacheControlHeaderToString()
        {
            // Arrange
            string accept = "max-age=3,max-stale=392,min-fresh=0,no-transform";
            HttpCacheControlHeader expected = new HttpCacheControlHeader();
            expected.MaxAge = 3;
            expected.MaxStaleAccept = true;
            expected.MaxStaleValue = 392;
            expected.MinFresh = 0;
            expected.NoCache = false;
            expected.NoTransform = true;
            expected.OnlyIfCached = false;

            // Act
            string str = expected.ToString();

            // Assert
            Assert.AreEqual(accept, str);
        }
    }
}
