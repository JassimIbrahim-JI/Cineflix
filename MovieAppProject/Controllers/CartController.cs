using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAppProject.Data;
using MovieAppProject.Interfaces;
using MovieAppProject.Models;
using MovieAppProject.Models.ViewModels.Carts;
using Stripe;
using System.Security.Claims;

namespace MovieAppProject.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IMovieRepository _movieRepository;
        private readonly MovieDbContext _context;
        private readonly ILogger<CartController> _logger;
        private readonly IConfiguration _configuration;

        public CartController(IMovieRepository movieRepository, MovieDbContext context, ILogger<CartController> logger, IConfiguration configuration)
        {
            _movieRepository = movieRepository;
            _context = context;
            _logger = logger;
            _configuration = configuration;

            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = await _movieRepository.GetCartItemsAsync(userId);

            var viewModel = new CartViewModel
            {
                CartItems = cartItems.ToList(),
                TotalAmount = cartItems.Sum(ci => ci.Movie.Price * ci.Quantity)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _movieRepository.AddMovieToCartAsync(userId, id);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var count = await _context.CartItems.Where(c => c.UserId == userId)
                    .SumAsync(c => c.Quantity);

                if (result)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Item added to cart",
                        count = count
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Movie is already in your cart",
                        count = count
                    });
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _movieRepository.RemoveFromCartAsync(userId, id);

            // if it's an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // Return JSON response for AJAX requests
                var count = await _context.CartItems.Where(c => c.UserId == userId)
                    .SumAsync(c => c.Quantity);

                return Json(new
                {
                    success = true,
                    message = "Item removed from cart",
                    count = count
                });
            }

            // Return redirect for normal requests
            return RedirectToAction("Index");
        }


        // Quantity updates are not required:
        // a movie can only be purchased once and remains fixed in the cart(Fixed Qty = 1)

        /* [HttpPost]
         public async Task<IActionResult> UpdateQuantity(int id, int quantity)
         {
             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
             var cartItem = await _movieRepository.GetCartItemAsync(userId, id);

             if (cartItem != null)
             {
                 cartItem.Quantity = quantity;
                 await _context.SaveChangesAsync();
             }

             return RedirectToAction("Index");
         }*/

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = await _movieRepository.GetCartItemsAsync(userId);

            if (!cartItems.Any())
            {
                TempData["CheckoutError"] = "Your cart is empty";
                return RedirectToAction("Index");
            }

            // Check movie availability
            foreach (var item in cartItems)
            {
                var movie = await _movieRepository.GetByIdAsync(item.MovieId);
                if (movie == null)
                {
                    TempData["Error"] = $"{item.Movie.Title} is no longer available";
                    return RedirectToAction("Index");
                }
            }
            var viewModel = new CartViewModel
            {
                CartItems = cartItems.ToList(),
                TotalAmount = cartItems.Sum(ci => ci.Movie.Price * ci.Quantity)
            };
            ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentIntent()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = await _movieRepository.GetCartItemsAsync(userId);

            if (!cartItems.Any())
            {
                return Json(new { error = "Your cart is empty" });
            }

            try
            {
                var totalAmount = cartItems.Sum(ci => ci.Movie.Price * ci.Quantity);
                var taxAmount = totalAmount * 0.1m;
                var finalAmount = (long)((totalAmount + taxAmount) * 100); // Convert to cents

                var options = new PaymentIntentCreateOptions
                {
                    Amount = finalAmount,
                    Currency = "qar",
                    Metadata = new Dictionary<string, string>
                    {
                        { "userId", userId }
                    }
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                return Json(new { clientSecret = paymentIntent.ClientSecret });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating payment intent");
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPaymentSuccess(string paymentIntentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Json(new { success = false, message = "User not logged in." });

            var cartItems = await _movieRepository.GetCartItemsAsync(userId);

            if (!cartItems.Any())
                return Json(new { success = false, message = "Cart is empty." });

            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);

                if (paymentIntent.Status != "succeeded")
                    return Json(new { success = false, message = "Payment failed." });

                // Create purchase with paymentIntentId
                var purchase = await _movieRepository.CreatePurchaseAsync(userId, paymentIntentId, cartItems.ToList());

                return Json(new { success = true, purchaseId = purchase.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for user {UserId}", userId);
                return Json(new
                {
                    success = false,
                    message = $"Unexpected error: {ex.Message} {ex.InnerException?.Message}"
                });
            }
        }

        public async Task<IActionResult> PurchaseComplete(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Movie)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null || purchase.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound();
            }

            return View(purchase);
        }

        [HttpGet]
        public async Task<IActionResult> GetCartItemCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var count = await _context.CartItems.Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity);

            return Json(new { count });
        }
    }
}