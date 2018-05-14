using System.Windows;

namespace GyroWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 1)
            {
                MainWindow w = new MainWindow(e.Args[0]);
                w.Show();
            }
        }
    }
}