using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using WpfApp14.Services;
using WpfApp14.UIModels;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace WpfApp14
{
    public partial class CreateGamePage : Page
    {
        public ObservableCollection<QuestionUIModel> Questions { get; set; } = [];
        public ObservableCollection<ChatMessage> ChatMessages { get; set; } = [];
        private bool _isChatPanelOpen = false;
        private readonly HttpClient _httpClient = new();
        private const string GeminiApiKey = "AIzaSyBxzmF7owWmJMehO-GaxXEp-AkLC5Vp9YA";
        private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key=";

        public bool IsChatPanelOpen
        {
            get => _isChatPanelOpen;
            set
            {
                _isChatPanelOpen = value;
                AIChatPanel.Width = _isChatPanelOpen ? new GridLength(30, GridUnitType.Star) : new GridLength(0);
            }
        }

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
            string title = QuizTitleBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Пожалуйста, введите название викторины.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Questions.All(q => string.IsNullOrWhiteSpace(q.QuestionText)))
            {
                MessageBox.Show("Нужно хотя бы один вопрос с текстом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int quizId = DatabaseService.InsertQuiz(title);
                foreach (var q in Questions)
                {
                    var answers = q.Answers.Select(a => (a.Text, a.IsCorrect)).ToArray();
                    int timer = q.SelectedTimerIndex switch { 0 => 10, 1 => 20, 2 => 30, _ => 10 };
                    DatabaseService.InsertQuestion(quizId, q.QuestionText, timer, answers);
                }
                MessageBox.Show($"Викторина \"{title}\" сохранена (ID={quizId}).", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService?.Navigate(new FindGamePage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка при сохранении викторины", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleAIChatButton_Click(object sender, RoutedEventArgs e)
        {
            IsChatPanelOpen = !IsChatPanelOpen;
            if (IsChatPanelOpen)
            {
                ChatMessages.Clear();
                ChatMessages.Add(new ChatMessage { Sender = "AI", Message = "Привет! Я здесь, чтобы помочь тебе создать викторину. Что тебе нужно?" });
            }
        }

        private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            string userMessage = ChatInputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(userMessage))
                return;

            ChatMessages.Add(new ChatMessage { Sender = "User", Message = userMessage });
            ChatInputBox.Text = string.Empty;

            try
            {
                string aiResponse = await GetGeminiAIResponse(userMessage);
                ChatMessages.Add(new ChatMessage { Sender = "AI", Message = aiResponse });
            }
            catch (Exception ex)
            {
                ChatMessages.Add(new ChatMessage { Sender = "AI", Message = $"Ошибка: {ex.Message}" });
            }
        }

        private async Task<string> GetGeminiAIResponse(string prompt)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{GeminiApiUrl}{GeminiApiKey}", content);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Не удалось получить ответ от Gemini AI.");

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? "Извини, я не смог сгенерировать ответ.";
        }
    }

    public class ChatMessage
    {
        public string? Sender { get; set; }
        public string? Message { get; set; }
    }
}