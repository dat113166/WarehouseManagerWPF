using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Data.Entity;
using WMS.Model;
using WMS.Utils;
using WMS.ViewModels.Base;

namespace WMS.ViewModels
{
    public class CustomerViewModel : ViewModelBase
    {
        public ObservableCollection<Customer> Customers { get; } = new ObservableCollection<Customer>();

        private Customer _selectedCustomer;
        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set { _selectedCustomer = value; OnPropertyChanged(); }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string _address;
        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(); }
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        public CustomerViewModel()
        {
            AddCommand = new RelayCommand(o => AddCustomer());
            UpdateCommand = new RelayCommand(o => UpdateCustomer(), o => SelectedCustomer != null);
            DeleteCommand = new RelayCommand(o => DeleteCustomer(), o => SelectedCustomer != null);

            LoadCustomers();
        }

        private void LoadCustomers()
        {
            Customers.Clear();
            try
            {
                using (var db = new Model1())
                {
                    var list = db.Customers.OrderBy(c => c.CustomerName).ToList();
                    foreach (var c in list) Customers.Add(c);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("L?i khi t?i khách hàng: " + ex.Message);
            }
        }

        private void AddCustomer()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Tên khách hàng không ???c ?? tr?ng.");
                return;
            }

            try
            {
                using (var db = new Model1())
                {
                    var c = new Customer { CustomerName = Name.Trim(), Address = Address?.Trim(), Phone = Phone?.Trim() };
                    db.Customers.Add(c);
                    db.SaveChanges();

                    Customers.Add(c);
                }

                ClearFields();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("L?i khi thêm khách hàng: " + ex.Message);
            }
        }

        private void UpdateCustomer()
        {
            if (SelectedCustomer == null) return;

            if (string.IsNullOrWhiteSpace(SelectedCustomer.CustomerName))
            {
                MessageBox.Show("Tên khách hàng không ???c ?? tr?ng.");
                return;
            }

            try
            {
                using (var db = new Model1())
                {
                    db.Customers.Attach(SelectedCustomer);
                    db.Entry(SelectedCustomer).State = EntityState.Modified;
                    db.SaveChanges();
                }

                LoadCustomers();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("L?i khi c?p nh?t khách hàng: " + ex.Message);
                LoadCustomers();
            }
        }

        private void DeleteCustomer()
        {
            if (SelectedCustomer == null) return;
            var r = MessageBox.Show($"B?n có mu?n xóa khách hàng '{SelectedCustomer.CustomerName}'?", "Xác nh?n", MessageBoxButton.YesNo);
            if (r != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new Model1())
                {
                    db.Customers.Attach(SelectedCustomer);
                    db.Customers.Remove(SelectedCustomer);
                    db.SaveChanges();
                }

                Customers.Remove(SelectedCustomer);
                ClearFields();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("L?i khi xóa khách hàng: " + ex.Message);
                LoadCustomers();
            }
        }

        private void ClearFields()
        {
            Name = string.Empty;
            Address = string.Empty;
            Phone = string.Empty;
            SelectedCustomer = null;
        }
    }
}
