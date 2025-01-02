
using FoodOrderingSystem.Data;
using FoodOrderingSystem.Models;
using FoodOrderingSystem.Services;
using FoodOrderingSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FoodOrderingSystem.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly ShoppingCartService _shoppingCartService;
        private readonly ApplicationDbContext _context;

        public ShoppingCartController(ShoppingCartService shoppingCartService, ApplicationDbContext context)
        {
            _shoppingCartService = shoppingCartService;
            _context = context;
        }
        public IActionResult ProceedToCheckout()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Redirect to the checkout page if the user is authenticated
                return RedirectToAction("Index", "Checkout");
            }
            else
            {
                // Redirect to the login page if the user is not authenticated
                // Include a returnUrl so the user can be redirected back to checkout after logging in
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Checkout") });
            }
        }
        public ShoppingCart GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString("ShoppingCart");
            if (string.IsNullOrEmpty(cartJson))
            {
                return new ShoppingCart { Items = new List<ShoppingCartItem>() };
            }
            try
            {
                return JsonSerializer.Deserialize<ShoppingCart>(cartJson);
            }
            catch (JsonException)
            {
                // Handle JSON deserialization errors if needed
                return new ShoppingCart { Items = new List<ShoppingCartItem>() };
            }
        }

        public void SaveCartToSession(ShoppingCart cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("ShoppingCart", cartJson);
        }

        public async Task<IActionResult> Index()
        {
            var cartIdCookie = Request.Cookies["CartId"];
            if (string.IsNullOrEmpty(cartIdCookie) || !int.TryParse(cartIdCookie, out int cartId))
            {
                // Handle case where cartId is missing or invalid
                return RedirectToAction("Error", "Home"); // Or any error handling
            }

            var cart = await _context.ShoppingCarts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .SingleOrDefaultAsync(c => c.Id == cartId);

            if (cart == null)
            {
                // Handle case where cart is not found
                return RedirectToAction("EmptyCart", "ShoppingCart"); // Or any error handling
            }

            var viewModel = new ShoppingCartViewModel
            {
                CartId = cart.Id,
                Items = cart.Items.Select(item => new ShoppingCartItemViewModel
                {
                    Id = item.Id,
                    ProductName = item.Product?.Name ?? "Unknown", // Handle null Product
                    Quantity = item.Quantity ?? 0,
                    UnitPrice = item.UnitPrice ?? 0,
                    TotalPrice = (item.Quantity ?? 0) * (item.UnitPrice ?? 0) // Calculate total price
                }).ToList()
            };

            var totalPrice = viewModel.Items.Sum(item => item.TotalPrice);
            ViewBag.TotalPrice = totalPrice;

            // Save the total price in TempData
            TempData["TotalPrice"] = totalPrice.ToString(CultureInfo.InvariantCulture);

            //return View(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var cartIdCookie = Request.Cookies["CartId"];
            if (string.IsNullOrEmpty(cartIdCookie) || !int.TryParse(cartIdCookie, out int cartId))
            {
                // Handle case where cartId is missing or invalid
                TempData["ErrorMessage"] = "Invalid cart.";
                return RedirectToAction("Index");
            }

            var cart = await _context.ShoppingCarts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .SingleOrDefaultAsync(c => c.Id == cartId);

            if (cart == null)
            {
                // Handle case where cart is not found
                TempData["ErrorMessage"] = "Cart not found.";
                return RedirectToAction("Index");
            }

            var product = await _context.Products.FindAsync(productId);

            if (product == null || quantity <= 0)
            {
                // Handle error (e.g., product not found or invalid quantity)
                TempData["ErrorMessage"] = "Invalid product or quantity.";
                return RedirectToAction("Index");
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var newItem = new ShoppingCartItem
                {
                    ProductId = productId,
                    Product = product,
                    Quantity = quantity,
                    UnitPrice = product.Price
                };
                cart.Items.Add(newItem);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var cartIdCookie = Request.Cookies["CartId"];
            if (string.IsNullOrEmpty(cartIdCookie) || !int.TryParse(cartIdCookie, out int cartId))
            {
                // Handle case where cartId is missing or invalid
                TempData["ErrorMessage"] = "Invalid cart.";
                return RedirectToAction("Index");
            }

            var cart = await _context.ShoppingCarts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .SingleOrDefaultAsync(c => c.Id == cartId);

            if (cart == null)
            {
                // Handle case where cart is not found
                TempData["ErrorMessage"] = "Cart not found.";
                return RedirectToAction("Index");
            }

            var itemToRemove = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
            if (itemToRemove != null)
            {
                cart.Items.Remove(itemToRemove);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateItemQuantity(int cartItemId, int quantity)
        {
            var cartIdCookie = Request.Cookies["CartId"];
            if (string.IsNullOrEmpty(cartIdCookie) || !int.TryParse(cartIdCookie, out int cartId))
            {
                // Handle case where cartId is missing or invalid
                TempData["ErrorMessage"] = "Invalid cart.";
                return RedirectToAction("Index");
            }

            var cart = await _context.ShoppingCarts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .SingleOrDefaultAsync(c => c.Id == cartId);

            if (cart == null)
            {
                // Handle case where cart is not found
                TempData["ErrorMessage"] = "Cart not found.";
                return RedirectToAction("Index");
            }

            var itemToUpdate = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

            if (itemToUpdate != null)
            {
                if (quantity <= 0)
                {
                    cart.Items.Remove(itemToUpdate);
                }
                else
                {
                    itemToUpdate.Quantity = quantity;
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }

    
}

