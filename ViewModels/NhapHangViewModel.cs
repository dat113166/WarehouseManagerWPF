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
    public class NhapHangViewModel : ViewModelBase
    {
        // Lookups
        public ObservableCollection<Product> Products { get; } = ProductRepository.Products;
        public ObservableCollection<Supplier> Suppliers { get; } = new ObservableCollection<Supplier>();
        public ObservableCollection<Warehouse> Warehouses { get; } = new ObservableCollection<Warehouse>();

        // Receiving notes list
        public ObservableCollection<ReceivingNote> Notes { get; } = new ObservableCollection<ReceivingNote>();

        // Details for currently selected note (in UI)
        public ObservableCollection<ReceivingNoteDetail> Details { get; } = new ObservableCollection<ReceivingNoteDetail>();

        private ReceivingNote _selectedNote;
        public ReceivingNote SelectedNote
        {
            get => _selectedNote;
            set
            {
                _selectedNote = value; OnPropertyChanged();
                LoadDetailsForSelectedNote();
                if (_selectedNote != null)
                {
                    SelectedSupplier = Suppliers.FirstOrDefault(s => s.SupplierID == _selectedNote.SupplierID);
                    SelectedWarehouse = Warehouses.FirstOrDefault(w => w.WarehouseID == _selectedNote.WarehouseID);
                    NoteDate = _selectedNote.NoteDate;
                }
            }
        }

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

        // Fields for adding a detail line
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

        private ReceivingNoteDetail _selectedDetail;
        public ReceivingNoteDetail SelectedDetail
        {
            get => _selectedDetail;
            set { _selectedDetail = value; OnPropertyChanged(); }
        }

        // Commands
        public ICommand NewNoteCommand { get; }
        public ICommand SaveNoteCommand { get; }
        public ICommand UpdateNoteCommand { get; }
        public ICommand DeleteNoteCommand { get; }

        public ICommand AddDetailCommand { get; }
        public ICommand RemoveDetailCommand { get; }

        public NhapHangViewModel()
        {
            NewNoteCommand = new RelayCommand(o => NewNote());
            SaveNoteCommand = new RelayCommand(o => SaveNote(), o => SelectedSupplier != null && SelectedWarehouse != null && Details.Any());
            UpdateNoteCommand = new RelayCommand(o => UpdateNote(), o => SelectedNote != null);
            DeleteNoteCommand = new RelayCommand(o => DeleteNote(), o => SelectedNote != null);

            AddDetailCommand = new RelayCommand(o => AddDetail(), o => SelectedProductForDetail != null && DetailQuantity > 0 && DetailUnitPrice > 0);
            RemoveDetailCommand = new RelayCommand(o => RemoveDetail(), o => SelectedDetail != null);

            // Ensure products loaded
            if (!Products.Any()) ProductRepository.Load();

            LoadLookups();
            LoadNotes();
        }

        private void LoadLookups()
        {
            Suppliers.Clear();
            Warehouses.Clear();
            try
            {
                using (var db = new Model1())
                {
                    foreach (var s in db.Suppliers.OrderBy(s => s.SupplierName)) Suppliers.Add(s);
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
                    var list = db.ReceivingNotes.Include(r => r.ReceivingNoteDetails).OrderByDescending(r => r.NoteDate).ToList();
                    foreach (var n in list) Notes.Add(n);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi t?i phi?u nh?p: " + ex.Message);
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
                    var details = db.ReceivingNoteDetails.Where(d => d.NoteID == SelectedNote.NoteID).Include(d => d.Product).ToList();
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
            SelectedSupplier = null;
            SelectedWarehouse = null;
            NoteDate = DateTime.Now;
        }

        private void AddDetail()
        {
            if (SelectedProductForDetail == null) return;
            var det = new ReceivingNoteDetail
            {
                ProductID = SelectedProductForDetail.ProductID,
                Quantity = DetailQuantity,
                UnitPrice = DetailUnitPrice
            };
            // Fill navigation property for UI
            det.Product = SelectedProductForDetail;
            Details.Add(det);

            // reset detail fields
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
            if (SelectedSupplier == null || SelectedWarehouse == null || !Details.Any())
            {
                MessageBox.Show("Phi?u ph?i có nhà cung c?p, kho và ít nh?t 1 chi ti?t.");
                return;
            }

            try
            {
                using (var db = new Model1())
                {
                    var note = new ReceivingNote
                    {
                        NoteDate = NoteDate,
                        SupplierID = SelectedSupplier.SupplierID,
                        WarehouseID = SelectedWarehouse.WarehouseID,
                        TotalAmount = Details.Sum(d => d.UnitPrice * d.Quantity)
                    };

                    db.ReceivingNotes.Add(note);
                    db.SaveChanges(); // to get NoteID

                    foreach (var d in Details)
                    {
                        var det = new ReceivingNoteDetail
                        {
                            NoteID = note.NoteID,
                            ProductID = d.ProductID,
                            Quantity = d.Quantity,
                            UnitPrice = d.UnitPrice
                        };
                        db.ReceivingNoteDetails.Add(det);
                    }

                    db.SaveChanges();
                }

                LoadNotes();
                NewNote();
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi l?u phi?u nh?p: " + ex.Message);
            }
        }

        private void UpdateNote()
        {
            if (SelectedNote == null) return;
            try
            {
                using (var db = new Model1())
                {
                    // Update header
                    var note = db.ReceivingNotes.Find(SelectedNote.NoteID);
                    if (note == null) { MessageBox.Show("Phi?u không t?n t?i trong DB."); return; }

                    note.NoteDate = NoteDate;
                    note.SupplierID = SelectedNote.SupplierID = SelectedSupplier?.SupplierID ?? note.SupplierID;
                    note.WarehouseID = SelectedNote.WarehouseID = SelectedWarehouse?.WarehouseID ?? note.WarehouseID;
                    note.TotalAmount = Details.Sum(d => d.UnitPrice * d.Quantity);

                    // Remove existing details
                    var existing = db.ReceivingNoteDetails.Where(d => d.NoteID == note.NoteID).ToList();
                    foreach (var exd in existing) db.ReceivingNoteDetails.Remove(exd);

                    // Add current details
                    foreach (var d in Details)
                    {
                        var det = new ReceivingNoteDetail
                        {
                            NoteID = note.NoteID,
                            ProductID = d.ProductID,
                            Quantity = d.Quantity,
                            UnitPrice = d.UnitPrice
                        };
                        db.ReceivingNoteDetails.Add(det);
                    }

                    db.SaveChanges();
                }

                LoadNotes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi c?p nh?t phi?u nh?p: " + ex.Message);
                LoadNotes();
            }
        }

        private void DeleteNote()
        {
            if (SelectedNote == null) return;
            var r = MessageBox.Show($"B?n có ch?c ch?n mu?n xóa phi?u nh?p #{SelectedNote.NoteID}?", "Xác nh?n", MessageBoxButton.YesNo);
            if (r != MessageBoxResult.Yes) return;

            try
            {
                using (var db = new Model1())
                {
                    var note = db.ReceivingNotes.Find(SelectedNote.NoteID);
                    if (note != null)
                    {
                        // delete details first
                        var details = db.ReceivingNoteDetails.Where(d => d.NoteID == note.NoteID).ToList();
                        foreach (var d in details) db.ReceivingNoteDetails.Remove(d);

                        db.ReceivingNotes.Remove(note);
                        db.SaveChanges();
                    }
                }

                LoadNotes();
                NewNote();
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi xóa phi?u nh?p: " + ex.Message);
            }
        }
    }
}
