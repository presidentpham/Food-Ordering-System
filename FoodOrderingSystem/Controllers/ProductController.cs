using FoodOrderingSystem.Data;
using FoodOrderingSystem.Models;
using FoodOrderingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodOrderingSystem.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly ApplicationDbContext _context;
        private readonly ShoppingCartService _shoppingCartService;
        

        public ProductController(ProductService productService, ApplicationDbContext context, ShoppingCartService shoppingCartService)
        {
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _context = context;
            
        }

        public IActionResult Index()
        {
            List<Product> products = (List<Product>)_productService.GetAllProducts();

            var cartIdCookie = Request.Cookies["CartId"];
            int cartId;
            if (cartIdCookie != null && int.TryParse(cartIdCookie, out cartId))
            {
                var shoppingCart = _context.ShoppingCarts
                    .Include(sc => sc.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefault(sc => sc.Id == cartId);

                if (shoppingCart != null)
                {
                    // Create a dictionary for easy lookup
                    var cartItems = shoppingCart.Items.ToDictionary(i => i.ProductId, i =>i.Quantity);

                    // Populate QuantityInCart for each product
                    foreach (var product in products)
                    {
                        if (cartItems.TryGetValue(product.Id, out int? quantity))
                        {
                            product.QuantityInCart = quantity ?? 0;
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

       
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity)
        {
            var cartIdCookie = Request.Cookies["CartId"];
            int cartId;
            var product = _context.Products.Find(productId);
            
            if (quantity <= 0)
            {
                TempData["ErrorMessage"] = "Invalid quantity.";
                return RedirectToAction("Index", "Home");
            }

            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction("Index", "Home");
            }

            if (cartIdCookie != null && int.TryParse(cartIdCookie, out cartId))
            {
                cartId = int.Parse(cartIdCookie);
            }
            else
            {
                var newCart = new ShoppingCart
                {
                    UserId = User.Identity.IsAuthenticated ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null
                };
                _context.ShoppingCarts.Add(newCart);
                _context.SaveChanges();
                cartId = newCart.Id;
                Response.Cookies.Append("CartId", cartId.ToString(), new CookieOptions { HttpOnly = true, Expires = DateTimeOffset.Now.AddDays(30) });
            }

            var shoppingCart = _context.ShoppingCarts
                .Include(sc => sc.Items) // Ensure Items are included in the query
                .FirstOrDefault(sc => sc.Id == cartId);

            if (shoppingCart == null)
            {
                shoppingCart = new ShoppingCart
                {
                    //
                    Id = cartId,
                    UserId = User.Identity.IsAuthenticated ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null
                };
                _context.ShoppingCarts.Add(shoppingCart);
                _context.SaveChanges();
            }

            var existingItem = shoppingCart.Items.FirstOrDefault(i => i.ProductId == productId);


            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var newItem = new ShoppingCartItem
                {
                    ProductId = product.Id,
                    Product = product,
                    ShoppingCartId = shoppingCart.Id,
                    Quantity = quantity,
                    UnitPrice = product.Price
                };
                shoppingCart.Items.Add(newItem);
                
            }

            // Save changes to the database
            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cartId = Request.Cookies["CartId"] ?? Guid.NewGuid().ToString();
            var shoppingCart = _context.ShoppingCarts
                .Include(sc => sc.Items)
                .FirstOrDefault(sc => sc.Id.ToString() == cartId);


            if (shoppingCart == null)
            {
                // Handle the case where the shopping cart does not exist
                TempData["ErrorMessage"] = "Shopping cart not found.";
                return RedirectToAction("Index", "Home");
            }

            // Find the item to remove
            var itemToRemove = shoppingCart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (itemToRemove != null)
            {
                // Remove the item from the shopping cart's items list
                shoppingCart.Items.Remove(itemToRemove);

                // Remove the item from the ShoppingCartItems table
                _context.ShoppingCartItems.Remove(itemToRemove);

                // Save changes to the database
                _context.SaveChanges();
            }
            else
            {
                // Handle the case where the item is not found in the cart
                TempData["ErrorMessage"] = "Item not found in cart.";
            }

            return RedirectToAction("Index", "Home"); // Redirect to shopping cart page or any other relevant page
        }

    }
}
