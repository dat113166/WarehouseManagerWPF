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
    public class InventoryViewModel : ViewModelBase
    {
        public ObservableCollection<Inventory> Inventories { get; } = new ObservableCollection<Inventory>();
        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        public ObservableCollection<Warehouse> Warehouses { get; } = new ObservableCollection<Warehouse>();

        private Inventory _selectedInventory;
        public Inventory SelectedInventory
        {
            get => _selectedInventory;
            set { _selectedInventory = value; OnPropertyChanged(); }
        }

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set { _selectedProduct = value; OnPropertyChanged(); }
        }

        private Warehouse _selectedWarehouse;
        public Warehouse SelectedWarehouse
        {
            get => _selectedWarehouse;
            set { _selectedWarehouse = value; OnPropertyChanged(); }
        }

        private int _stockQuantity;
        public int StockQuantity
        {
            get => _stockQuantity;
            set { _stockQuantity = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }

        public InventoryViewModel()
        {
            AddCommand = new RelayCommand(o => AddInventory(), o => SelectedProduct != null && SelectedWarehouse != null);
            UpdateCommand = new RelayCommand(o => UpdateInventory(), o => SelectedInventory != null);
            DeleteCommand = new RelayCommand(o => DeleteInventory(), o => SelectedInventory != null);

            LoadLookups();
            LoadInventories();
        }

        private void LoadLookups()
        {
            Products.Clear();
            Warehouses.Clear();
            try
            {
                using (var db = new Model1())
                {
                    var prods = db.Products.OrderBy(p => p.ProductName).ToList();
                    foreach (var p in prods) Products.Add(p);

                    var whs = db.Warehouses.OrderBy(w => w.WarehouseName).ToList();
                    foreach (var w in whs) Warehouses.Add(w);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("L?i khi load lookup: " + ex.Message);
            }
        }

        private void LoadInventories()
        {
            Inventories.Clear();
            try
            {
                using (var db = new Model1())
                {
                    var list = db.Inventories.Include(i => i.Product).Include(i => i.Warehouse).OrderBy(i => i.InventoryID).ToList();
                    foreach (var it in list) Inventories.Add(it);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("L?i khi load t?n kho: " + ex.Message);
            }
        }

        private void AddInventory()
        {
            if (SelectedProduct == null || SelectedWarehouse == null)
            {
                MessageBox.Show("Ch?n s?n ph?m và kho tr??c khi thêm.");
                return;
            }

            try
            {
                using (var db = new Model1())
                {
                    var inv = new Inventory
                    {
                        ProductID = SelectedProduct.ProductID,
                        WarehouseID = SelectedWarehouse.WarehouseID,
                        StockQuantity = StockQuantity
                    };

                    db.Inventories.Add(inv);
                    db.SaveChanges();

                    // Load related entities for display
                    db.Entry(inv).Reference(i => i.Product).Load();
                    db.Entry(inv).Reference(i => i.Warehouse).Load();

                    Inventories.Add(inv);
                }

                // reset
                SelectedProduct = null;
                SelectedWarehouse = null;
                StockQuantity = 0;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("L?i khi thêm t?n kho: " + ex.Message);
            }
        }

        private void UpdateInventory()
        {
            if (SelectedInventory == null) return;
            try
            {
                using (var db = new Model1())
                {
                    db.Inventories.Attach(SelectedInventory);
                    db.Entry(SelectedInventory).State = EntityState.Modified;
                    db.SaveChanges();
                }

                LoadInventories();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("L?i khi c?p nh?t t?n kho: " + ex.Message);
                LoadInventories();
            }
        }

        private void DeleteInventory()
        {
            if (SelectedInventory == null) return;
            var r = MessageBox.Show($"B?n có mu?n xóa m?c t?n kho #{SelectedInventory.InventoryID}?", "Xác nh?n", MessageBoxButton.YesNo);
            if (r != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new Model1())
                {
                    db.Inventories.Attach(SelectedInventory);
                    db.Inventories.Remove(SelectedInventory);
                    db.SaveChanges();
                }

                Inventories.Remove(SelectedInventory);
                SelectedInventory = null;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("L?i khi xóa t?n kho: " + ex.Message);
                LoadInventories();
            }
        }
    }
}
