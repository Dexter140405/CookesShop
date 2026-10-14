using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CookesShop.Web.Data;
using CookesShop.Web.Models;
using System.Security.Claims;

namespace CookesShop.Web.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string GetSessionId()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("CartSessionId")))
        {
            HttpContext.Session.SetString("CartSessionId", Guid.NewGuid().ToString());
        }
        return HttpContext.Session.GetString("CartSessionId")!;
    }

    public async Task<IActionResult> Index()
    {
        var sessionId = GetSessionId();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Fetch cart items by User ID if logged in, otherwise by Session ID
        var query = _context.CartItems.Include(c => c.Product).AsQueryable();
        if (userId != null)
        {
            query = query.Where(c => c.UserId == userId || c.SessionId == sessionId);
        }
        else
        {
            query = query.Where(c => c.SessionId == sessionId);
        }

        var cartItems = await query.ToListAsync();

        // Migrate session items to user items if logged in
        if (userId != null && cartItems.Any(c => c.UserId == null))
        {
            foreach (var item in cartItems.Where(c => c.UserId == null))
            {
                item.UserId = userId;
            }
            await _context.SaveChangesAsync();
        }

        return View(cartItems);
    }

    [HttpGet]
    public async Task<IActionResult> GetCartCount()
    {
        var sessionId = GetSessionId();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int totalItems = await GetCartCountAsync(sessionId, userId);
        return Json(new { count = totalItems });
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null || product.StockQuantity < quantity)
        {
            return Json(new { success = false, message = "Sản phẩm không đủ số lượng." });
        }

        var sessionId = GetSessionId();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        CartItem? cartItem = null;
        if (userId != null)
        {
            cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == userId);
        }
        else
        {
            cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.ProductId == productId && c.SessionId == sessionId);
        }

        if (cartItem == null)
        {
            cartItem = new CartItem
            {
                ProductId = productId,
                SessionId = sessionId,
                UserId = userId,
                Quantity = quantity
            };
            _context.CartItems.Add(cartItem);
        }
        else
        {
            cartItem.Quantity += quantity;
            if (cartItem.Quantity > product.StockQuantity)
                cartItem.Quantity = product.StockQuantity;
        }

        await _context.SaveChangesAsync();

        // Get total count
        int totalItems = await GetCartCountAsync(sessionId, userId);

        return Json(new { success = true, totalItems = totalItems });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
    {
        var cartItem = await _context.CartItems.Include(c => c.Product).FirstOrDefaultAsync(c => c.Id == cartItemId);
        if (cartItem != null)
        {
            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                if (quantity > cartItem.Product!.StockQuantity) quantity = cartItem.Product.StockQuantity;
                cartItem.Quantity = quantity;
            }
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int cartItemId)
    {
        var cartItem = await _context.CartItems.FindAsync(cartItemId);
        if (cartItem != null)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<int> GetCartCountAsync(string sessionId, string? userId)
    {
        if (userId != null)
        {
            return await _context.CartItems.Where(c => c.UserId == userId).SumAsync(c => c.Quantity);
        }
        return await _context.CartItems.Where(c => c.SessionId == sessionId).SumAsync(c => c.Quantity);
    }
}
