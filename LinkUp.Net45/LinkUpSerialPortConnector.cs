using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace LinkUp.Portable
{
    public class LinkUpSerialPortConnector : LinkUpConnector
    {
        private SerialPort _SerialPort;
        private Task _Task;

        public LinkUpSerialPortConnector(string portName, int baudRate)
        {
            _Task = Task.Factory.StartNew(() =>
            {
                while (true)
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
                }
            });
        }

        public override void Dispose()
        {
            _Task.Dispose();
            if (_SerialPort != null)
            {
                _SerialPort.Dispose();
            }
        }

        protected override void SendData(byte[] data)
        {
            if (_SerialPort != null && _SerialPort.IsOpen)
                _SerialPort.Write(data, 0, data.Length);
        }

        private void _SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_SerialPort != null && _SerialPort.IsOpen)
            {
                while (_SerialPort.BytesToRead > 0)
                {
                    int bytesToRead = _SerialPort.BytesToRead;
                    byte[] buffer = new byte[bytesToRead];
                    _SerialPort.Read(buffer, 0, bytesToRead);
                    OnDataReceived(null);
                }
            }
        }
    }
}