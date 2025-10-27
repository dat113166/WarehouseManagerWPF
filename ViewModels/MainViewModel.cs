using System.Windows.Input;
using WMS.Utils;
using WMS.ViewModels;
using WMS.ViewModels.Base; // Dùng lớp Base

namespace WMS.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentView;
        public ViewModelBase CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        // --- Các lệnh điều hướng ---
        public ICommand ShowDashboardViewCommand { get; }
        public ICommand ShowProductViewCommand { get; }
        // Thêm các Command khác cho Nhập hàng, Xuất hàng...

        public MainViewModel()
        {
            // --- Khởi tạo các Command ---
            ShowDashboardViewCommand = new RelayCommand(o => CurrentView = new DashboardViewModel());
            ShowProductViewCommand = new RelayCommand(o => CurrentView = new ProductViewModel());
            // Thêm các Command khác...

            // --- View mặc định khi mở ứng dụng ---
            CurrentView = new DashboardViewModel(); // Đặt Dashboard làm view mặc định
        }
    }
}