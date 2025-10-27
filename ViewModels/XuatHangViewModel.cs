using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Data.Entity;
using WMS.Model;
using WMS.Utils;
using WMS.ViewModels.Base;
using WMS.Services;

namespace WMS.ViewModels
{
    public class XuatHangViewModel : ViewModelBase
    {
        // Lookups
        public ObservableCollection<Product> Products { get; } = ProductRepository.Products;
        public ObservableCollection<Customer> Customers { get; } = new ObservableCollection<Customer>();
        public ObservableCollection<Warehouse> Warehouses { get; } = new ObservableCollection<Warehouse>();

        // Shipping notes list
        public ObservableCollection<ShippingNote> Notes { get; } = new ObservableCollection<ShippingNote>();

        // Details for selected note
        public ObservableCollection<ShippingNoteDetail> Details { get; } = new ObservableCollection<ShippingNoteDetail>();

        private ShippingNote _selectedNote;
        public ShippingNote SelectedNote
        {
            get => _selectedNote;
            set
            {
                _selectedNote = value; OnPropertyChanged();
                LoadDetailsForSelectedNote();
                if (_selectedNote != null)
                {
                    SelectedCustomer = Customers.FirstOrDefault(c => c.CustomerID == _selectedNote.CustomerID);
                    SelectedWarehouse = Warehouses.FirstOrDefault(w => w.WarehouseID == _selectedNote.WarehouseID);
                    NoteDate = _selectedNote.NoteDate;
                }
            }
        }

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

        private Product _selectedProductForDetail;
        public Product SelectedProductForDetail
        {
            get => _selectedProductForDetail;
            set { _selectedProductForDetail = value; OnPropertyChanged(); }
        }

        private int _detailQuantity;
        public int DetailQuantity
        {
            get => _detailQuantity;
            set { _detailQuantity = value; OnPropertyChanged(); }
        }

        private decimal _detailUnitPrice;
        public decimal DetailUnitPrice
        {
            get => _detailUnitPrice;
            set { _detailUnitPrice = value; OnPropertyChanged(); }
        }

        private ShippingNoteDetail _selectedDetail;
        public ShippingNoteDetail SelectedDetail
        {
            get => _selectedDetail;
            set { _selectedDetail = value; OnPropertyChanged(); }
        }

        public ICommand NewNoteCommand { get; }
        public ICommand SaveNoteCommand { get; }
        public ICommand UpdateNoteCommand { get; }
        public ICommand DeleteNoteCommand { get; }

        public ICommand AddDetailCommand { get; }
        public ICommand RemoveDetailCommand { get; }

        public XuatHangViewModel()
        {
            NewNoteCommand = new RelayCommand(o => NewNote());
            SaveNoteCommand = new RelayCommand(o => SaveNote(), o => SelectedCustomer != null && SelectedWarehouse != null && Details.Any());
            UpdateNoteCommand = new RelayCommand(o => UpdateNote(), o => SelectedNote != null);
            DeleteNoteCommand = new RelayCommand(o => DeleteNote(), o => SelectedNote != null);

            AddDetailCommand = new RelayCommand(o => AddDetail(), o => SelectedProductForDetail != null && DetailQuantity > 0 && DetailUnitPrice > 0);
            RemoveDetailCommand = new RelayCommand(o => RemoveDetail(), o => SelectedDetail != null);

            if (!Products.Any()) ProductRepository.Load();

            LoadLookups();
            LoadNotes();
        }

        private void LoadLookups()
        {
            Customers.Clear();
            Warehouses.Clear();
            try
            {
                using (var db = new Model1())
                {
                    foreach (var c in db.Customers.OrderBy(c => c.CustomerName)) Customers.Add(c);
                    foreach (var w in db.Warehouses.OrderBy(w => w.WarehouseName)) Warehouses.Add(w);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi t?i danh m?c: " + ex.Message);
            }
        }

        private void LoadNotes()
        {
            Notes.Clear();
            try
            {
                using (var db = new Model1())
                {
                    var list = db.ShippingNotes.Include(r => r.ShippingNoteDetails).OrderByDescending(r => r.NoteDate).ToList();
                    foreach (var n in list) Notes.Add(n);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi t?i phi?u xu?t: " + ex.Message);
            }
        }

        private void LoadDetailsForSelectedNote()
        {
            Details.Clear();
            if (SelectedNote == null) return;
            try
            {
                using (var db = new Model1())
                {
                    var details = db.ShippingNoteDetails.Where(d => d.NoteID == SelectedNote.NoteID).Include(d => d.Product).ToList();
                    foreach (var d in details) Details.Add(d);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi t?i chi ti?t: " + ex.Message);
            }
        }

        private void NewNote()
        {
            SelectedNote = null;
            Details.Clear();
            SelectedCustomer = null;
            SelectedWarehouse = null;
            NoteDate = DateTime.Now;
        }

        private void AddDetail()
        {
            if (SelectedProductForDetail == null) return;
            var det = new ShippingNoteDetail
            {
                ProductID = SelectedProductForDetail.ProductID,
                Quantity = DetailQuantity,
                UnitPrice = DetailUnitPrice
            };
            det.Product = SelectedProductForDetail;
            Details.Add(det);

            SelectedProductForDetail = null;
            DetailQuantity = 0;
            DetailUnitPrice = 0;
        }

        private void RemoveDetail()
        {
            if (SelectedDetail == null) return;
            Details.Remove(SelectedDetail);
            SelectedDetail = null;
        }

        private void SaveNote()
        {
            if (SelectedCustomer == null || SelectedWarehouse == null || !Details.Any())
            {
                MessageBox.Show("Phi?u ph?i có khách hàng, kho và ít nh?t 1 chi ti?t.");
                return;
            }

            try
            {
                using (var db = new Model1())
                {
                    var note = new ShippingNote
                    {
                        NoteDate = NoteDate,
                        CustomerID = SelectedCustomer.CustomerID,
                        WarehouseID = SelectedWarehouse.WarehouseID,
                        TotalAmount = Details.Sum(d => d.UnitPrice * d.Quantity)
                    };

                    db.ShippingNotes.Add(note);
                    db.SaveChanges();

                    foreach (var d in Details)
                    {
                        var det = new ShippingNoteDetail
                        {
                            NoteID = note.NoteID,
                            ProductID = d.ProductID,
                            Quantity = d.Quantity,
                            UnitPrice = d.UnitPrice
                        };
                        db.ShippingNoteDetails.Add(det);
                    }

                    db.SaveChanges();
                }

                LoadNotes();
                NewNote();
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi l?u phi?u xu?t: " + ex.Message);
            }
        }

        private void UpdateNote()
        {
            if (SelectedNote == null) return;
            try
            {
                using (var db = new Model1())
                {
                    var note = db.ShippingNotes.Find(SelectedNote.NoteID);
                    if (note == null) { MessageBox.Show("Phi?u không t?n t?i trong DB."); return; }

                    note.NoteDate = NoteDate;
                    note.CustomerID = SelectedNote.CustomerID = SelectedCustomer?.CustomerID ?? note.CustomerID;
                    note.WarehouseID = SelectedNote.WarehouseID = SelectedWarehouse?.WarehouseID ?? note.WarehouseID;
                    note.TotalAmount = Details.Sum(d => d.UnitPrice * d.Quantity);

                    var existing = db.ShippingNoteDetails.Where(d => d.NoteID == note.NoteID).ToList();
                    foreach (var exd in existing) db.ShippingNoteDetails.Remove(exd);

                    foreach (var d in Details)
                    {
                        var det = new ShippingNoteDetail
                        {
                            NoteID = note.NoteID,
                            ProductID = d.ProductID,
                            Quantity = d.Quantity,
                            UnitPrice = d.UnitPrice
                        };
                        db.ShippingNoteDetails.Add(det);
                    }

                    db.SaveChanges();
                }

                LoadNotes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi c?p nh?t phi?u xu?t: " + ex.Message);
                LoadNotes();
            }
        }

        private void DeleteNote()
        {
            if (SelectedNote == null) return;
            var r = MessageBox.Show($"B?n có ch?c ch?n mu?n xóa phi?u xu?t #{SelectedNote.NoteID}?", "Xác nh?n", MessageBoxButton.YesNo);
            if (r != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new Model1())
                {
                    var note = db.ShippingNotes.Find(SelectedNote.NoteID);
                    if (note != null)
                    {
                        var details = db.ShippingNoteDetails.Where(d => d.NoteID == note.NoteID).ToList();
                        foreach (var d in details) db.ShippingNoteDetails.Remove(d);

                        db.ShippingNotes.Remove(note);
                        db.SaveChanges();
                    }
                }

                LoadNotes();
                NewNote();
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi xóa phi?u xu?t: " + ex.Message);
            }
        }
    }
}
