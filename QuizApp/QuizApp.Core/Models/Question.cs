using System.Collections.Generic;

namespace QuizApp.Core.Models
{
    public class Question
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string Text { get; set; }
        public int Points { get; set; }
        public int TimeLimit { get; set; } // Time limit in seconds
        public QuestionType Type { get; set; }
        public virtual Quiz Quiz { get; set; }
        public virtual ICollection<Answer> Answers { get; set; }

        public Question()
        {
            Answers = new List<Answer>();
            Points = 1000; // Default points
            TimeLimit = 20; // Default 20 seconds
        }
    }

    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse
    }
} 