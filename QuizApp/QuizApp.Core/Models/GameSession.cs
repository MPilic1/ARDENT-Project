using System;
using System.Collections.Generic;

namespace QuizApp.Core.Models
{
    public class GameSession
    {
        public int Id { get; set; }
        public string Code { get; set; } // Unique join code for the game
        public int QuizId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public GameStatus Status { get; set; }
        public virtual Quiz Quiz { get; set; }
        public virtual ICollection<Player> Players { get; set; }
        public virtual ICollection<PlayerAnswer> PlayerAnswers { get; set; }

        public GameSession()
        {
            Players = new List<Player>();
            PlayerAnswers = new List<PlayerAnswer>();
            StartTime = DateTime.UtcNow;
            Status = GameStatus.WaitingToStart;
            Code = GenerateGameCode();
        }

        private string GenerateGameCode()
        {
            // Generate a random 6-character code
            return new Random().Next(100000, 999999).ToString();
        }
    }

    public enum GameStatus
    {
        WaitingToStart,
        InProgress,
        Completed,
        Cancelled
    }
} 