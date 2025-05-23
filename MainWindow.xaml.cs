using System.Windows;
using System.Windows.Controls;

namespace WpfApp14
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (MainFrame == null)
            {
                throw new System.InvalidOperationException("MainFrame not found.");
            }
        }
    }
}