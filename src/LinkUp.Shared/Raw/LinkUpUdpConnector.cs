#if NET45 || NETCOREAPP2_0
using System.Net.Sockets;
#endif
using System;
using System.Net;
using System.Threading.Tasks;
using System.Threading;

namespace LinkUp.Raw
{
    public class LinkUpUdpConnector : LinkUpConnector
    {
#if NET45 || NETCOREAPP2_0
        private UdpClient _UdpClient;
        private Task _Task;
        private bool _IsRunning = true;

        public LinkUpUdpConnector(IPAddress sourceAddress, IPAddress destinationAddress, int sourcePort, int destinationPort)
        {
            _Task = Task.Run(() =>
            {
                while (_IsRunning)
                {
                    try
                    {
                        if(_UdpClient == null)
                        {
                            _UdpClient = new UdpClient(new IPEndPoint(sourceAddress, sourcePort));
                            _UdpClient.Connect(new IPEndPoint(destinationAddress, destinationPort));
                        }
                        IPEndPoint endPoint = new IPEndPoint(destinationAddress, destinationPort);
                        byte[] data = _UdpClient.Receive(ref endPoint);
                        OnDataReceived(data);
                    }
                    catch (Exception ex)
                    {
                        _UdpClient.Close();
                        _UdpClient = null;
                    }
                }
            });
        }
#endif
        public override void Dispose()
        {
#if NET45 || NETCOREAPP2_0
            _IsRunning = false;
            if(_UdpClient != null)
            {
                _UdpClient.Close();
            }
            _Task.Wait();
#endif
        }

        protected override void SendData(byte[] data)
        {
#if NET45 || NETCOREAPP2_0
            if (_UdpClient == null)
            {
                Thread.Sleep(200);
            }
            if (_UdpClient != null)
            {
                _UdpClient.Send(data, data.Length);
            }
            else
            {
                throw new Exception("Not connected.");
            }
#endif
        }
    }
}