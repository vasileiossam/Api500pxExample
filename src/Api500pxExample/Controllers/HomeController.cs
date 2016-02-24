using System.Threading.Tasks;
using Api500pxExample.Api;
using Microsoft.AspNet.Mvc;

namespace Api500pxExample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<ViewResult> Popular()
        {
            var service = new Api500px();

            ViewData["Message"] = "Popular Photos";
            ViewData.Model = await service.Popular();

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
