using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoPost.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            if (!User.IsInRole("Admin"))
            {
                return Content("You are not in the Admin role.");
            }

            return View();
        }

    }
}
