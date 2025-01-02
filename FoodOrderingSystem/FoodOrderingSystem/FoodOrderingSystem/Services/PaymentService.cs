using FoodOrderingSystem.Data;
using FoodOrderingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingSystem.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;

        public PaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PaymentOptions>> GetPaymentOptionsAsync()
        {
            return await _context.PaymentOptions.ToListAsync();
        }

        public async Task<decimal?> GetTotalPriceAsync(int cartId)
        {
            var cartItems = await _context.ShoppingCartItems
                                  .Where(item => item.Id == cartId)
                                  .ToListAsync();

            decimal? totalPrice = cartItems.Sum(item => item.UnitPrice * item.Quantity);
            return totalPrice;
        }

        
    }
}