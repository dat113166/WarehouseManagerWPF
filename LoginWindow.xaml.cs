using System.Windows;
using WMS.ViewModels;

namespace WMS
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            var viewModel = new LoginViewModel();
            this.DataContext = viewModel;

            // Đăng ký sự kiện LoginSuccess
            viewModel.LoginSuccess += (employeeId) =>
            {
                // 1. Tạo MainWindow (bạn có thể truyền employeeId vào nếu cần)
                MainWindow mainWindow = new MainWindow();
                // mainWindow.CurrentEmployeeId = employeeId; // Ví dụ

                // 2. Hiển thị MainWindow
                mainWindow.Show();

                // 3. Đóng LoginWindow
                this.Close();
            };
        }
    }
}