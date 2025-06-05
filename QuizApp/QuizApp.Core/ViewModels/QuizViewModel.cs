using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuizApp.Core.ViewModels
{
    public class QuizViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        
        public bool IsActive { get; set; }
        
        [Required(ErrorMessage = "Time limit is required")]
        public string TimeLimit { get; set; } // Format: HH:mm:ss
        
        [Required(ErrorMessage = "At least one question is required")]
        public List<QuestionViewModel> Questions { get; set; }
    }

    public class QuestionViewModel
    {
        [Required(ErrorMessage = "Question text is required")]
        public string Text { get; set; }
        
        [Range(0, 1000, ErrorMessage = "Points must be between 0 and 1000")]
        public int Points { get; set; }
        
        public int TimeLimit { get; set; } = 20; // Default 20 seconds
        
        [Required(ErrorMessage = "At least one answer is required")]
        public List<AnswerViewModel> Answers { get; set; }
    }

    public class AnswerViewModel
    {
        [Required(ErrorMessage = "Answer text is required")]
        public string Text { get; set; }
        
        public bool IsCorrect { get; set; }
    }
} 