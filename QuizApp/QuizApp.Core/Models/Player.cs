using System;

namespace QuizApp.Core.Models
{
    public class Player
    {
        public int Id { get; set; }
        public int GameSessionId { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public DateTime JoinedAt { get; set; }
        public int? UserId { get; set; }
        public virtual GameSession GameSession { get; set; }
        public virtual User User { get; set; }

        public Player()
        {
            JoinedAt = DateTime.UtcNow;
            Score = 0;
        }
    }
} 