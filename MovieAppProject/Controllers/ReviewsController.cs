using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAppProject.Data;
using MovieAppProject.Models;
using System.Security.Claims;

namespace MovieAppProject.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly MovieDbContext _context;
        private readonly UserManager<MovieUser> _userManager;

        public ReviewsController(MovieDbContext context, UserManager<MovieUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int movieId, int rating, string content)
        {
            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            // Check if user already reviewed this movie
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.MovieId == movieId && r.UserId == userId);

            if (existingReview != null)
            {
                TempData["ErrorMessage"] = "You've already reviewed this movie.";
                return RedirectToAction("Details", "Home", new { id = movieId });
            }

            var review = new Review
            {
                MovieId = movieId,
                UserId = userId,
                Rating = rating,
                Content = content,
                ReviewDate = DateTime.UtcNow,
                User = user
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Update movie average rating
            await UpdateMovieRating(movieId);

            TempData["SuccessMessage"] = "Thank you for your review!";
            return RedirectToAction("Details", "Home", new { id = movieId });
        }

        // POST: Reviews/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int movieId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            if (review.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            // Update movie average rating
            await UpdateMovieRating(movieId);

            TempData["SuccessMessage"] = "Review deleted successfully.";
            return RedirectToAction("Details", "Home", new { id = movieId });
        }

        private async Task UpdateMovieRating(int movieId)
        {
            var movie = await _context.Movies
                .Include(m => m.Reviews)
                .FirstOrDefaultAsync(m => m.Id == movieId);

            if (movie != null && movie.Reviews.Any())
            {
                movie.Rating = (float)Math.Round(movie.Reviews.Average(r => r.Rating), 1);
                _context.Movies.Update(movie);
                await _context.SaveChangesAsync();
            }
        }
    }
}
