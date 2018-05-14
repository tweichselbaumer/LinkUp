using LinkUp.Raw;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows;

namespace GyroWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //const string DATA_PORT = "COM3";
        //const string DEBUG_PORT = "COM3";

        // Using a DependencyProperty as the backing store for Count.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register("Count", typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(List<LinkUpPacket>), typeof(MainWindow), new PropertyMetadata(new List<LinkUpPacket>()));

        private const int DATA_BAUD = 2000000;
        private const int DEBUG_BAUD = 250000;

        private bool addData = false;
        private LinkUpSerialPortConnector connector;
        private SynchronizationContext Context;

        private SerialPort port;

        private Stopwatch watch;

        public MainWindow(string port)
        {
            InitializeComponent();

            Context = SynchronizationContext.Current;

            watch = new Stopwatch();
            watch.Start();

            //port = new SerialPort(DEBUG_PORT, DEBUG_BAUD);
            //port.Open();
            //port.ReadExisting();
            //port.DataReceived += Port_DataReceived;

            connector = new LinkUpSerialPortConnector(port, DATA_BAUD);
            connector.ReveivedPacket += Connector_ReveivedPacket;
        }

        public int Count
        {
            get { return (int)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        public List<LinkUpPacket> Data
        {
            get { return (List<LinkUpPacket>)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        private void Button_Click_Save(object sender, RoutedEventArgs e)
        {
            Context.Post((c) =>
            {
                try
                {
                    SaveFileDialog dialog = new SaveFileDialog();
                    dialog.ShowDialog(Application.Current.MainWindow);

                    using (Stream writeStream = dialog.OpenFile())
                    {
                        try
                        {
                            BinaryWriter writer = new BinaryWriter(writeStream);
                            foreach (LinkUpPacket p in Data)
                            {
                                writer.Write(p.Data);
                            }
                            writer.Flush();
                        }
                        catch
                        {
                            // Handle any errors...
                        }
                        finally
                        {
                            // then clean up
                            writeStream.Flush();
                            writeStream.Close();
                        }
                    }
                }
                catch (Exception) { }
            }, null);
        }

        private void Button_Click_Start(object sender, RoutedEventArgs e)
        {
            Context.Post((c) =>
            {
                lock (Data)
                {
                    Data.Clear();
                    addData = true;
                    Count = Data.Count;
                }
            }, null);
        }

        //private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    if (sender is SerialPort)
        //    {
        //        SerialPort port = (SerialPort)sender;
        //        lock (Console.Out)
        //        {
        //            Console.ForegroundColor = ConsoleColor.Cyan;
        //            Console.WriteLine("{0} - Debug:\t{1}", watch.ElapsedTicks * 1000 / Stopwatch.Frequency, port.ReadExisting());
        //            Console.ResetColor();
        //        }
        //    }
        //}
        private void Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            addData = false;
        }

        private void Connector_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            if (addData)
            {
                Context.Post((c) =>
                {
                    lock (Data)
                    {
                        Data.Add(packet);
                        Count = Data.Count;
                    }
                }, null);
            }
            //Console.WriteLine("{0} - Receive:\n\t{1}", watch.ElapsedTicks * 1000 / Stopwatch.Frequency, string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
        }
    }
}