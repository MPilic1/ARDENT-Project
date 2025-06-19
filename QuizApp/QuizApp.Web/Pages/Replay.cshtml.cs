using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QuizApp.Web.Pages
{
    public class ReplayModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int QuizId { get; set; }

        public void OnGet()
        {
            // Placeholder: In the future, fetch quiz data and start a new game session
        }
    }
} 