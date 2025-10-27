using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WMS.Utils; // Thư mục chứa RelayCommand
// using WMS.Models; // Giả sử bạn đã có DbContext và Employee
using System.Linq;
using WMS.RelayCommand;

namespace WMS.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        public event Action<int> LoginSuccess; // Event để báo cho View biết đăng nhập thành công
        public event PropertyChangedEventHandler PropertyChanged;

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin, CanLogin);
        }

        private bool CanLogin(object parameter)
        {
            // Có thể thêm logic kiểm tra Username không được rỗng
            return !string.IsNullOrEmpty(Username);
        }

        private void ExecuteLogin(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null) return;

            string password = passwordBox.Password;

            // --- Logic xác thực ---
            try
            {
                // Giả sử bạn có WarehouseDbContext đã được tạo
                // using (var db = new WarehouseDbContext())
                // {
                //    // 1. Hash mật khẩu người dùng nhập
                //    string hashedPassword = HashPassword(password); // Dùng hàm Hash bạn chọn

                //    // 2. Tìm Employee
                //    var employee = db.Employees.FirstOrDefault(e => e.Username == Username);

                //    // 3. So sánh
                //    if (employee != null && employee.PasswordHash == hashedPassword)
                //    {
                //        // Đăng nhập thành công!
                //        ErrorMessage = "";
                //        LoginSuccess?.Invoke(employee.EmployeeID); // Gửi ID nhân viên
                //    }
                //    else
                //    {
                //        ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";
                //    }
                // }

                // --- CODE GIẢ LẬP (Xóa khi có DbContext) ---
                if (Username == "admin" && password == "123")
                {
                    ErrorMessage = "";
                    LoginSuccess?.Invoke(1); // Gửi EmployeeID = 1
                }
                else
                {
                    ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";
                }
                // --- KẾT THÚC CODE GIẢ LẬP ---
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi kết nối CSDL: " + ex.Message;
            }
        }

        // VÍ DỤ VỀ HÀM BĂM MẬT KHẨU (dùng SHA256)
        // Bạn PHẢI dùng cùng một hàm này khi TẠO tài khoản
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}