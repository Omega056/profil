using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfApp14
{
    public partial class ResultPage : Page
    {
        public ResultPage(int correctCount, int incorrectCount)
        {
            InitializeComponent();
            CorrectCountText.Text = $"���������� �������: {correctCount}";
            IncorrectCountText.Text = $"������������ �������: {incorrectCount}";
        }

        private void Home_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService?.Navigate(new HomePage());
        }
    }
}