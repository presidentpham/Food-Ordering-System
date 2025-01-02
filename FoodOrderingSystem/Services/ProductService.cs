using FoodOrderingSystem.Data;
using FoodOrderingSystem.Models;
using Microsoft.Data.SqlClient;

namespace FoodOrderingSystem.Services
{
    public class ProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Product> GetAllProducts()
        {
            try
            {
                return _context.Products.ToList();
            }
            catch (SqlException ex)
            {
                // Log the exception details
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public Product GetProductById(int productId)
        {
            return _context.Products.Find(productId);
        }
    }
}
