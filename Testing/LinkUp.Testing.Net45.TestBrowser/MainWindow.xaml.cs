using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Windows;

namespace LinkUp.Testing.Net45.TestBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LinkUpBridge 
        public MainWindow()
        {
            InitializeComponent();
        }

        public List<string> SerialPortNames
        {
            get
            {
                return SerialPort.GetPortNames().Distinct().OrderBy(c=>c).ToList();
            }
        }

        private void comboBox_Debug_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}