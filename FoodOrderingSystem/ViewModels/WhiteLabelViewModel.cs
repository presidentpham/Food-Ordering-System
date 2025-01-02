using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSystem.ViewModels
{
    public class WhiteLabelViewModel
    {
        [Required]
        [Display(Name = "Cardholder Name")]
        public string CardholderName { get; set; }

        [Required]
        [CreditCard]
        [Display(Name = "Credit Card Number")]
        public string CreditCardNumber { get; set; }

        [Required]
        [Display(Name = "Expiration Date (MM/YY)")]
        public string ExpirationDate { get; set; }

        [Required]
        [Display(Name = "Security Code")]
        public string SecurityCode { get; set; }
    }
}
