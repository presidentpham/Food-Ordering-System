using FoodOrderingSystem.Data;
using FoodOrderingSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrderingSystem.Services
{
    public class ShoppingCartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string ShoppingCartSessionKey = "ShoppingCart";

        public ShoppingCartService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpContext HttpContext => _httpContextAccessor.HttpContext;

        private string GetUserId()
        {
            return HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<ShoppingCart> GetCartByUserIdAsync()
        {
            var userId = GetUserId(); // Kullanıcı ID'sini doğru bir şekilde alın
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            var sessionCart = GetCartFromSession();
            if (sessionCart != null && sessionCart.UserId == userId)
            {
                return sessionCart;
            }

            var cart = await _context.ShoppingCarts
                                     .Include(sc => sc.Items)
                                     .ThenInclude(si => si.Product)
                                     .FirstOrDefaultAsync(sc => sc.UserId == userId);

            if (cart != null)
            {
                SaveCartToSession(cart); // Cache in session
            }

            return cart ?? new ShoppingCart { UserId = userId };
        }

        public async Task AddItemToCartAsync(int productId, int quantity)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User not logged in.");
            }

            var shoppingCart = await GetCartByUserIdAsync();

            if (shoppingCart == null)
            {
                shoppingCart = new ShoppingCart
                {
                    UserId = userId,
                    Items = new List<ShoppingCartItem>()
                };
                _context.ShoppingCarts.Add(shoppingCart);
            }

            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
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
                    ProductId = productId,
                    Product = product,
                    Quantity = quantity,
                    UnitPrice = product.Price
                };
                shoppingCart.Items.Add(newItem);
            }

            await _context.SaveChangesAsync();
            SaveCartToSession(shoppingCart); // Update session
        }

        public async Task RemoveItemFromCartAsync(int cartItemId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User not logged in.");
            }

            var cart = await GetCartByUserIdAsync();

            if (cart == null)
            {
                throw new KeyNotFoundException("Shopping cart not found");
            }

            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

            if (item != null)
            {
                _context.ShoppingCartItems.Remove(item); // Remove from DbContext
                cart.Items.Remove(item); // Remove from list
                await _context.SaveChangesAsync();
                SaveCartToSession(cart); // Update session
            }
            else
            {
                throw new KeyNotFoundException("Cart item not found");
            }
        }

        public async Task UpdateItemQuantityAsync(int cartItemId, int quantity)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User not logged in.");
            }

            var cart = await GetCartByUserIdAsync();

            if (cart == null)
            {
                throw new KeyNotFoundException("Shopping cart not found");
            }

            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Items.Remove(item); // Remove item if quantity is zero or less
                }
                else
                {
                    item.Quantity = quantity;
                }
                await _context.SaveChangesAsync();
                SaveCartToSession(cart); // Update session
            }
            else
            {
                throw new KeyNotFoundException("Cart item not found");
            }
        }

        private ShoppingCart GetCartFromSession()
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
                return new ShoppingCart { Items = new List<ShoppingCartItem>() };
            }
        }

        private void SaveCartToSession(ShoppingCart cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("ShoppingCart", cartJson);
        }
    }
}
