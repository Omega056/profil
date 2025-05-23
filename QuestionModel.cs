using System.Collections.Generic;

namespace WpfApp14
{
    public class QuestionModel
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public List<AnswerModel> Answers { get; set; }
    }

    public class AnswerModel
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string AnswerText { get; set; }
        public bool IsCorrect { get; set; }
    }
}
