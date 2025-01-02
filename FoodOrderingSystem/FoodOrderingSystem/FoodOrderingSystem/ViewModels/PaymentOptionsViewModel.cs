using System.Collections;
using System.Collections.Generic;
using FoodOrderingSystem.Models;
namespace FoodOrderingSystem.ViewModels
{
    public class PaymentOptionsViewModel
    {
        public decimal? TotalPrice { get; set; }
        public IEnumerable<PaymentOptions> PaymentOptions { get; set; }
    }
}
