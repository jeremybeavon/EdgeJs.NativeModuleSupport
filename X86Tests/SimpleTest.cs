using System;
using EdgeJs.NativeModuleSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EdgeJs.NativeModuleSupport.Tests.x86
{
    [TestClass]
    public class SimpleTest
    {
        public static void Main()
        {
            new SimpleTest().TestZombie();
        }

        [TestMethod]
        public void TestZombie()
        {
            string javascript = @"
                var Browser = require('zombie');
                var browser = Browser.create();
                return function (data, callback) {
                    callback(null, 10 + 12);
                };";
            EdgeWithNativeModules.Func(javascript, "zombie")(null).Wait();
        }
    }
}
