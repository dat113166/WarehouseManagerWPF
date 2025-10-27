using System.Windows;

namespace WMS
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // StartupUri in App.xaml will open MainWindow. Removed LoginWindow startup.
        }
    }
}