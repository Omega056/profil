using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WpfApp14.Services;
using static WpfApp14.Services.DatabaseService;

namespace WpfApp14
{
    public partial class GamePage : Page
    {
        private readonly List<QuestionDataModel> _questions;
        private int _currentIndex;
        private int _score;
        private int _remainingSeconds;
        private readonly DispatcherTimer _timer;

        public GamePage(List<QuestionDataModel> questions)
        {
            InitializeComponent();
            _questions = questions;
            _currentIndex = 0;
            _score = 0;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += TimerTick;

            ShowNextQuestion();
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            if (--_remainingSeconds <= 0)
            {
                _timer.Stop();
                ProcessAnswer(null);
                return;
            }
            TimerTextBlock.Text = _remainingSeconds.ToString();
        }

        private void ShowNextQuestion()
        {
            if (_currentIndex >= _questions.Count)
            {
                _timer.Stop();
                NavigationService?.Navigate(new ResultPage(_score, _questions.Count - _score));
                return;
            }

            var q = _questions[_currentIndex];
            QuestionTextBlock.Text = q.Text;
            _remainingSeconds = q.TimerSeconds;
            TimerTextBlock.Text = _remainingSeconds.ToString();

            var buttons = new[] { AnswerButton1, AnswerButton2, AnswerButton3, AnswerButton4 };
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i < q.Answers.Count)
                {
                    var a = q.Answers[i];
                    buttons[i].Content = a.Text;
                    buttons[i].Tag = a.IsCorrect;
                    buttons[i].Visibility = Visibility.Visible;
                    buttons[i].IsEnabled = true;
                    buttons[i].Background = System.Windows.Media.Brushes.White;
                }
                else
                {
                    buttons[i].Visibility = Visibility.Collapsed;
                }
            }

            _timer.Start();
        }

        private void OnAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is bool isCorrect)
            {
                _timer.Stop();
                ProcessAnswer(isCorrect);
            }
        }

        private void ProcessAnswer(bool? isCorrect)
        {
            if (isCorrect == true)
                _score++;

            var buttons = new[] { AnswerButton1, AnswerButton2, AnswerButton3, AnswerButton4 };
            foreach (var btn in buttons.Where(b => b.Visibility == Visibility.Visible))
            {
                bool correct = btn.Tag is bool c && c;
                btn.Background = correct
                    ? System.Windows.Media.Brushes.LightGreen
                    : System.Windows.Media.Brushes.LightCoral;
                btn.IsEnabled = false;
            }

            _currentIndex++;
            var delay = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            delay.Tick += (_, _) =>
            {
                delay.Stop();
                ShowNextQuestion();
            };
            delay.Start();
        }

        private void OnQuit_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            NavigationService?.GoBack();
        }
    }
}