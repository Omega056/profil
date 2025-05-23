namespace WpfApp14.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int IQ { get; set; }
        public float CorrectAnswerPercentage { get; set; }
    }
}