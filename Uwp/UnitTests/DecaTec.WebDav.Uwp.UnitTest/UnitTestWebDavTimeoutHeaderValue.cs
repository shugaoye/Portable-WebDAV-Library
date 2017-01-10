﻿using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;

namespace DecaTec.WebDav.Uwp.UnitTest
{
    [TestClass]
    public class UnitTestWebDavTimeoutHeaderValue
    {
        [TestMethod]
        public void UT_UWP_WebDavTimeoutHeaderValue_ToString_Infinite()
        {
            var wdthv = WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeout();

            Assert.AreEqual(wdthv.ToString(), "Infinite");
        }

        [TestMethod]
        public void UT_UWP_WebDavTimeoutHeaderValue_ToString_Timeout()
        {
            var wdthv = WebDavTimeoutHeaderValue.CreateWebDavTimeout(TimeSpan.FromSeconds(500));

            Assert.AreEqual(wdthv.ToString(), "Second-500");
        }

        [TestMethod]
        public void UT_UWP_WebDavTimeoutHeaderValue_ToString_InfiniteWithAlternativeTimeout()
        {
            var wdthv = WebDavTimeoutHeaderValue.CreateInfiniteWebDavTimeoutWithAlternative(TimeSpan.FromSeconds(500));

            Assert.AreEqual(wdthv.ToString(), "Infinite, Second-500");
        }
    }
}
