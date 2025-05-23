using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WpfApp14.Models;
using WpfApp14.Services;

namespace WpfApp14
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var user = DatabaseService.GetUserByUsername(username);
                if (user == null || user.Password != password)
                {
                    MessageBox.Show("Неверное имя пользователя или пароль.", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Set CurrentUser and navigate to HomePage
                ((App)Application.Current).CurrentUser = user;
                NavigationService?.Navigate(new HomePage());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegistrationPage());
        }
    }
}