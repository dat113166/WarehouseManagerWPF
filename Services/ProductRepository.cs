using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WMS.Model;

namespace WMS.Services
{
    public static class ProductRepository
    {
        // Shared observable collection used by view models
        public static ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();

        public static void Load()
        {
            Products.Clear();
            try
            {
                using (var db = new Model1())
                {
                    var list = db.Products.OrderBy(p => p.ProductName).ToList();
                    foreach (var prod in list)
                    {
                        Products.Add(prod);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không th? t?i s?n ph?m: " + ex.Message, "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void Add(Product p)
        {
            try
            {
                using (var db = new Model1())
                {
                    var entity = new Product
                    {
                        ProductName = p.ProductName,
                        Unit = p.Unit,
                        Price = p.Price,
                        CategoryID = p.CategoryID,
                        SupplierID = p.SupplierID
                    };

                    db.Products.Add(entity);
                    db.SaveChanges();

                    // Add saved entity (with generated ID) to shared collection
                    Products.Add(entity);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi thêm s?n ph?m: " + ex.Message, "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void Update(Product p)
        {
            try
            {
                using (var db = new Model1())
                {
                    db.Products.Attach(p);
                    db.Entry(p).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                // Optionally refresh collection to reflect DB
                // Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi c?p nh?t s?n ph?m: " + ex.Message, "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void Delete(Product p)
        {
            try
            {
                using (var db = new Model1())
                {
                    db.Products.Attach(p);
                    db.Products.Remove(p);
                    db.SaveChanges();
                }

                Products.Remove(p);
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi xóa s?n ph?m: " + ex.Message, "L?i", MessageBoxButton.OK, MessageBoxImage.Error);
                Load();
            }
        }
    }
}
