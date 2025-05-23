using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfApp14
{
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SettingsPage());
        }

        private void StoreButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new StorePage());
        }

        private void CreateGameButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CreateGamePage());
        }

        private void FindGameButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new FindGamePage());
        }

        private void ProfileBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           
            NavigationService?.Navigate(new ProfilePage());
        }
    }
}