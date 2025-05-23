using System.Windows;
using System.Windows.Controls;
using WpfApp14.Models;

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
                UsernameTextBlock.Text = profile.Username;
                IQTextBlock.Text = profile.IQ.ToString();
                CorrectAnswersTextBlock.Text = $"{profile.CorrectAnswerPercentage:F2}%";
                UsernameTextBox.Text = profile.Username;
            }
            else
            {
               
            }
        }

        private void EditUsernameButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditingUsername)
            {
                UsernameTextBlock.Visibility = Visibility.Collapsed;
                UsernameTextBox.Visibility = Visibility.Visible;
                EditUsernameButton.Content = "Сохранить";
                _isEditingUsername = true;
            }
            else
            {
                string newUsername = UsernameTextBox.Text.Trim();
                if (string.IsNullOrEmpty(newUsername))
                {
                    MessageBox.Show("Имя не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (((App)Application.Current).CurrentUser is User user)
                {
                    user.Username = newUsername;
                    UsernameTextBlock.Text = newUsername;
                    UsernameTextBlock.Visibility = Visibility.Visible;
                    UsernameTextBox.Visibility = Visibility.Collapsed;
                    EditUsernameButton.Content = "Изменить имя";
                    _isEditingUsername = false;
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