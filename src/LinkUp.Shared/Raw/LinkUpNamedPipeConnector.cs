using System;
using System.Collections.Concurrent;

#if NET45
using System.IO.Pipes;
#endif

using System.Threading.Tasks;

namespace LinkUp.Raw
{
    public class LinkUpNamedPipeConnector : LinkUpConnector
    {
#if NET45
        public enum Mode
        {
            Server,
            Client
        }

        private const int BUFFER_SIZE = 1024;
        private const int TIMEOUT = 100;

        private string _Name;
        private Mode _Mode;
        private Task _Task;
        private bool _IsRunning = true;
        private PipeStream _Stream;

        public LinkUpNamedPipeConnector(string name, Mode mode)
        {
            _Name = name;
            _Mode = mode;

            if (mode == Mode.Server)
            {
                _Task = Task.Factory.StartNew(() =>
                {
                    _Stream = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                    (_Stream as NamedPipeServerStream).WaitForConnection();
                    Task localReadTask = null;
                    while (_IsRunning)
                    {
                        try
                        {
                            byte[] dataIn;

                            localReadTask = Task.Run(() =>
                            {
                                try
                                {
                                    while (_IsRunning)
                                    {
                                        dataIn = new byte[BUFFER_SIZE];
                                        int bytesRead = _Stream.Read(dataIn, 0, BUFFER_SIZE);
                                        if (bytesRead > 0)
                                        {
                                            byte[] result = new byte[bytesRead];
                                            Array.Copy(dataIn, result, bytesRead);
                                            Task.Run(() => { OnDataReceived(result); });
                                        }
                                    }
                                }
                                catch (Exception ex) { }
                            });





                            if (!(_Stream as NamedPipeServerStream).IsConnected)
                            {
                                (_Stream as NamedPipeServerStream).Close();
                                _Stream = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                                (_Stream as NamedPipeServerStream).WaitForConnection();
                            }
                            localReadTask.Wait();

                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    _Stream.Close();
                }, TaskCreationOptions.LongRunning);
            }
            else if (mode == Mode.Client)
            {
                _Task = Task.Run(() =>
                {
                    _Stream = new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous);
                    (_Stream as NamedPipeClientStream).Connect();
                    Task localTask = null;
                    while (_IsRunning)
                    {
                        try
                        {
                            byte[] dataIn;
                            if (localTask == null || localTask.IsCanceled || localTask.IsCompleted || localTask.IsFaulted)
                            {
                                localTask = Task.Run(() =>
                                {
                                    dataIn = new byte[BUFFER_SIZE];

                                    int bytesRead = _Stream.Read(dataIn, 0, BUFFER_SIZE);
                                    if (bytesRead > 0)
                                    {
                                        byte[] result = new byte[bytesRead];
                                        Array.Copy(dataIn, result, bytesRead);
                                        OnDataReceived(result);
                                    }
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    _Stream.Close();
                });
            }
        }
#endif

        public override void Dispose()
        {
#if NET45
#if NET45
            _IsRunning = false;
            _Task.Wait();
#endif
            IsDisposed = true;
#endif
        }

        protected override void SendData(byte[] data)
        {
#if NET45
            _Stream.Write(data, 0, data.Length);
            _Stream.Flush();
#endif
        }
    }
}