using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LinkUp.Raw;
using System.Collections.Concurrent;

namespace LinkUp.Testing.UnitTest
{
    [TestClass]
    public class MemoryConnectorTest
    {
        [TestMethod]
        public void TestTransmitData()
        {
            BlockingCollection<byte[]> col1 = new BlockingCollection<byte[]>();
            BlockingCollection<byte[]> col2 = new BlockingCollection<byte[]>();

            LinkUpMemoryConnector c1 = new LinkUpMemoryConnector(col1,col2);
            LinkUpMemoryConnector c2 = new LinkUpMemoryConnector(col2, col1);

            c1.
        }
    }
}
