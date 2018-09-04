#if NET45 || NETCOREAPP2_0

using System.Net.Sockets;

#endif

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace LinkUp.Raw
{
    public class LinkUpTcpClientConnector : LinkUpConnector
    {
#if NET45 || NETCOREAPP2_0
        private TcpClient _TcpClient;
        private Task _Task;
        private bool _IsRunning = true;
        private const int maxRead = 1024 * 100;
        private byte[] data = new byte[maxRead];

        private BlockingCollection<byte[]> _Queue = new BlockingCollection<byte[]>();

        public LinkUpTcpClientConnector(IPAddress destinationAddress, int destinationPort)
        {
            _Task = Task.Factory.StartNew(() =>
            {
                byte[] buffer = new byte[maxRead * 50];
                while (_IsRunning)
                {
                    try
                    {
                        byte[] data;

                        int size = 0;
                        int count = 0;

                        while (_Queue.TryTake(out data))
                        {
                            Array.Copy(data, 0, buffer, size, data.Length);
                            size += data.Length;
                            count++;
                            if (count >= 50)
                            {
                                break;
                            }
                        }

                        if (size > 0)
                        {
                            data = new byte[size];
                            Array.Copy(buffer, data, size);
                            OnDataReceived(data);
                        }

                        Thread.Sleep(1);

                        if (_TcpClient == null)
                        {
                            _TcpClient = new TcpClient();
                            _TcpClient.Connect(new IPEndPoint(destinationAddress, destinationPort));
                            OnConnected();
                            BeginRead();
                        }
                    }
                    catch (Exception)
                    {
                        try
                        {
                            if (_TcpClient != null)
                                _TcpClient.Close();
                        }
                        catch (Exception) { }
                        _TcpClient = null;
                        OnDisconnected();
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
                _Queue.Add(data);
                BeginRead();
            }
            catch (Exception)
            {
                try
                {
                    if (_TcpClient != null)
                        _TcpClient.Close();
                }
                catch (Exception) { }
                _TcpClient = null;
                OnDisconnected();
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
            _TcpClient = null;
            _Task.Wait();
            base.Dispose();
#endif
        }

        protected override void SendData(byte[] data)
        {
#if NET45 || NETCOREAPP2_0
            try
            {
                if (_TcpClient == null)
                {
                    Thread.Sleep(200);
                }
                if (_TcpClient != null)
                {
                    if (_TcpClient.Connected)
                    {
                        _TcpClient.GetStream().Write(data, 0, data.Length);
                    }
                }
                else
                {
                    try
                    {
                        if (_TcpClient != null)
                            _TcpClient.Close();
                    }
                    catch (Exception) { }
                    _TcpClient = null;
                    OnDisconnected();
                }
            }
            catch (Exception)
            {
                try
                {
                    if (_TcpClient != null)
                        _TcpClient.Close();
                }
                catch (Exception) { }
                _TcpClient = null;
                OnDisconnected();
            }
#endif
        }
    }
}