using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp14.Services;
using static WpfApp14.Services.DatabaseService;

namespace WpfApp14
{
    public partial class FindGamePage : Page
    {
        public FindGamePage()
        {
            InitializeComponent();
            LoadQuizzes();
        }

        private void LoadQuizzes()
        {
            QuizzesListView.ItemsSource = DatabaseService.GetAllQuizzes();
        }

        private void QuizzesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (QuizzesListView.SelectedItem is QuizInfo quiz)
                NavigateToGame(quiz.Id);
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (QuizzesListView.SelectedItem is not QuizInfo quiz)
            {
                MessageBox.Show("Пожалуйста, выберите викторину.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            NavigateToGame(quiz.Id);
        }

        private void DeleteQuizButton_Click(object sender, RoutedEventArgs e)
        {
            if (QuizzesListView.SelectedItem is not QuizInfo quiz)
            {
                MessageBox.Show("Пожалуйста, выберите викторину для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить викторину '{quiz.Title}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                DatabaseService.DeleteQuiz(quiz.Id);
                LoadQuizzes(); // Обновляем список викторин после удаления
            }
        }

        private void NavigateToGame(int quizId)
        {
            if (NavigationService is null)
            {
                MessageBox.Show("Навигация недоступна.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Получаем список вопросов по Id викторины
            var questions = DatabaseService.GetQuestionsForQuiz(quizId);
            if (questions == null || questions.Count == 0)
            {
                MessageBox.Show("В выбранной викторине нет вопросов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Переходим на страницу игры
            NavigationService.Navigate(new GamePage(questions));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}