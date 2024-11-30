using Microsoft.AspNetCore.Mvc;

namespace EQToolApis.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
