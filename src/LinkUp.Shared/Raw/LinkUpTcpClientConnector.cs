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
        byte[] data = new byte[1024];

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
                            BeginRead();
                        }

                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        _TcpClient.Close();
                        _TcpClient = null;
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void BeginRead()
        {
            var ns = _TcpClient.GetStream();
            ns.BeginRead(data, 0, data.Length, EndRead, data);
        }

        public void EndRead(IAsyncResult result)
        {
            try
            {
                var buffer = (byte[])result.AsyncState;
                var ns = _TcpClient.GetStream();
                var bytesAvailable = ns.EndRead(result);

                byte[] data = new byte[bytesAvailable];
                Array.Copy(buffer, data, bytesAvailable);
                OnDataReceived(data);
                BeginRead();
            }
            catch (Exception ex)
            {
                _TcpClient.Close();
                _TcpClient = null;
            }
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
                    //_TcpClient.GetStream().Flush();
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