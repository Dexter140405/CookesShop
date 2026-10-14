using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CookesShop.Web.Data;
using CookesShop.Web.Models;

namespace CookesShop.Web.Controllers;

public class ProductController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Product?category=slug&search=keyword
    public async Task<IActionResult> Index(string? category, string? search, int page = 1)
    {
        int pageSize = 12;
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        // Lọc theo danh mục
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category != null && p.Category.Slug == category);
            ViewBag.CurrentCategory = category;
        }

        // Tìm kiếm
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));
            ViewBag.CurrentSearch = search;
        }

        // Lấy danh sách danh mục cho sidebar filter
        ViewBag.Categories = await _context.Categories.ToListAsync();

        int totalItems = await query.CountAsync();
        var products = await query
            .OrderByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return View(products);
    }

    // GET: /Product/Details/{slug}
    [Route("Product/Details/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        if (string.IsNullOrEmpty(slug)) return NotFound();

        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(p => p.Slug == slug);

        if (product == null) return NotFound();

        // Lấy sản phẩm liên quan
        var relatedProducts = await _context.Products
            .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
            .Take(4)
            .ToListAsync();
            
        ViewBag.RelatedProducts = relatedProducts;

        return View(product);
    }
}
