using System.Windows;

namespace WpfApp14
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new HomePage());
        }
    }
}
