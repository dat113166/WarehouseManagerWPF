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
    public class WarehouseViewModel : ViewModelBase
    {
        public ObservableCollection<Warehouse> Warehouses { get; } = new ObservableCollection<Warehouse>();

        private Warehouse _selectedWarehouse;
        public Warehouse SelectedWarehouse
        {
            get => _selectedWarehouse;
            set { _selectedWarehouse = value; OnPropertyChanged(); }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string _location;
        public string Location
        {
            get => _location;
            set { _location = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        public WarehouseViewModel()
        {
            AddCommand = new RelayCommand(o => AddWarehouse());
            UpdateCommand = new RelayCommand(o => UpdateWarehouse(), o => SelectedWarehouse != null);
            DeleteCommand = new RelayCommand(o => DeleteWarehouse(), o => SelectedWarehouse != null);

            LoadWarehouses();
        }

        private void LoadWarehouses()
        {
            Warehouses.Clear();
            try
            {
                using (var db = new Model1())
                {
                    var list = db.Warehouses.OrderBy(w => w.WarehouseName).ToList();
                    foreach (var w in list) Warehouses.Add(w);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("L?i khi t?i kho: " + ex.Message);
            }
        }

        private void AddWarehouse()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Tên kho không ???c ?? tr?ng.");
                return;
            }

            try
            {
                using (var db = new Model1())
                {
                    var w = new Warehouse { WarehouseName = Name.Trim(), Location = Location?.Trim() };
                    db.Warehouses.Add(w);
                    db.SaveChanges();

                    Warehouses.Add(w);
                }

                ClearFields();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("L?i khi thêm kho: " + ex.Message);
            }
        }

        private void UpdateWarehouse()
        {
            if (SelectedWarehouse == null) return;

            if (string.IsNullOrWhiteSpace(SelectedWarehouse.WarehouseName))
            {
                MessageBox.Show("Tên kho không ???c ?? tr?ng.");
                return;
            }

            try
            {
                using (var db = new Model1())
                {
                    db.Warehouses.Attach(SelectedWarehouse);
                    db.Entry(SelectedWarehouse).State = EntityState.Modified;
                    db.SaveChanges();
                }

                LoadWarehouses();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("L?i khi c?p nh?t kho: " + ex.Message);
                LoadWarehouses();
            }
        }

        private void DeleteWarehouse()
        {
            if (SelectedWarehouse == null) return;
            var r = MessageBox.Show($"B?n có mu?n xóa kho '{SelectedWarehouse.WarehouseName}'?", "Xác nh?n", MessageBoxButton.YesNo);
            if (r != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new Model1())
                {
                    db.Warehouses.Attach(SelectedWarehouse);
                    db.Warehouses.Remove(SelectedWarehouse);
                    db.SaveChanges();
                }

                Warehouses.Remove(SelectedWarehouse);
                ClearFields();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("L?i khi xóa kho: " + ex.Message);
                LoadWarehouses();
            }
        }

        private void ClearFields()
        {
            Name = string.Empty;
            Location = string.Empty;
            SelectedWarehouse = null;
        }
    }
}
