using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp14
{
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Навигация к ProfilePage
        }

        private void SupportButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Навигация к SupportPage
        }
    }
}