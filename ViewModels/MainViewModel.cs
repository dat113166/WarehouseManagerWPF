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
        public ICommand ShowWarehouseViewCommand { get; }
        public ICommand ShowNhapHangViewCommand { get; }
        public ICommand ShowXuatHangViewCommand { get; }
        public ICommand ShowInventoryViewCommand { get; }
        public ICommand ShowCustomerViewCommand { get; }
        public ICommand ShowSupplierViewCommand { get; }
        public ICommand ExitCommand { get; }

        public MainViewModel()
        {
            // --- Khởi tạo các Command ---
            ShowDashboardViewCommand = new RelayCommand(o => CurrentView = new DashboardViewModel());
            ShowProductViewCommand = new RelayCommand(o => CurrentView = new ProductViewModel());
            ShowWarehouseViewCommand = new RelayCommand(o => CurrentView = new WarehouseViewModel());
            ShowNhapHangViewCommand = new RelayCommand(o => CurrentView = new NhapHangViewModel());
            ShowXuatHangViewCommand = new RelayCommand(o => CurrentView = new XuatHangViewModel());
            ShowInventoryViewCommand = new RelayCommand(o => CurrentView = new InventoryViewModel());
            ShowCustomerViewCommand = new RelayCommand(o => CurrentView = new CustomerViewModel());
            ShowSupplierViewCommand = new RelayCommand(o => CurrentView = new SupplierViewModel());
            ExitCommand = new RelayCommand(o => System.Windows.Application.Current.Shutdown());

            // --- View mặc định khi mở ứng dụng ---
            CurrentView = new DashboardViewModel(); // Đặt Dashboard làm view mặc định
        }
    }
}