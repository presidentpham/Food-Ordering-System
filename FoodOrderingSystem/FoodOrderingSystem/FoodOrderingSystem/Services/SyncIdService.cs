using FoodOrderingSystem.Data;
using FoodOrderingSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingSystem.Services
{

    public interface ISyncIdService
    {
        Task<int> GetNextSyncIdAsync(string userId, int cartId);
    }
    public class SyncIdService:ISyncIdService
    {
        private readonly ApplicationDbContext _context;

        public SyncIdService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetNextSyncIdAsync(string userId, int cartId)
        {
            // Get the maximum SyncId from the database
            var maxSyncId = await _context.SyncIds.MaxAsync(s => (int?)s.CurrentSyncId) ?? 0;

            // Create a new SyncId record with the incremented value
            var newSyncIdRecord = new SyncIds
            {
                UserId = userId,
                CartId = cartId,
                CurrentSyncId = maxSyncId + 1
            };

            // Add the new SyncId record to the database
            _context.SyncIds.Add(newSyncIdRecord);
            await _context.SaveChangesAsync();

            // Return the new SyncId value
            return newSyncIdRecord.CurrentSyncId;
        }

    }
}
