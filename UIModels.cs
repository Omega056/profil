using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfApp14.UIModels
{
    public class QuestionUIModel : INotifyPropertyChanged
    {
        private string _questionText = string.Empty;
        private string _header = "Вопрос 1";
        private int _selectedTimerIndex;

        public QuestionUIModel()
        {
            Answers = [new(), new(), new(), new()];
        }

        public QuestionUIModel(int index)
        {
            Answers = [new(), new(), new(), new()];
            UpdateHeader(index);
        }

        public string Header
        {
            get => _header;
            set
            {
                _header = value;
                OnPropertyChanged();
            }
        }

        public string QuestionText
        {
            get => _questionText;
            set
            {
                _questionText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<AnswerUIModel> Answers { get; }

        public int SelectedTimerIndex
        {
            get => _selectedTimerIndex;
            set
            {
                _selectedTimerIndex = value;
                OnPropertyChanged();
            }
        }

        public void UpdateHeader(int index)
        {
            Header = $"Вопрос {index}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AnswerUIModel : INotifyPropertyChanged
    {
        private string _text = string.Empty;
        private bool _isCorrect;

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        public bool IsCorrect
        {
            get => _isCorrect;
            set
            {
                _isCorrect = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}