namespace FoodOrderingSystem.ViewModels
{
    public class ShoppingCartItemViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }/*=> Quantity * UnitPrice;*/
    }
}
