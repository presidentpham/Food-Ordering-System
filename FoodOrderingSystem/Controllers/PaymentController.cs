using Microsoft.AspNetCore.Mvc;
using FoodOrderingSystem.Data;
using FoodOrderingSystem.Models;
using System.Threading.Tasks;
using FoodOrderingSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using FoodOrderingSystem.Services;
using FoodOrderingSystem.ViewModels;
using System.Web;
using Microsoft.Extensions.Options;

namespace FoodOrderingSystem.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentService _paymentService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly PaymentSettings _paymentSettings;
        private readonly ISyncIdService _syncIdService;

        private const string Username = "A49A8F35C812BD1CDEEB";
        private const string Token = "A49A8F35C812BD1CDEEBAF96992F145A";
        private const int KeywordId = 2;
        private const string RequestUrl = "https://testpos.payby.me/webpayment/Request.aspx";
        private const string PaymentPageUrl = "https://testpos.payby.me/webpayment/Pay.aspx";

        public PaymentController(ApplicationDbContext context, IPaymentService paymentService, IHttpClientFactory httpClientFactory, IOptions<PaymentSettings> paymentSettings, ISyncIdService syncIdService)
        {
            _context = context;
            _paymentService = paymentService;
            _httpClientFactory = httpClientFactory;
            _paymentSettings = paymentSettings.Value;
            _syncIdService = syncIdService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> PaymentOptions()
        {
            var paymentOptions = await _paymentService.GetPaymentOptionsAsync();

            var cartIdCookie = Request.Cookies["CartId"];
            if (string.IsNullOrEmpty(cartIdCookie) || !int.TryParse(cartIdCookie, out int cartId))
            {
                return RedirectToAction("Error", "Home");
            }

            var cart = await _context.ShoppingCarts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                .SingleOrDefaultAsync(c => c.Id == cartId);

            var totalPrice = await _paymentService.GetTotalPriceAsync(cartId);
            var totalPriceFromTempData = totalPrice.HasValue ? totalPrice.Value.ToString("F2") : "0.00";

            var viewModel = new PaymentOptionsViewModel
            {
                TotalPrice = totalPrice,
                PaymentOptions = paymentOptions
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> PaymentWidget(PaymentRequestModel model)
        {

            var cartIdCookie = Request.Cookies["CartId"];
            if (string.IsNullOrEmpty(cartIdCookie) || !int.TryParse(cartIdCookie, out int cartId))
            {
                return RedirectToAction("Error", "Home");
            }

            var userId = User.Identity.Name; // Get the logged-in user's ID
            int nextSyncId = await _syncIdService.GetNextSyncIdAsync(userId, cartId);

            var cartItems = await _context.ShoppingCartItems
                .Where(c => c.ShoppingCartId == cartId)
                .ToListAsync();
            decimal? totalPrice = cartItems.Sum(item => item.Quantity * item.UnitPrice);
            int assetPrice = totalPrice.HasValue ? (int)(totalPrice.Value * 100) : 0;
            //decimal? totalPrice = await _paymentService.GetTotalPriceAsync(cartId);
            //int assetPrice = totalPrice.HasValue ? (int)totalPrice.Value : 0;

            var client = _httpClientFactory.CreateClient();
            var requestUrl = "https://testpos.payby.me/webpayment/Request.aspx"; 

            var parameters = new Dictionary<string, string>
            {
                { "username", Username },
                { "token", Token },
                { "syncId", nextSyncId.ToString() },
                { "keywordId", KeywordId.ToString() },
                { "subCompany", "ZeynepTech" },
                { "assetName", "Elenktronik" },
                { "assetPrice", assetPrice.ToString() },
                { "clientIp", "176.236.74.26" },
                { "countryCode", "TR" },
                { "currencyCode", "TRY" },
                { "languageCode", "try" },
                { "notifyPage", "https://localhost:7181/Payment" },
                { "redirectPage", "https://localhost:7181/Payment" },
                { "errorPage", "https://localhost:7181/Payment" },
                { "paymentType", "vpos" }
            };

            var encodedContent = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync(requestUrl, encodedContent);
            var responseText = await response.Content.ReadAsStringAsync();

            var responseParams = HttpUtility.ParseQueryString(responseText);

            if (responseParams["Status"] == "1")
            {
                var hash = responseParams["ErrorDesc"];
                return Redirect($"{PaymentPageUrl}?hash={hash}");
            }
            else
            {
                return RedirectToAction("ErrorPage", new { errorCode = responseParams["ErrorCode"], errorDesc = responseParams["ErrorDesc"] });
            }
        }

        public IActionResult PaymentPage(string hash)
        {
            ViewBag.Hash = hash;
            return View();
        }

        public IActionResult RedirectPage()
        {
            return View();
        }

        public IActionResult ErrorPage(string errorCode, string errorDesc)
        {
            ViewBag.ErrorCode = errorCode ?? "Unknown error";
            ViewBag.ErrorDesc = errorDesc ?? "No description available";
            return View("Error");
        }

        [HttpPost]
        public IActionResult NotifyPage()
        {
            return Content("OK");
        }
        
    }
}
