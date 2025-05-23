using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using WpfApp14.Models;
using WpfApp14.Services;

namespace WpfApp14
{
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
            Loaded += HomePage_Loaded; // Subscribe to Loaded event
            // Subscribe to NavigationService.Navigated event
            if (NavigationService != null)
            {
                NavigationService.Navigated += NavigationService_Navigated;
            }
        }

        private void HomePage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserProfile(); // Refresh user profile when page is loaded
        }

        private void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            LoadUserProfile(); // Refresh user profile when navigated to
        }

        private void LoadUserProfile()
        {
            var user = ((App)Application.Current).CurrentUser;
            if (user is User profile)
            {
                // Load user from database if ID exists
                if (profile.Id > 0)
                {
                    var dbUser = DatabaseService.GetUser(profile.Id);
                    if (dbUser != null)
                    {
                        profile.Username = dbUser.Username;
                        profile.Password = dbUser.Password;
                        profile.IQ = dbUser.IQ;
                        profile.CorrectAnswerPercentage = dbUser.CorrectAnswerPercentage;
                        ((App)Application.Current).CurrentUser = dbUser; // Update CurrentUser to ensure consistency
                    }
                }
                ProfileUsernameTextBlock.Text = string.IsNullOrEmpty(profile.Username) ? "Гость" : profile.Username;
                ProfileIQTextBlock.Text = profile.IQ.ToString();
                ProfileCorrectAnswersTextBlock.Text = $"{profile.CorrectAnswerPercentage:F2}%";
            }
            else
            {
                ProfileUsernameTextBlock.Text = "Гость";
                ProfileIQTextBlock.Text = "0";
                ProfileCorrectAnswersTextBlock.Text = "0%";
            }
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