using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WMS.Model;
using WMS.Utils;
using WMS.ViewModels.Base;
using WMS.Services;

namespace WMS.ViewModels
{
    public class ProductViewModel : ViewModelBase
    {
        public ObservableCollection<Product> Products { get; } = ProductRepository.Products;

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get { return _selectedProduct; }
            set { _selectedProduct = value; OnPropertyChanged(); }
        }

        // Fields for creating a new product
        private string _newProductName;
        public string NewProductName
        {
            get => _newProductName;
            set { _newProductName = value; OnPropertyChanged(); }
        }

        private string _newUnit;
        public string NewUnit
        {
            get => _newUnit;
            set { _newUnit = value; OnPropertyChanged(); }
        }

        private decimal? _newPrice;
        public decimal? NewPrice
        {
            get => _newPrice;
            set { _newPrice = value; OnPropertyChanged(); }
        }

        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearNewCommand { get; }

        public ProductViewModel()
        {
            AddCommand = new RelayCommand(o => AddProduct());
            UpdateCommand = new RelayCommand(o => UpdateProduct(), o => SelectedProduct != null);
            DeleteCommand = new RelayCommand(o => DeleteProduct(), o => SelectedProduct != null);
            ClearNewCommand = new RelayCommand(o => ClearNewFields());

            // Load shared product list once
            if (!Products.Any())
                ProductRepository.Load();
        }

        private void AddProduct()
        {
            if (string.IsNullOrWhiteSpace(NewProductName))
            {
                MessageBox.Show("Tên sản phẩm không được để trống.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var p = new Product
            {
                ProductName = NewProductName.Trim(),
                Unit = string.IsNullOrWhiteSpace(NewUnit) ? "" : NewUnit.Trim(),
                Price = NewPrice
            };

            ProductRepository.Add(p);
            ClearNewFields();
        }

        private void UpdateProduct()
        {
            if (SelectedProduct == null) return;

            if (string.IsNullOrWhiteSpace(SelectedProduct.ProductName))
            {
                MessageBox.Show("Tên sản phẩm không được để trống.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ProductRepository.Update(SelectedProduct);
            OnPropertyChanged(nameof(Products));
        }

        private void DeleteProduct()
        {
            if (SelectedProduct == null) return;

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa '{SelectedProduct.ProductName}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            ProductRepository.Delete(SelectedProduct);
            SelectedProduct = null;
        }

        private void ClearNewFields()
        {
            NewProductName = string.Empty;
            NewUnit = string.Empty;
            NewPrice = null;
        }
    }
}