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
    public class ShippingViewModel : ViewModelBase
    {
        private Model1 _db;

        // --- Danh sách cho các ComboBox ---
        public ObservableCollection<Customer> ListCustomers { get; set; }
        public ObservableCollection<Warehouse> ListWarehouses { get; set; }
        public ObservableCollection<Product> ListProducts { get; set; }

        // --- Thông tin chung của Phiếu (Master) ---
        private Customer _selectedCustomer;
        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set { _selectedCustomer = value; OnPropertyChanged(); }
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
        public ObservableCollection<ShippingNoteDetail> NoteDetails { get; set; }

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
                // Tự động lấy giá bán (chính là giá trong bảng Product)
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

        public ShippingViewModel()
        {
            _db = new Model1();
            LoadCboData();

            NoteDetails = new ObservableCollection<ShippingNoteDetail>();

            AddProductToNoteCommand = new RelayCommand(ExecuteAddProduct, CanExecuteAddProduct);
            SaveNoteCommand = new RelayCommand(ExecuteSaveNote, CanExecuteSaveNote);
            ClearAllCommand = new RelayCommand(o => ClearAll());
        }

        private void LoadCboData()
        {
            ListCustomers = new ObservableCollection<Customer>(_db.Customers.ToList());
            ListWarehouses = new ObservableCollection<Warehouse>(_db.Warehouses.ToList());
            ListProducts = new ObservableCollection<Product>(_db.Products.ToList());
        }

        // --- Logic thêm vào giỏ hàng (CÓ KIỂM TRA TỒN KHO) ---
        private bool CanExecuteAddProduct(object obj)
        {
            return SelectedProductToAdd != null && SelectedWarehouse != null && CurrentQuantity > 0 && CurrentUnitPrice >= 0;
        }

        private void ExecuteAddProduct(object obj)
        {
            // ***** BƯỚC KIỂM TRA TỒN KHO *****
            var inventoryItem = _db.Inventories.FirstOrDefault(
                i => i.ProductID == SelectedProductToAdd.ProductID && i.WarehouseID == SelectedWarehouse.WarehouseID);

            int currentStock = (inventoryItem != null) ? inventoryItem.StockQuantity : 0;

            // Kiểm tra số lượng trong giỏ hàng (nếu đã thêm)
            var existingDetail = NoteDetails.FirstOrDefault(d => d.ProductID == SelectedProductToAdd.ProductID);
            int quantityInCart = (existingDetail != null) ? existingDetail.Quantity : 0;

            if (currentStock < (quantityInCart + CurrentQuantity))
            {
                MessageBox.Show($"Không đủ hàng! Tồn kho '{SelectedWarehouse.WarehouseName}' chỉ còn: {currentStock}");
                return;
            }
            // ***** KẾT THÚC KIỂM TRA TỒN KHO *****


            if (existingDetail != null)
            {
                // Cập nhật số lượng
                existingDetail.Quantity += CurrentQuantity;
            }
            else
            {
                // Thêm mới vào giỏ
                var newDetail = new ShippingNoteDetail
                {
                    ProductID = SelectedProductToAdd.ProductID,
                    Product = SelectedProductToAdd, // Để DataGrid hiển thị tên
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

        // --- Logic LƯU PHIẾU (Quan trọng) ---
        private bool CanExecuteSaveNote(object obj)
        {
            return SelectedCustomer != null && SelectedWarehouse != null && NoteDetails.Any();
        }

        private void ExecuteSaveNote(object obj)
        {
            // Bắt đầu 1 GIAO DỊCH (Transaction)
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    // 1. TẠO PHIẾU XUẤT (Master)
                    var newNote = new ShippingNote
                    {
                        NoteDate = NoteDate,
                        CustomerID = SelectedCustomer.CustomerID,
                        WarehouseID = SelectedWarehouse.WarehouseID,
                        EmployeeID = 1, // Tạm thời hardcode EmployeeID = 1
                        TotalAmount = TotalAmount
                    };

                    // 2. THÊM CÁC CHI TIẾT (Details) VÀO PHIẾU
                    foreach (var detail in NoteDetails)
                    {
                        detail.Product = null;
                        newNote.ShippingNoteDetails.Add(detail);
                    }

                    _db.ShippingNotes.Add(newNote);
                    _db.SaveChanges(); // Lưu phiếu và chi tiết phiếu

                    // 3. CẬP NHẬT TỒN KHO (TRỪ TỒN KHO)
                    foreach (var detail in NoteDetails)
                    {
                        var inventoryItem = _db.Inventories.FirstOrDefault(
                            i => i.ProductID == detail.ProductID && i.WarehouseID == newNote.WarehouseID);

                        if (inventoryItem == null || inventoryItem.StockQuantity < detail.Quantity)
                        {
                            // Kiểm tra lại lần nữa (phòng trường hợp 2 người cùng xuất 1 lúc)
                            throw new Exception($"Không đủ tồn kho cho sản phẩm ID: {detail.ProductID}");
                        }

                        // TRỪ TỒN KHO
                        inventoryItem.StockQuantity -= detail.Quantity;
                    }
                    _db.SaveChanges(); // Lưu tồn kho

                    // 4. HOÀN TẤT GIAO DỊCH
                    transaction.Commit();

                    MessageBox.Show("Lưu phiếu xuất thành công!");
                    ClearAll();
                }
                catch (Exception ex)
                {
                    // 5. NẾU LỖI -> HỦY BỎ TẤT CẢ
                    transaction.Rollback();
                    MessageBox.Show("Lỗi nghiêm trọng khi lưu phiếu: " + ex.Message);
                }
            }
        }

        private void ClearAll()
        {
            SelectedCustomer = null;
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