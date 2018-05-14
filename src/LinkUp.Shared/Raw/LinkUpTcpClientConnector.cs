#if NET45 || NETCOREAPP2_0
using System.Net.Sockets;
#endif

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LinkUp.Raw
{
    public class LinkUpTcpClientConnector : LinkUpConnector
    {
#if NET45 || NETCOREAPP2_0
        private TcpClient _TcpClient;
        private Task _Task;
        private bool _IsRunning = true;

        public LinkUpTcpClientConnector(IPAddress destinationAddress, int destinationPort)
        {
            _Task = Task.Factory.StartNew(() =>
            {
                while (_IsRunning)
                {
                    try
                    {
                        if (_TcpClient == null)
                        {
                            _TcpClient = new TcpClient();
                            _TcpClient.Connect(new IPEndPoint(destinationAddress, destinationPort));
                        }

                        byte[] data = new byte[1024];
                        if (_TcpClient.Connected)
                        {
                            if (_TcpClient.GetStream().DataAvailable)
                            {
                                _TcpClient.GetStream().Read(data, 0, 1024);
                                OnDataReceived(data);
                            }
                        }
                        else
                        {
                        }
                    }
                    catch (Exception ex)
                    {
                        _TcpClient.Close();
                        _TcpClient = null;
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
#endif

        public override void Dispose()
        {
#if NET45 || NETCOREAPP2_0
            _IsRunning = false;
            if (_TcpClient != null)
            {
                _TcpClient.Close();
            }
            _Task.Wait();
#endif
        }

        protected override void SendData(byte[] data)
        {
#if NET45 || NETCOREAPP2_0

            if (_TcpClient == null)
            {
                Thread.Sleep(200);
            }
            if (_TcpClient != null)
            {
                if (_TcpClient.Connected)
                {
                    _TcpClient.GetStream().Write(data, 0, data.Length);
                    _TcpClient.GetStream().Flush();
                }
            }
            else
            {
                throw new Exception("Not connected.");
            }

#endif
        }
    }
}