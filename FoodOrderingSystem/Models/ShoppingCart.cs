namespace FoodOrderingSystem.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public List<ShoppingCartItem>? Items { get; set; } = new List<ShoppingCartItem>();
        
        
    }
}
