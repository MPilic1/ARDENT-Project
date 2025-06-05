using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizApp.Core.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        [Required(ErrorMessage = "Creator information is required")]
        public string CreatedBy { get; set; }
        
        public bool IsActive { get; set; }
        
        [Required(ErrorMessage = "Time limit is required")]
        public TimeSpan TimeLimit { get; set; }
        
        public virtual ICollection<Question> Questions { get; set; }
        
        public Quiz()
        {
            Questions = new List<Question>();
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            CreatedBy = "Anonymous"; // Default value
        }
    }
} 