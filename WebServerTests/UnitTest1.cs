using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebServer;

namespace WebServerTests
{
    [TestClass]
    public class TestAcceptHeader
    {
        [TestMethod]
        public void TestAcceptHeaderEquality()
        {
            // Arrange
            HttpAcceptHeader a = new HttpAcceptHeader();
            a.AddAccept("text", "html", null);
            a.AddAccept("text", "json", 0.9);
            a.AddAccept("*", "*", 0.2);

            HttpAcceptHeader b = new HttpAcceptHeader();
            b.AddAccept("text", "html", null);
            b.AddAccept("text", "json", 0.9);
            b.AddAccept("*", "*", 0.2);

            // Assert
            Assert.AreEqual(a, b);
        }

        [TestMethod]
        public void TestAcceptHeaderHashEquality()
        {
            // Arrange
            HttpAcceptHeader a = new HttpAcceptHeader();
            a.AddAccept("text", "html", null);
            a.AddAccept("text", "json", 0.9);
            a.AddAccept("*", "*", 0.2);

            HttpAcceptHeader b = new HttpAcceptHeader();
            b.AddAccept("text", "html", null);
            b.AddAccept("text", "json", 0.9);
            b.AddAccept("*", "*", 0.2);

            // Act
            int ha = a.GetHashCode();
            int hb = b.GetHashCode();

            // Assert
            Assert.AreEqual(ha, hb);
        }

        [TestMethod]
        public void TestAcceptHeaderParser()
        {
            // Arrange
            string accept = "text/html,text/json;q=0.9,*/*;q=0.2";
            HttpAcceptHeader expected = new HttpAcceptHeader();
            expected.AddAccept("text", "html", null);
            expected.AddAccept("text", "json", 0.9);
            expected.AddAccept("*", "*", 0.2);

            // Act
            HttpAcceptHeader header = HttpAcceptHeader.Parse(accept);

            // Assert
            Assert.AreEqual(expected, header);
        }
    }
}
