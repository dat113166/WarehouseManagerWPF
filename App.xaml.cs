using System.Windows;

namespace WMS
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Khởi động LoginWindow
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            // (Không cần dòng MainWindow.Show() ở đây nữa)
        }
    }
}