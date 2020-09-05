using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace RelaUI.Utilities.UT
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void SplitByLinesTest()
        {
            List<string> splitTest = "test\r\none".SplitByLines();
            Assert.IsTrue(splitTest.Count == 2);
            Assert.AreEqual("test", splitTest[0]);
            Assert.AreEqual("one", splitTest[1]);

            splitTest = "test\rone".SplitByLines();
            Assert.IsTrue(splitTest.Count == 2);
            Assert.AreEqual("test", splitTest[0]);
            Assert.AreEqual("one", splitTest[1]);

            splitTest = "test\none".SplitByLines();
            Assert.IsTrue(splitTest.Count == 2);
            Assert.AreEqual("test", splitTest[0]);
            Assert.AreEqual("one", splitTest[1]);

            splitTest = "test\rone\r\n".SplitByLines();
            Assert.IsTrue(splitTest.Count == 3);
            Assert.AreEqual("test", splitTest[0]);
            Assert.AreEqual("one", splitTest[1]);
            Assert.AreEqual("", splitTest[2]);

            splitTest = "test\rone\n".SplitByLines();
            Assert.IsTrue(splitTest.Count == 3);
            Assert.AreEqual("test", splitTest[0]);
            Assert.AreEqual("one", splitTest[1]);
            Assert.AreEqual("", splitTest[2]);

            splitTest = "test\rone\r\ntwo\rthree".SplitByLines();
            Assert.IsTrue(splitTest.Count == 4);
            Assert.AreEqual("test", splitTest[0]);
            Assert.AreEqual("one", splitTest[1]);
            Assert.AreEqual("two", splitTest[2]);
            Assert.AreEqual("three", splitTest[3]);

            splitTest = "\ntest".SplitByLines();
            Assert.IsTrue(splitTest.Count == 2);
            Assert.AreEqual("", splitTest[0]);
            Assert.AreEqual("test", splitTest[1]);

            splitTest = "\n".SplitByLines();
            Assert.IsTrue(splitTest.Count == 2);
            Assert.AreEqual("", splitTest[0]);
            Assert.AreEqual("", splitTest[1]);

            splitTest = "\r\n".SplitByLines();
            Assert.IsTrue(splitTest.Count == 2);
            Assert.AreEqual("", splitTest[0]);
            Assert.AreEqual("", splitTest[1]);

            splitTest = "\n\n".SplitByLines();
            Assert.IsTrue(splitTest.Count == 3);
            Assert.AreEqual("", splitTest[0]);
            Assert.AreEqual("", splitTest[1]);
            Assert.AreEqual("", splitTest[2]);
        }
    }
}
