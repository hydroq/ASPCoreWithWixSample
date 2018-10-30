using Microsoft.AspNetCore.Mvc;

namespace REFLEKT.ONEAuthor.WebAPI.Controllers
{
    public class ViewerController : Controller
    {
        [HttpGet]
        [Route("")]
        public IActionResult Default()
        {
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("/viewer")]
        public IActionResult Index()
        {
            return View();
        }
    }
}