using System.Collections.ObjectModel; // <== THÊM DÒNG NÀY
using System.Linq; // <== THÊM DÒNG NÀY
using WMS.Model; // <== THÊM DÒNG NÀY
using WMS.ViewModels.Base;

namespace WMS.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        public ObservableCollection<object> RecentTransactions { get; set; }
        private Model1 _db;

        public DashboardViewModel()
        {
            _db = new Model1();
            LoadDashboard();
        }

        private void LoadDashboard()
        {
            // Lấy 5 phiếu nhập
            var receivings = _db.ReceivingNotes
                                .OrderByDescending(n => n.NoteDate)
                                .Take(5)
                                .Select(n => new { Type = "Nhập hàng", Date = n.NoteDate, Amount = n.TotalAmount });
            // Lấy 5 phiếu xuất
            var shippings = _db.ShippingNotes
                               .OrderByDescending(n => n.NoteDate)
                               .Take(5)
                               .Select(n => new { Type = "Xuất hàng", Date = n.NoteDate, Amount = n.TotalAmount });

            // Gộp lại và sắp xếp
            var allTrans = receivings.Concat(shippings)
                                     .OrderByDescending(t => t.Date)
                                     .Take(10);

            // Thêm .ToList() để thực thi truy vấn ngay lập tức
            RecentTransactions = new ObservableCollection<object>(allTrans.ToList());
        }
    }
}