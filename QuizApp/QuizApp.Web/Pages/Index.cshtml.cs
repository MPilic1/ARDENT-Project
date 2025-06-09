using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuizApp.Core.Models;
using QuizApp.Web.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizApp.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly QuizApiService _quizApiService;

        public IndexModel(QuizApiService quizApiService)
        {
            _quizApiService = quizApiService;
        }

        public IEnumerable<Quiz> Quizzes { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Quizzes = await _quizApiService.GetQuizzesAsync();
                return Page();
            }
            catch
            {
                Quizzes = new List<Quiz>();
                return Page();
            }
        }
    }
}