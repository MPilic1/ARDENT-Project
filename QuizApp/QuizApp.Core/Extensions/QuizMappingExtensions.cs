using System;
using QuizApp.Core.Models;
using QuizApp.Core.ViewModels;
using System.Linq;

namespace QuizApp.Core.Extensions
{
    public static class QuizMappingExtensions
    {
        public static QuizViewModel ToViewModel(this Quiz quiz)
        {
            if (quiz == null) return null;

            return new QuizViewModel
            {
                Title = quiz.Title,
                Description = quiz.Description,
                IsActive = quiz.IsActive,
                TimeLimit = quiz.TimeLimit.ToString(@"hh\:mm\:ss"),
                Questions = quiz.Questions?.Select(q => new QuestionViewModel
                {
                    Text = q.Text,
                    Points = q.Points,
                    TimeLimit = q.TimeLimit,
                    Answers = q.Answers?.Select(a => new AnswerViewModel
                    {
                        Text = a.Text,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                }).ToList()
            };
        }

        public static Quiz ToModel(this QuizViewModel viewModel)
        {
            if (viewModel == null) return null;

            return new Quiz
            {
                Title = viewModel.Title,
                Description = viewModel.Description,
                IsActive = viewModel.IsActive,
                TimeLimit = TimeSpan.Parse(viewModel.TimeLimit),
                Questions = viewModel.Questions?.Select(q => new Question
                {
                    Text = q.Text,
                    Points = q.Points,
                    TimeLimit = q.TimeLimit,
                    Type = QuestionType.MultipleChoice,
                    Answers = q.Answers?.Select(a => new Answer
                    {
                        Text = a.Text,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                }).ToList()
            };
        }
    }
} 