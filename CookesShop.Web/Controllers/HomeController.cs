using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CookesShop.Web.Models;
using CookesShop.Web.Data;

namespace CookesShop.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Get Hot Products (even if out of stock, they should show 'Cháy hàng')
        var hotProducts = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsHot)
            .Take(8)
            .ToListAsync();

        // Get Best Sellers (simulate by taking random/recent, but excluding out of stock)
        // In a real app, this would be based on OrderItems counts
        var bestSellers = await _context.Products
            .Include(p => p.Category)
            .Where(p => !p.IsHot && p.StockQuantity > 0)
            .OrderByDescending(p => p.Id) // Fake best sellers logic for now
            .Take(8)
            .ToListAsync();

        ViewBag.HotProducts = hotProducts;
        ViewBag.BestSellers = bestSellers;

        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
