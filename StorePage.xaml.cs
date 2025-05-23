using System.Windows;
using System.Windows.Controls;

namespace WpfApp14
{
    public partial class StorePage : Page
    {
        public StorePage()
        {
            InitializeComponent();
        }
        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            // Здесь можно обработать покупку
            Button buyButton = sender as Button;
            if (buyButton != null)
            {
                var item = buyButton.DataContext;
                MessageBox.Show("Вы купили: " + item?.ToString(), "Покупка", MessageBoxButton.OK, MessageBoxImage.Information);

                // Здесь вы можете добавить более точную обработку item,
                // например, приведение к вашему классу товара и списание баланса
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}