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
        private BlockingCollection<byte[]> _OutStream = new BlockingCollection<byte[]>();

        public LinkUpNamedPipeConnector(string name, Mode mode)
        {
            _Name = name;
            _Mode = mode;

            if (mode == Mode.Server)
            {
                _Task = Task.Factory.StartNew(() =>
                {
                    NamedPipeServerStream server = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                    server.WaitForConnection();
                    Task localReadTask = null;
                    Task localWriteTask = null;
                    while (_IsRunning)
                    {
                        try
                        {
                            byte[] dataIn;
                            byte[] dataOut;
                            if (localReadTask == null || localReadTask.IsCanceled || localReadTask.IsCompleted || localReadTask.IsFaulted)
                            {
                                localReadTask = Task.Run(() =>
                                {
                                    try
                                    {
                                        dataIn = new byte[BUFFER_SIZE];
                                        int bytesRead = server.Read(dataIn, 0, BUFFER_SIZE);
                                        if (bytesRead > 0)
                                        {
                                            byte[] result = new byte[bytesRead];
                                            Array.Copy(dataIn, result, bytesRead);
                                            Task.Run(() => { OnDataReceived(result); });
                                        }
                                    }
                                    catch (Exception ex) { }
                                });
                            }

                            if (localWriteTask == null || localWriteTask.IsCanceled || localWriteTask.IsCompleted || localWriteTask.IsFaulted)
                            {
                                localWriteTask = Task.Run(() =>
                                {
                                    try
                                    {
                                        dataOut = _OutStream.Take();
                                        server.Write(dataOut, 0, dataOut.Length);
                                        server.Flush();
                                    }
                                    catch (Exception ex) { }
                                });
                            }
                            if (!server.IsConnected)
                            {
                                server.Close();
                                server = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                                server.WaitForConnection();
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    server.Close();
                }, TaskCreationOptions.LongRunning);
            }
            else if (mode == Mode.Client)
            {
                _Task = Task.Run(() =>
                {
                    NamedPipeClientStream client = new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous);
                    client.Connect();
                    Task localTask = null;
                    while (_IsRunning)
                    {
                        try
                        {
                            byte[] dataIn;
                            byte[] dataOut;
                            if (localTask == null || localTask.IsCanceled || localTask.IsCompleted || localTask.IsFaulted)
                            {
                                localTask = Task.Run(() =>
                                {
                                    dataIn = new byte[BUFFER_SIZE];

                                    int bytesRead = client.Read(dataIn, 0, BUFFER_SIZE);
                                    if (bytesRead > 0)
                                    {
                                        byte[] result = new byte[bytesRead];
                                        Array.Copy(dataIn, result, bytesRead);
                                        OnDataReceived(result);
                                    }
                                });
                            }

                            _OutStream.TryTake(out dataOut, TIMEOUT);
                            if (dataOut != null)
                            {
                                client.Write(dataOut, 0, dataOut.Length);
                                client.Flush();
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    client.Close();
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
            _OutStream.Add(data);
#endif
        }
    }
}