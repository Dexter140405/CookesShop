using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CookesShop.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Sales")]
public class CustomerCareController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
