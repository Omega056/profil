using System.Windows;
using WpfApp14.Models;
using WpfApp14.Services;

namespace WpfApp14
{
    public partial class App : Application
    {
        public User? CurrentUser { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize the database
            DatabaseService.Initialize("quiz_database.db");

            // Create and show the main window with LoginPage in the Frame
            var mainWindow = new MainWindow();
            mainWindow.MainFrame.Navigate(new LoginPage());
            mainWindow.Show();
        }
    }
}