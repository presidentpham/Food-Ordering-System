using FoodOrderingSystem.Models;

namespace FoodOrderingSystem.Services
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentOptions>> GetPaymentOptionsAsync();
        Task<decimal?> GetTotalPriceAsync(int cartId);
    }
}
