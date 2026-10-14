using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CookesShop.Web.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return Redirect("/Identity/Account/Login");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return Redirect("/Identity/Account/Register");
    }

    [Authorize]
    [HttpGet]
    public IActionResult Profile()
    {
        return Redirect("/Identity/Account/Manage");
    }
}
