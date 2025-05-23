using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WpfApp14.Models;
using WpfApp14.Services;

namespace WpfApp14
{
    public partial class RegistrationPage : Page
    {
        public RegistrationPage()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            string confirmPassword = ConfirmPasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Check if username already exists
                if (DatabaseService.GetUserByUsername(username) != null)
                {
                    MessageBox.Show("Пользователь с таким именем уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create new user
                var newUser = new User
                {
                    Username = username,
                    Password = password,
                    IQ = 0,
                    CorrectAnswerPercentage = 0
                };

                // Insert user into database
                int userId = DatabaseService.InsertOrUpdateUser(newUser);
                newUser.Id = userId;

                MessageBox.Show("Регистрация успешна! Пожалуйста, войдите.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService?.Navigate(new LoginPage());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LoginPage());
        }
    }
}