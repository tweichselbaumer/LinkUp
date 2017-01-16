using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LinkUp.Raw;

namespace LinkUp.Testing.Net45.TestBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort _DebugPort;
        private LinkUpSerialPortConnector _DataPort;
        private Task _Task;

        public MainWindow()
        {
            InitializeComponent();
        }

        public List<string> SerialPortNames
        {
            get
            {
                return SerialPort.GetPortNames().Distinct().OrderBy(c => c).ToList();
            }
        }

        private void comboBox_Debug_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_DebugPort != null)
            {
                _DebugPort.Dispose();
            }
            if (_Task == null)
            {
                _Task = Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        try
                        {
                            if (_DebugPort == null || !_DebugPort.IsOpen)
                            {
                                string port = "";

                                Dispatcher.Invoke(delegate ()
                                {
                                    port = comboBox_Debug.SelectedValue.ToString();
                                });

                                _DebugPort = new SerialPort(port, 3000000);
                                _DebugPort.Open();
                                _DebugPort.DataReceived += _DebugPort_DataReceived;
                            }
                        }
                        catch (Exception)
                        {
                            _DebugPort = null;
                        }
                        Thread.Sleep(100);
                    }
                });
            }

        }

        private void _DebugPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Dispatcher.Invoke(delegate ()
            {
                textBox_Debug.AppendText(_DebugPort.ReadExisting());
                textBox_Debug.ScrollToEnd();
            });
        }

        private void comboBox_Data_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _DataPort = new LinkUpSerialPortConnector(comboBox_Data.SelectedValue.ToString(), 3000000);
            _DataPort.ReveivedPacket += _DataPort_ReveivedPacket;
        }

        private void _DataPort_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            Dispatcher.Invoke(delegate ()
            {
                textBox_DataOut.Text = Encoding.UTF8.GetString(packet.Data);
            });
        }

        private void textBox_Data_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(textBox_DataIn.Text);
                    _DataPort.SendPacket(new LinkUpPacket() { Data = data });
                    textBox_DataIn.Text = "";
                }
                catch (Exception) { }
            }
        }
    }
}