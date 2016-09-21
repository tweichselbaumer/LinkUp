using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkUp.Portable
{
    public class LinkUpSerialPortConnector : LinkUpConnector
    { 
        public LinkUpSerialPortConnector(string port, int baut)
        {

        }
        protected override void Dispose()
        {
            throw new NotImplementedException();
        }

        protected override void SendData(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
