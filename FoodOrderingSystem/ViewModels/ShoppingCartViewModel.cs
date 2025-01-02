namespace FoodOrderingSystem.ViewModels
{
    public class ShoppingCartViewModel
    {
        public int CartId { get; set; }
        public List<ShoppingCartItemViewModel> Items { get; set; } = new List<ShoppingCartItemViewModel>();

        public decimal TotalPrice { get; set; }
    }
}
