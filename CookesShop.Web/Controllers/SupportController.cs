using Microsoft.AspNetCore.Mvc;

namespace CookesShop.Web.Controllers;

public class SupportController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
