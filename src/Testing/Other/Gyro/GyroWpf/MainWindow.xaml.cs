using LinkUp.Raw;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GyroWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string DATA_PORT = "COM4";
        const string DEBUG_PORT = "COM3";

        const int DATA_BAUD = 250000;
        const int DEBUG_BAUD = 250000;

        SerialPort port;
        LinkUpSerialPortConnector connector;
        Stopwatch watch;

        public MainWindow()
        {
            InitializeComponent();

            watch = new Stopwatch();
            watch.Start();

            port = new SerialPort(DEBUG_PORT, DEBUG_BAUD);
            port.Open();
            port.ReadExisting();
            port.DataReceived += Port_DataReceived;

            connector = new LinkUpSerialPortConnector(DATA_PORT, DATA_BAUD);
            connector.ReveivedPacket += Connector_ReveivedPacket;
        }

        private void Connector_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            Console.WriteLine("{0} - Receive:\n\t{1}", watch.ElapsedTicks * 1000 / Stopwatch.Frequency, string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }
    }
}
