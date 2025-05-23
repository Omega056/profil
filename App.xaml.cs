using System.Windows;
using WpfApp14.Models;
using WpfApp14.Services;

namespace WpfApp14
{
    public partial class App : Application
    {
        public User CurrentUser { get; set; } // Remove nullable to ensure always initialized

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize the database
            DatabaseService.Initialize("quiz.db");

            // Create or load a default user
            CurrentUser = DatabaseService.GetUser(1) ?? new User
            {
                Id = 0, // Will be set by InsertOrUpdateUser
                Username = "Студент",
                Password = string.Empty,
                IQ = 100,
                CorrectAnswerPercentage = 0.0f
            };
            if (CurrentUser.Id == 0)
            {
                DatabaseService.InsertOrUpdateUser(CurrentUser);
            }
        }
    }
}