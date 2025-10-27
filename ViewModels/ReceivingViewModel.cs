using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WMS.Model;
using WMS.Utils; // Namespace của RelayCommand
using WMS.ViewModels.Base;

namespace WMS.ViewModels
{
    public class ReceivingViewModel : ViewModelBase
    {
        private Model1 _db;

        // --- Danh sách cho các ComboBox ---
        public ObservableCollection<Supplier> ListSuppliers { get; set; }
        public ObservableCollection<Warehouse> ListWarehouses { get; set; }
        public ObservableCollection<Product> ListProducts { get; set; }

        // --- Thông tin chung của Phiếu (Master) ---
        private Supplier _selectedSupplier;
        public Supplier SelectedSupplier
        {
            get => _selectedSupplier;
            set { _selectedSupplier = value; OnPropertyChanged(); }
        }

        private Warehouse _selectedWarehouse;
        public Warehouse SelectedWarehouse
        {
            get => _selectedWarehouse;
            set { _selectedWarehouse = value; OnPropertyChanged(); }
        }

        private DateTime _noteDate = DateTime.Now;
        public DateTime NoteDate
        {
            get => _noteDate;
            set { _noteDate = value; OnPropertyChanged(); }
        }

        // --- Thông tin "Giỏ hàng" (Details) ---
        public ObservableCollection<ReceivingNoteDetail> NoteDetails { get; set; }

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set { _totalAmount = value; OnPropertyChanged(); }
        }

        // --- Form thêm sản phẩm ---
        private Product _selectedProductToAdd;
        public Product SelectedProductToAdd
        {
            get => _selectedProductToAdd;
            set
            {
                _selectedProductToAdd = value;
                if (value != null)
                    CurrentUnitPrice = value.Price ?? 0;
                OnPropertyChanged();
            }
        }

        private int _currentQuantity = 1;
        public int CurrentQuantity
        {
            get => _currentQuantity;
            set { _currentQuantity = value; OnPropertyChanged(); }
        }

        private decimal _currentUnitPrice;
        public decimal CurrentUnitPrice
        {
            get => _currentUnitPrice;
            set { _currentUnitPrice = value; OnPropertyChanged(); }
        }

        // --- Commands ---
        public ICommand AddProductToNoteCommand { get; }
        public ICommand SaveNoteCommand { get; }
        public ICommand ClearAllCommand { get; }

        public ReceivingViewModel()
        {
            _db = new Model1();
            LoadCboData();

            NoteDetails = new ObservableCollection<ReceivingNoteDetail>();

            AddProductToNoteCommand = new RelayCommand(ExecuteAddProduct, CanExecuteAddProduct);
            SaveNoteCommand = new RelayCommand(ExecuteSaveNote, CanExecuteSaveNote);
            ClearAllCommand = new RelayCommand(o => ClearAll());
        }

        private void LoadCboData()
        {
            ListSuppliers = new ObservableCollection<Supplier>(_db.Suppliers.ToList());
            ListWarehouses = new ObservableCollection<Warehouse>(_db.Warehouses.ToList());
            ListProducts = new ObservableCollection<Product>(_db.Products.ToList());
        }

        private bool CanExecuteAddProduct(object obj)
        {
            return SelectedProductToAdd != null && CurrentQuantity > 0 && CurrentUnitPrice >= 0;
        }

        private void ExecuteAddProduct(object obj)
        {
            var existingDetail = NoteDetails.FirstOrDefault(d => d.ProductID == SelectedProductToAdd.ProductID);

            if (existingDetail != null)
            {
                existingDetail.Quantity += CurrentQuantity;
            }
            else
            {
                var newDetail = new ReceivingNoteDetail
                {
                    ProductID = SelectedProductToAdd.ProductID,
                    Product = SelectedProductToAdd,
                    Quantity = CurrentQuantity,
                    UnitPrice = CurrentUnitPrice
                };
                NoteDetails.Add(newDetail);
            }
            RecalculateTotal();
        }

        private void RecalculateTotal()
        {
            TotalAmount = (decimal)NoteDetails.Sum(d => d.LineTotal);
        }

        private bool CanExecuteSaveNote(object obj)
        {
            return SelectedSupplier != null && SelectedWarehouse != null && NoteDetails.Any();
        }

        private void ExecuteSaveNote(object obj)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var newNote = new ReceivingNote
                    {
                        NoteDate = NoteDate,
                        SupplierID = SelectedSupplier.SupplierID,
                        WarehouseID = SelectedWarehouse.WarehouseID,
                        EmployeeID = 1, // Tạm thời hardcode EmployeeID = 1
                        TotalAmount = TotalAmount
                    };

                    foreach (var detail in NoteDetails)
                    {
                        detail.Product = null;
                        newNote.ReceivingNoteDetails.Add(detail);
                    }

                    _db.ReceivingNotes.Add(newNote);
                    _db.SaveChanges();

                    foreach (var detail in NoteDetails)
                    {
                        var inventoryItem = _db.Inventories.FirstOrDefault(
                            i => i.ProductID == detail.ProductID && i.WarehouseID == newNote.WarehouseID);

                        if (inventoryItem == null)
                        {
                            inventoryItem = new Inventory
                            {
                                ProductID = detail.ProductID,
                                WarehouseID = newNote.WarehouseID,
                                StockQuantity = detail.Quantity
                            };
                            _db.Inventories.Add(inventoryItem);
                        }
                        else
                        {
                            inventoryItem.StockQuantity += detail.Quantity;
                        }
                    }
                    _db.SaveChanges();

                    transaction.Commit();

                    MessageBox.Show("Lưu phiếu nhập thành công!");
                    ClearAll();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Lỗi nghiêm trọng khi lưu phiếu: " + ex.Message);
                }
            }
        }

        private void ClearAll()
        {
            SelectedSupplier = null;
            SelectedWarehouse = null;
            NoteDate = DateTime.Now;
            NoteDetails.Clear();
            ClearProductForm();
            RecalculateTotal();
        }

        private void ClearProductForm()
        {
            SelectedProductToAdd = null;
            CurrentQuantity = 1;
            CurrentUnitPrice = 0;
        }
    }
}