using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAppProject.Data;
using MovieAppProject.Interfaces;
using System.Security.Claims;

namespace MovieAppProject.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly IMovieRepository _movieRepository;
        private readonly MovieDbContext _context;

        public WishlistController(IMovieRepository movieRepository, MovieDbContext context)
        {
            _movieRepository = movieRepository;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wishlistItems = await _context.WishlistItems
                .Include(w => w.Movie)
                .Where(w => w.UserId == userId)
                .ToListAsync();

            if (User.Identity.IsAuthenticated)
            {
                var moviesInCart = await _movieRepository.GetCartItemsAsync(userId);
                ViewData["MoviesInCart"] = moviesInCart.Select(c => c.MovieId).ToList();
            }

            return View(wishlistItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToWishlist(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _movieRepository.AddItemToWishlistAsync(userId, id);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    message = "Item added to wishlist"
                });
            }

            return RedirectToAction("Details", "Home", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromWishlist(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _movieRepository.RemoveItemFromWishlistAsync(userId, id);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    message = "Item removed from wishlist"
                });
            }

            return RedirectToAction("Details", "Home", new { id });
        }

    }
}
