using Microsoft.AspNetCore.Mvc;

namespace YenMay_web.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
