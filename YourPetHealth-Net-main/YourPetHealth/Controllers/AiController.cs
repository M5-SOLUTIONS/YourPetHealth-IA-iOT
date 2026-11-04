using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace YourPetHealth.Controllers
{
    [Authorize]
    public class AiController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}