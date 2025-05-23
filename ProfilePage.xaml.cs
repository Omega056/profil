using System.Windows;
using System.Windows.Controls;
using WpfApp14.Models;
using WpfApp14.Services;

namespace WpfApp14
{
    public partial class ProfilePage : Page
    {
        private bool _isEditingUsername;

        public ProfilePage()
        {
            InitializeComponent();
            LoadUserProfile();
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
                        ((App)Application.Current).CurrentUser = dbUser; // Update CurrentUser
                    }
                }
                UsernameTextBlock.Text = string.IsNullOrEmpty(profile.Username) ? "Гость" : profile.Username;
                IQTextBlock.Text = profile.IQ.ToString();
                CorrectAnswersTextBlock.Text = $"{profile.CorrectAnswerPercentage:F2}%";
                UsernameTextBox.Text = profile.Username;
            }
            else
            {
                UsernameTextBlock.Text = "Гость";
                IQTextBlock.Text = "0";
                CorrectAnswersTextBlock.Text = "0%";
            }
        }

        private void EditUsernameButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditingUsername)
            {
                UsernameTextBlock.Visibility = Visibility.Collapsed;
                UsernameTextBox.Visibility = Visibility.Visible;
                UsernameTextBox.Focus(); // Set focus to textbox for immediate editing
                EditUsernameButton.Content = "Сохранить";
                _isEditingUsername = true;
            }
            else
            {
                string newUsername = UsernameTextBox.Text.Trim();
                if (string.IsNullOrEmpty(newUsername))
                {
                    MessageBox.Show("Имя не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    UsernameTextBox.Focus(); // Keep focus on textbox to allow retry
                    return;
                }

                if (((App)Application.Current).CurrentUser is User user)
                {
                    user.Username = newUsername;
                    // Save to database
                    DatabaseService.InsertOrUpdateUser(user);
                    UsernameTextBlock.Text = newUsername;
                    UsernameTextBlock.Visibility = Visibility.Visible;
                    UsernameTextBox.Visibility = Visibility.Collapsed;
                    EditUsernameButton.Content = "Изменить имя";
                    _isEditingUsername = false;
                    // Show success message
                    MessageBox.Show("Имя успешно обновлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).CurrentUser = null;
            NavigationService?.Navigate(new HomePage());
        }
    }
}