using FoodOrderingSystem.Data;
using FoodOrderingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Dapper;
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    private readonly string _connectionString = "Server=PHAMVANPHUONG\\SQLEXPRESS06;Database=FoodWebApp;Integrated Security=True;Trust Server Certificate=True"; // Add your database connection string here.

    // GET: Admin/AddOrEdit
    public IActionResult AddOrEdit(int id = 0)
    {
        if (id == 0)
        {
            // Return a new product when no ID is provided (for creating a new product)
            return View(new Product());
        }
        else
        {
            // Retrieve the product from the database if an ID is provided (for editing an existing product)
            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                dbConnection.Open();
                var product = dbConnection.QueryFirstOrDefault<Product>("SELECT * FROM Products WHERE Id = @Id", new { Id = id });
                return View(product);
            }
        }
    }

    // POST: Admin/AddOrEdit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddOrEdit([Bind("Id", "Name", "Description", "Price", "ImageUrl")] Product product)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Random hóa giá trị Stock và QuantityInCart
                Random random = new Random();
                product.Stock = random.Next(50, 201);  // Stock từ 50 đến 200
                product.QuantityInCart = random.Next(0, product.Stock + 1);  // Số lượng trong giỏ hàng từ 0 đến Stock

                using (IDbConnection dbConnection = new SqlConnection(_connectionString))
                {
                    dbConnection.Open();

                    // Insertion or Update Query
                    string query = product.Id == 0
                        ? "INSERT INTO Products (Name, Description, Price, ImageUrl, Stock, QuantityInCart) VALUES (@Name, @Description, @Price, @ImageUrl, @Stock, @QuantityInCart)"
                        : "UPDATE Products SET Name = @Name, Description = @Description, Price = @Price, ImageUrl = @ImageUrl, Stock = @Stock, QuantityInCart = @QuantityInCart WHERE Id = @Id";

                    var result = await dbConnection.ExecuteAsync(query, product);

                    Console.WriteLine(result > 0 ? "Dữ liệu đã được thêm hoặc cập nhật thành công!" : "Có lỗi khi thêm hoặc cập nhật dữ liệu.");
                    return RedirectToAction("Dashboard");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi truy vấn CSDL: {ex.Message}");
                ModelState.AddModelError("", "Đã có lỗi xảy ra trong quá trình lưu dữ liệu.");
            }
        }
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        using (IDbConnection dbConnection = new SqlConnection(_connectionString))
        {
            dbConnection.Open();

            // Xóa tất cả các bản ghi trong bảng ShoppingCartItems liên kết với sản phẩm
            string deleteCartItemsQuery = "DELETE FROM ShoppingCartItems WHERE ProductId = @ProductId";
            dbConnection.Execute(deleteCartItemsQuery, new { ProductId = id });

            // Xóa sản phẩm
            string deleteProductQuery = "DELETE FROM Products WHERE Id = @Id";
            dbConnection.Execute(deleteProductQuery, new { Id = id });

            return RedirectToAction("Dashboard");
        }
    }

    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }
    // Hiển thị trang đăng nhập admin
    public IActionResult AdminLogin()
    {
        return View();
    }

    // Xử lý đăng nhập admin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string username, string password)
    {
        var admin = _context.Admins.FirstOrDefault(a => a.Username == username);
        if (admin != null)
        {
            // Nếu bạn đang sử dụng hash mật khẩu
            if (VerifyPassword(password, admin.Password))
            {
                HttpContext.Session.SetString("AdminUsername", username); // Lưu tên đăng nhập vào session
                return RedirectToAction("Dashboard");
            }

            // Hoặc nếu không dùng hash, bạn có thể so sánh mật khẩu trực tiếp
            // if (admin.Password == password) { ... }
        }

        TempData["ErrorMessage"] = "Invalid username or password.";
        return RedirectToAction("AdminLogin");
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        // Đây là một ví dụ về cách bạn có thể kiểm tra mật khẩu đã được hash
        // (Nếu bạn sử dụng một hàm hash như SHA256 hoặc bcrypt, bạn cần thay đổi phương thức này)
        return password == storedHash; // Chỉ so sánh trực tiếp nếu không dùng hash
    }


    // Dashboard cho admin sau khi đăng nhập
    public IActionResult Dashboard()
    {
        var products = _context.Products.ToList();
        return View(products);
    }
}
