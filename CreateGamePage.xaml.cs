using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using WpfApp14.Services;
using WpfApp14.UIModels;

namespace WpfApp14
{
    public partial class CreateGamePage : Page
    {
        public ObservableCollection<QuestionUIModel> Questions { get; set; } = new();

        public CreateGamePage()
        {
            InitializeComponent();
            DataContext = this;
            AdjustQuestions(5);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
            => NavigationService?.GoBack();

        private void QuestionsCountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (QuestionsCountCombo.SelectedItem is ComboBoxItem item
                && int.TryParse(item.Content.ToString(), out int newCount))
                AdjustQuestions(newCount);
        }

        private void AdjustQuestions(int count)
        {
            while (Questions.Count < count)
                Questions.Add(new QuestionUIModel(Questions.Count + 1));
            while (Questions.Count > count)
                Questions.RemoveAt(Questions.Count - 1);
            for (int i = 0; i < Questions.Count; i++)
                Questions[i].UpdateHeader(i + 1);
        }

        private void AddQuizButton_Click(object sender, RoutedEventArgs e)
        {
            // 1) Ловим факт нажатия
            MessageBox.Show("DEBUG: вошли в AddQuizButton_Click", "DEBUG");

            // 2) Валидация заголовка
            string title = QuizTitleBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Пожалуйста, введите название викторины.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3) Валидация хотя бы одного вопроса
            if (Questions.All(q => string.IsNullOrWhiteSpace(q.QuestionText)))
            {
                MessageBox.Show("Нужно хотя бы один вопрос с текстом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 4) Сохраняем викторину
                int quizId = DatabaseService.InsertQuiz(title);

                // 5) Сохраняем вопросы и ответы
                foreach (var q in Questions)
                {
                    var answers = q.Answers.Select(a => (a.Text, a.IsCorrect)).ToArray();
                    int timer = q.SelectedTimerIndex switch { 0 => 10, 1 => 20, 2 => 30, _ => 10 };
                    DatabaseService.InsertQuestion(quizId, q.QuestionText, timer, answers);
                }

                // 6) Успех
                MessageBox.Show($"Викторина \"{title}\" сохранена (ID={quizId}).", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Можно перейти на страницу списка или очистить форму:
                NavigationService?.Navigate(new FindGamePage());
            }
            catch (Exception ex)
            {
                // 7) Выводим ошибку, если что-то пошло не так
                MessageBox.Show(ex.ToString(), "Ошибка при сохранении викторины", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
