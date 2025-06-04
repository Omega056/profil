using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp14
{
    public partial class StorePage : Page
    {
        public StorePage()
        {
            InitializeComponent();

            // Инициализация тестовых данных для магазина
            var items = new List<Item>
            {
                new Item
                {
                    ImagePath = "pack://application:,,,/Images/item1.png",
                    Title = "Товар 1",
                    Price = "500 тг",
                    TooltipText = "500 тг"
                },
                new Item
                {
                    ImagePath = "pack://application:,,,/Images/item2.png",
                    Title = "Товар 2",
                    Price = "1000 тг",
                    TooltipText = "50% убранных вопросов 1000 тг"
                }
            };

            // Установка данных для ItemsControl
            ItemsControl.ItemsSource = items;
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            // Обработка покупки
            Button buyButton = sender as Button;
            if (buyButton != null)
            {
                var item = buyButton.DataContext as Item;
                if (item != null)
                {
                    MessageBox.Show($"Вы купили: {item.Title} за {item.Price}", "Покупка", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        public class Item
        {
            public string ImagePath { get; set; }
            public string Title { get; set; }
            public string Price { get; set; }
            public string TooltipText { get; set; }
        }
    }
}