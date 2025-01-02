namespace FoodOrderingSystem.Models
{
    public class SyncIds
    {
        public int Id { get; set; }
        public int CurrentSyncId { get; set; }

        public string? UserId { get; set; } // Link to IdentityUser
        public int? CartId { get; set; }
    }
}
