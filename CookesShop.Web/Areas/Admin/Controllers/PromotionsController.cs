using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CookesShop.Web.Data;
using CookesShop.Web.Models;

namespace CookesShop.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Sales")]
public class PromotionsController : Controller
{
    private readonly ApplicationDbContext _context;

    public PromotionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Promotions.ToListAsync());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Promotion promotion)
    {
        if (ModelState.IsValid)
        {
            _context.Add(promotion);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(promotion);
    }
}
