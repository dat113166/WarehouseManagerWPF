using System.Windows;
using WMS.ViewModels; // Thêm

namespace WMS
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel(); // Gán DataContext
        }
    }
}