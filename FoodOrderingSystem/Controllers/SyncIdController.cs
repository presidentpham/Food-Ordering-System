using FoodOrderingSystem.Data;
using FoodOrderingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingSystem.Controllers
{
    public class SyncIdController:Controller
    {
        private readonly ApplicationDbContext _context;

        public SyncIdController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetNextSyncIdAsync(string userId, int cartId)
        {
            var syncIdRecord = await _context.SyncIds
                .Where(s => s.UserId == userId && s.CartId == cartId)
                .FirstOrDefaultAsync();

            if (syncIdRecord == null)
            {
                syncIdRecord = new SyncIds
                {
                    CurrentSyncId = 1,
                    UserId = userId,
                    CartId = cartId
                };
                _context.SyncIds.Add(syncIdRecord);
                await _context.SaveChangesAsync();
            }
            else
            {
                syncIdRecord.CurrentSyncId++;
                _context.SyncIds.Update(syncIdRecord);
                await _context.SaveChangesAsync();
            }

            return syncIdRecord.CurrentSyncId;
        }

    }
}
