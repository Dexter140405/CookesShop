using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CookesShop.Web.Data;

namespace CookesShop.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Sales")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.Date;
        
        ViewBag.TotalOrders = await _context.Orders.CountAsync();
        ViewBag.PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
        ViewBag.TotalProducts = await _context.Products.CountAsync();
        ViewBag.TotalUsers = await _context.Users.CountAsync();

        ViewBag.RevenueToday = await _context.Orders
            .Where(o => o.Status == "Completed" && o.OrderDate >= today)
            .SumAsync(o => o.TotalAmount);

        // Simple recent orders
        var recentOrders = await _context.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .ToListAsync();

        return View(recentOrders);
    }
}
