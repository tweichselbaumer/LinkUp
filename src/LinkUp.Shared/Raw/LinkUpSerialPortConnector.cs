using System;
using System.Threading.Tasks;
using System.Threading;

#if NET45 || NETCOREAPP2_0

using System.IO.Ports;

#endif

namespace LinkUp.Raw
{
    public class LinkUpSerialPortConnector : LinkUpConnector
    {
#if NET45
        private SerialPort _SerialPort;
        private Task _Task;
        private bool _IsRunning = true;
#endif

        public LinkUpSerialPortConnector(string portName, int baudRate)
        {
#if NET45
            _Task = Task.Run(() =>
            {
                while (_IsRunning)
                {
                    try
                    {
                        if (_SerialPort == null || !_SerialPort.IsOpen)
                        {
                            _SerialPort = new SerialPort(portName, baudRate);
                            _SerialPort.Open();
                            _SerialPort.DataReceived += _SerialPort_DataReceived;
                        }
                    }
                    catch (Exception)
                    {
                        _SerialPort = null;
                    }
                    Thread.Sleep(100);
                }
            });
#endif
#if NETCOREAPP2_0
            throw new NotImplementedException();
#endif
        }

        public override void Dispose()
        {
#if NET45
            _IsRunning = false;
            _Task.Wait();
            if (_SerialPort != null)
            {
                _SerialPort.Dispose();
            }
#endif
            IsDisposed = true;
        }

        protected override void SendData(byte[] data)
        {
#if NET45
            if (_SerialPort == null || !_SerialPort.IsOpen)
            {
                Thread.Sleep(200);
            }
            if (_SerialPort != null && _SerialPort.IsOpen)
            {
                _SerialPort.Write(data, 0, data.Length);
            }
            else
            {
                throw new Exception("Not connected.");
            }
#endif
        }

#if NET45
        private void _SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_SerialPort != null && _SerialPort.IsOpen)
            {
                while (_SerialPort.BytesToRead > 0)
                {
                    int bytesToRead = _SerialPort.BytesToRead;
                    byte[] buffer = new byte[bytesToRead];
                    _SerialPort.Read(buffer, 0, bytesToRead);
                    OnDataReceived(buffer);
                }
            }
        }
#endif
    }
}