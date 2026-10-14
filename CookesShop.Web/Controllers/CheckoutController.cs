using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CookesShop.Web.Data;
using CookesShop.Web.Models;
using System.Security.Claims;

namespace CookesShop.Web.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly ApplicationDbContext _context;

    public CheckoutController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var cartItems = await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        var user = await _context.Users.FindAsync(userId);
        ViewBag.CurrentUser = user;

        if (!cartItems.Any())
        {
            return RedirectToAction("Index", "Cart");
        }

        return View(cartItems);
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(string paymentMethod, string? promotionCode, string fullName, string phoneNumber, string address)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _context.Users.FindAsync(userId);

        if (user != null)
        {
            user.FullName = fullName;
            user.PhoneNumber = phoneNumber;
            user.Address = address;
        }
        
        var cartItems = await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

        // Validate stock
        foreach (var item in cartItems)
        {
            if (item.Product!.StockQuantity < item.Quantity)
            {
                TempData["Error"] = $"Sản phẩm {item.Product.Name} không đủ số lượng.";
                return RedirectToAction("Index");
            }
        }

        decimal totalAmount = cartItems.Sum(c => c.Quantity * c.Product!.Price);

        // Validate promotion
        if (!string.IsNullOrEmpty(promotionCode))
        {
            var promo = await _context.Promotions.FirstOrDefaultAsync(p => p.Code == promotionCode && p.IsActive && p.ExpiryDate > DateTime.UtcNow);
            if (promo != null)
            {
                decimal discount = totalAmount * (promo.DiscountPercentage / 100);
                if (promo.MaxDiscountAmount.HasValue && discount > promo.MaxDiscountAmount.Value)
                {
                    discount = promo.MaxDiscountAmount.Value;
                }
                totalAmount -= discount;
            }
        }

        // Create Order
        var order = new Order
        {
            UserId = userId!,
            PaymentMethod = paymentMethod,
            PaymentStatus = paymentMethod == "Online" ? "Paid" : "Unpaid", // Simplification
            TotalAmount = totalAmount,
            PromotionCode = promotionCode,
            Status = "Pending"
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Create Order Items and Update Stock
        foreach (var item in cartItems)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Product!.Price
            };
            _context.OrderItems.Add(orderItem);

            item.Product.StockQuantity -= item.Quantity; // Deduct stock
        }

        // Clear cart
        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        return RedirectToAction("Success", new { orderId = order.Id });
    }

    public IActionResult Success(int orderId)
    {
        ViewBag.OrderId = orderId;
        return View();
    }
}
