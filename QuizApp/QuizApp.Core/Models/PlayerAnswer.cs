using System;

namespace QuizApp.Core.Models
{
    public class PlayerAnswer
    {
        public int Id { get; set; }
        public int GameSessionId { get; set; }
        public int PlayerId { get; set; }
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
        public DateTime AnsweredAt { get; set; }
        public int ScoreEarned { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public virtual GameSession GameSession { get; set; }
        public virtual Player Player { get; set; }
        public virtual Question Question { get; set; }
        public virtual Answer Answer { get; set; }

        public PlayerAnswer()
        {
            AnsweredAt = DateTime.UtcNow;
        }
    }
} 