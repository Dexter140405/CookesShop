using CookesShop.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CookesShop.Web.Data;

public static class DbSeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roleNames = { "Admin", "Sales", "Delivery", "Customer" };
        IdentityResult roleResult;

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var adminUser = new ApplicationUser
        {
            UserName = "admin@cookesshop.com",
            Email = "admin@cookesshop.com",
            FullName = "Administrator",
            EmailConfirmed = true
        };

        var user = await userManager.FindByEmailAsync(adminUser.Email);
        if (user == null)
        {
            var createPowerUser = await userManager.CreateAsync(adminUser, "Admin@123");
            if (createPowerUser.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        var testCustomer = new ApplicationUser
        {
            UserName = "customer@cookesshop.com",
            Email = "customer@cookesshop.com",
            FullName = "Khách Hàng Test",
            EmailConfirmed = true
        };

        var customer = await userManager.FindByEmailAsync(testCustomer.Email);
        if (customer == null)
        {
            var createCustomer = await userManager.CreateAsync(testCustomer, "Customer@123");
            if (createCustomer.Succeeded)
            {
                await userManager.AddToRoleAsync(testCustomer, "Customer");
            }
        }
    }

    public static async Task SeedProductsAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category { Name = "Cookies", Slug = "cookies", Description = "Các loại bánh quy ngon tuyệt" },
                new Category { Name = "Bánh dứa", Slug = "banh-dua", Description = "Bánh dứa Đài Loan" },
                new Category { Name = "Bánh ngọt", Slug = "banh-ngot", Description = "Gato flan, Tiramisu..." }
            };
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        if (!context.Products.Any(p => p.Name.Contains("Oreo")))
        {
            var cookieCat = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "cookies") ?? context.Categories.First();
            var duaCat = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "banh-dua") ?? context.Categories.First();
            var ngotCat = await context.Categories.FirstOrDefaultAsync(c => c.Slug == "banh-ngot") ?? context.Categories.First();

            var products = new List<Product>
            {
                new Product { CategoryId = cookieCat.Id, Name = "Oreo Cookies (150gr)", Slug = "oreo-cookies-150gr", Price = 35000, StockQuantity = 100, ImageUrl = "/images/Oreo Cookies.jpg" },
                new Product { CategoryId = cookieCat.Id, Name = "Cacao Cookies (150gr)", Slug = "cacao-cookies-150gr", Price = 35000, StockQuantity = 100, ImageUrl = "/images/Caocao Cookies.jpg" },
                new Product { CategoryId = cookieCat.Id, Name = "Matcha Cookies (150gr)", Slug = "matcha-cookies-150gr", Price = 40000, StockQuantity = 100, ImageUrl = "/images/Matcha Cookies.jpg" },
                new Product { CategoryId = cookieCat.Id, Name = "Phô mai Cookies (150gr)", Slug = "pho-mai-cookies-150gr", Price = 40000, StockQuantity = 100, ImageUrl = "/images/Cheese Cookies.jpg" },
                new Product { CategoryId = cookieCat.Id, Name = "Mix vị Cookies (150gr)", Slug = "mix-vi-cookies-150gr", Price = 40000, StockQuantity = 100, ImageUrl = "" },

                new Product { CategoryId = duaCat.Id, Name = "Bánh dứa (10 cái)", Slug = "banh-dua-10-cai", Price = 35000, StockQuantity = 100, ImageUrl = "/images/Pineapple Cake.jpg" },
                new Product { CategoryId = duaCat.Id, Name = "Bánh dứa (15 cái)", Slug = "banh-dua-15-cai", Price = 50000, StockQuantity = 100, ImageUrl = "/images/Pineapple Cake.jpg" },
                new Product { CategoryId = duaCat.Id, Name = "Bánh dứa (20 cái)", Slug = "banh-dua-20-cai", Price = 65000, StockQuantity = 100, ImageUrl = "/images/Pineapple Cake.jpg" },
                new Product { CategoryId = duaCat.Id, Name = "Bánh dứa (30 cái)", Slug = "banh-dua-30-cai", Price = 95000, StockQuantity = 100, ImageUrl = "/images/Pineapple Cake.jpg" },

                new Product { CategoryId = ngotCat.Id, Name = "Gato flan", Slug = "gato-flan", Description = "Lưu ý đặt trước 24h", Price = 60000, StockQuantity = 50, ImageUrl = "/images/Gato Plan.jpg" },
                new Product { CategoryId = ngotCat.Id, Name = "Tiramisu (1 hộp)", Slug = "tiramisu-1-hop", Price = 65000, StockQuantity = 50, ImageUrl = "/images/Tiramisu.jpg" }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}
