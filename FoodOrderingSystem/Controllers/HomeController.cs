using FoodOrderingSystem.Data;
using FoodOrderingSystem.Models;
using FoodOrderingSystem.Services;
using FoodOrderingSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FoodOrderingSystem.Controllers
{
    public class HomeController : Controller
    {

        private readonly ShoppingCartService _shoppingCartService;
        private readonly ApplicationDbContext _context;


        private readonly ILogger<HomeController> _logger;
        private readonly ProductService _productService;
        private string userId;

        public HomeController(ILogger<HomeController> logger, ProductService productService, ShoppingCartService shoppingCartService, ApplicationDbContext context)
        {
            _logger = logger;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _context = context;

        }

        public async Task<IActionResult> Index()
        {
            List<Product> products = (List<Product>)_productService.GetAllProducts() ?? new List<Product>();

            var cartIdCookie = Request.Cookies["CartId"];
            int cartId;
            if (cartIdCookie != null && int.TryParse(cartIdCookie, out cartId))
            {
                var shoppingCart = await _context.ShoppingCarts
                    .Include(sc => sc.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(sc => sc.Id == cartId);

                if (shoppingCart != null)
                {
                    // Create a dictionary for easy lookup
                    var cartItems = shoppingCart.Items.ToDictionary(i => i.ProductId, i => i.Quantity ?? 0);

                    // Populate QuantityInCart for each product
                    foreach (var product in products)
                    {
                        if (cartItems.TryGetValue(product.Id, out int quantity))
                        {
                            product.QuantityInCart = quantity;
                        }
                        else
                        {
                            product.QuantityInCart = 0;
                        }
                    }
                }
            }
            else
            {
                // No cart found; all quantities are 0
                foreach (var product in products)
                {
                    product.QuantityInCart = 0;
                }
            }

            return View(products);
        }

        public async Task<IActionResult> CheckCartAndRedirect()//string userId)
        {
            var userId = User.Identity.Name;

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userCart = await _shoppingCartService.GetCartByUserIdAsync();
            if (userCart != null && userCart.Items.Any())
            {
                return RedirectToAction("CheckOut", "Checkout");
            }
            else
            {
                // Handle the case where the cart is null (e.g., return an error or redirect to a different page)
                return RedirectToAction("CartEmpty", "ShoppingCart");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }




    }
} 




