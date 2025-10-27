using System;
using System.Collections.ObjectModel; // Để dùng ObservableCollection
using System.Data.Entity; // Để dùng .Include() (tải kèm)
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WMS.Model; // Quan trọng: để dùng DbContext (Model1) và Product
using WMS.Utils;
using WMS.ViewModels.Base;

namespace WMS.ViewModels
{
    public class ProductViewModel : ViewModelBase
    {
        private Model1 _db; // DbContext của bạn

        // --- Danh sách Sản phẩm để binding với DataGrid ---
        private ObservableCollection<Product> _listProducts;
        public ObservableCollection<Product> ListProducts
        {
            get => _listProducts;
            set { _listProducts = value; OnPropertyChanged(); }
        }

        // --- Các thuộc tính binding cho Form Thêm/Sửa ---
        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
                if (SelectedProduct != null)
                {
                    // Khi chọn 1 dòng, copy thông tin lên form
                    ProductName = SelectedProduct.ProductName;
                    Unit = SelectedProduct.Unit;
                    Price = SelectedProduct.Price;
                    SelectedCategory = SelectedProduct.Category;
                    SelectedSupplier = SelectedProduct.Supplier;
                }
            }
        }

        private string _productName;
        public string ProductName
        {
            get => _productName;
            set { _productName = value; OnPropertyChanged(); }
        }

        private string _unit;
        public string Unit
        {
            get => _unit;
            set { _unit = value; OnPropertyChanged(); }
        }

        private decimal? _price;
        public decimal? Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        // --- Các danh sách cho ComboBox ---
        public ObservableCollection<Category> ListCategories { get; set; }
        public ObservableCollection<Supplier> ListSuppliers { get; set; }

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); }
        }

        private Supplier _selectedSupplier;
        public Supplier SelectedSupplier
        {
            get => _selectedSupplier;
            set { _selectedSupplier = value; OnPropertyChanged(); }
        }

        // --- Các ICommand ---
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearFormCommand { get; }


        public ProductViewModel()
        {
            _db = new Model1(); // Khởi tạo DbContext
            LoadData(); // Tải dữ liệu

            // Khởi tạo các Command
            AddCommand = new RelayCommand(ExecuteAdd, CanExecuteAdd);
            UpdateCommand = new RelayCommand(ExecuteUpdate, CanExecuteUpdateOrDelete);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanExecuteUpdateOrDelete);
            ClearFormCommand = new RelayCommand(o => ClearForm());
        }

        // --- Các hàm tải dữ liệu ---
        private void LoadData()
        {
            try
            {
                // Tải danh sách sản phẩm (bao gồm cả Category và Supplier)
                ListProducts = new ObservableCollection<Product>(
                    _db.Products.Include(p => p.Category).Include(p => p.Supplier).ToList());

                // Tải danh sách cho ComboBox
                ListCategories = new ObservableCollection<Category>(_db.Categories.ToList());
                ListSuppliers = new ObservableCollection<Supplier>(_db.Suppliers.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        // --- Các hàm thực thi Command ---
        private bool CanExecuteAdd(object obj)
        {
            // Chỉ cho phép "Thêm" nếu các trường cần thiết không rỗng
            return !string.IsNullOrEmpty(ProductName) && Price.HasValue && SelectedCategory != null;
        }

        private void ExecuteAdd(object obj)
        {
            try
            {
                var newProduct = new Product
                {
                    ProductName = ProductName,
                    Unit = Unit,
                    Price = Price,
                    CategoryID = SelectedCategory.CategoryID,
                    SupplierID = SelectedSupplier?.SupplierID // Cho phép NCC rỗng (Toán tử ?. an toàn)
                };

                _db.Products.Add(newProduct);
                _db.SaveChanges();

                ListProducts.Add(newProduct); // Thêm vào List trên UI
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm sản phẩm: " + ex.Message);
            }
        }

        private bool CanExecuteUpdateOrDelete(object obj)
        {
            // Chỉ cho phép "Sửa" / "Xóa" khi đã chọn 1 dòng
            return SelectedProduct != null;
        }

        private void ExecuteUpdate(object obj)
        {
            try
            {
                // SelectedProduct là đối tượng đang được EF theo dõi
                // Chỉ cần cập nhật các thuộc tính của nó
                SelectedProduct.ProductName = ProductName;
                SelectedProduct.Unit = Unit;
                SelectedProduct.Price = Price;
                SelectedProduct.CategoryID = SelectedCategory.CategoryID;
                SelectedProduct.SupplierID = SelectedSupplier?.SupplierID;

                _db.SaveChanges(); // Lưu thay đổi

                // Tải lại danh sách (để refresh DataGrid)
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật sản phẩm: " + ex.Message);
            }
        }

        private void ExecuteDelete(object obj)
        {
            if (MessageBox.Show("Bạn có chắc muốn xóa sản phẩm này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;

            try
            {
                // Tìm đúng đối tượng trong DbContext (nếu nó chưa được theo dõi)
                var productInDb = _db.Products.Find(SelectedProduct.ProductID);
                if (productInDb != null)
                {
                    _db.Products.Remove(productInDb);
                    _db.SaveChanges();

                    ListProducts.Remove(SelectedProduct); // Xóa khỏi List trên UI
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                // Lỗi này thường xảy ra nếu sản phẩm đang được dùng trong 1 phiếu nhập/xuất (lỗi khóa ngoại)
                MessageBox.Show("Lỗi khi xóa sản phẩm. Có thể sản phẩm này đã được sử dụng trong một giao dịch. Chi tiết: " + ex.Message);
            }
        }

        private void ClearForm()
        {
            SelectedProduct = null;
            ProductName = "";
            Unit = "";
            Price = null;
            SelectedCategory = null;
            SelectedSupplier = null;
        }
    }
}