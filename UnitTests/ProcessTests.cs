using ZwiftDataCollectionAgent.Console;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using DataAccess;

namespace ZwiftDataCollection.Tests
{
    [TestClass]
    public class ProcessTests
    {
        private class MockConfig : IBespokeConfig
        {
            public string zwiftPassword { get; set; }
            public string zwiftUsername { get; set; }
            public int zwiftId { get; set; }
        }
    }



}




