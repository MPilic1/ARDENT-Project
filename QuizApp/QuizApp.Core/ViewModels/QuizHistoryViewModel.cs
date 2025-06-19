using System;

namespace QuizApp.Core.ViewModels
{
    public class QuizHistoryViewModel
    {
        public int GameSessionId { get; set; }
        public string GameCode { get; set; }
        public string QuizTitle { get; set; }
        public DateTime PlayedAt { get; set; }
        public int FinalScore { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public TimeSpan Duration { get; set; }
        public string Status { get; set; }
    }
} 