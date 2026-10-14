using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CookesShop.Web.Data;
using CookesShop.Web.Models;

namespace CookesShop.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Sales")]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.DeliveryStaff)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.DeliveryStaff)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        // Get delivery staff list for assignment
        var deliveryStaffs = await _userManager.GetUsersInRoleAsync("Delivery");
        ViewBag.DeliveryStaffId = new SelectList(deliveryStaffs, "Id", "FullName", order.DeliveryStaffId);

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status, string? deliveryStaffId)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return NotFound();

        order.Status = status;
        
        if (!string.IsNullOrEmpty(deliveryStaffId))
        {
            order.DeliveryStaffId = deliveryStaffId;
        }

        // If completed, update payment status
        if (status == "Completed" && order.PaymentStatus == "Unpaid")
        {
            order.PaymentStatus = "Paid";
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công.";
        
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }
}
