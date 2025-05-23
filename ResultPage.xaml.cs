using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfApp14
{
    public partial class ResultPage : Page
    {
        public ResultPage(int correctCount, int incorrectCount)
        {
            InitializeComponent();
            CorrectCountText.Text = $"Правильных ответов: {correctCount}";
            IncorrectCountText.Text = $"Неправильных ответов: {incorrectCount}";
        }

        private void Home_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService?.Navigate(new HomePage());
        }
    }
}