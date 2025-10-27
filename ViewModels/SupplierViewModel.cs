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
    public class SupplierViewModel : ViewModelBase
    {
        public ObservableCollection<Supplier> Suppliers { get; } = new ObservableCollection<Supplier>();

        private Supplier _selectedSupplier;
        public Supplier SelectedSupplier
        {
            get => _selectedSupplier;
            set { _selectedSupplier = value; OnPropertyChanged(); }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string _contactPerson;
        public string ContactPerson
        {
            get => _contactPerson;
            set { _contactPerson = value; OnPropertyChanged(); }
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
        }

        private string _address;
        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        public SupplierViewModel()
        {
            AddCommand = new RelayCommand(o => AddSupplier());
            UpdateCommand = new RelayCommand(o => UpdateSupplier(), o => SelectedSupplier != null);
            DeleteCommand = new RelayCommand(o => DeleteSupplier(), o => SelectedSupplier != null);

            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            Suppliers.Clear();
            try
            {
                using (var db = new Model1())
                {
                    var list = db.Suppliers.OrderBy(s => s.SupplierName).ToList();
                    foreach (var s in list) Suppliers.Add(s);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("L?i khi t?i nhà phân ph?i: " + ex.Message);
            }
        }

        private void AddSupplier()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Tên nhà phân ph?i không ???c ?? tr?ng.");
                return;
            }

            try
            {
                using (var db = new Model1())
                {
                    var s = new Supplier { SupplierName = Name.Trim(), ContactPerson = ContactPerson?.Trim(), Phone = Phone?.Trim(), Address = Address?.Trim() };
                    db.Suppliers.Add(s);
                    db.SaveChanges();

                    Suppliers.Add(s);
                }

                ClearFields();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("L?i khi thêm nhà phân ph?i: " + ex.Message);
            }
        }

        private void UpdateSupplier()
        {
            if (SelectedSupplier == null) return;

            if (string.IsNullOrWhiteSpace(SelectedSupplier.SupplierName))
            {
                MessageBox.Show("Tên nhà phân ph?i không ???c ?? tr?ng.");
                return;
            }

            try
            {
                using (var db = new Model1())
                {
                    db.Suppliers.Attach(SelectedSupplier);
                    db.Entry(SelectedSupplier).State = EntityState.Modified;
                    db.SaveChanges();
                }

                LoadSuppliers();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("L?i khi c?p nh?t nhà phân ph?i: " + ex.Message);
                LoadSuppliers();
            }
        }

        private void DeleteSupplier()
        {
            if (SelectedSupplier == null) return;
            var r = MessageBox.Show($"B?n có mu?n xóa nhà phân ph?i '{SelectedSupplier.SupplierName}'?", "Xác nh?n", MessageBoxButton.YesNo);
            if (r != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new Model1())
                {
                    db.Suppliers.Attach(SelectedSupplier);
                    db.Suppliers.Remove(SelectedSupplier);
                    db.SaveChanges();
                }

                Suppliers.Remove(SelectedSupplier);
                ClearFields();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("L?i khi xóa nhà phân ph?i: " + ex.Message);
                LoadSuppliers();
            }
        }

        private void ClearFields()
        {
            Name = string.Empty;
            ContactPerson = string.Empty;
            Phone = string.Empty;
            Address = string.Empty;
            SelectedSupplier = null;
        }
    }
}
