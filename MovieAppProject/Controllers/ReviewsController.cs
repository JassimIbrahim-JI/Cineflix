using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAppProject.Data;
using MovieAppProject.Interfaces;
using MovieAppProject.Models;
using System.Security.Claims;

namespace MovieAppProject.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly IMovieRepository _movieRepository;
        private readonly UserManager<MovieUser> _userManager;

        public ReviewsController(IMovieRepository movieRepository, UserManager<MovieUser> userManager)
        {
            _movieRepository = movieRepository;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int movieId, int rating, string content)
        {
            var movieExists = await _movieRepository.ExistsAsync(movieId);
            if (!movieExists)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            var existingReview = await _movieRepository.GetUserReviewForMovieAsync(userId, movieId);

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
            await _movieRepository.AddReview(review);

            await _movieRepository.UpdateMovieRatingAsync(movieId);

            TempData["SuccessMessage"] = "Thank you for your review!";
            return RedirectToAction("Details", "Home", new { id = movieId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int movieId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var review = await _movieRepository.GetReviewByIdAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            if (review.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            await _movieRepository.DeleteReviewAsync(id);

            await _movieRepository.UpdateMovieRatingAsync(movieId);

            TempData["SuccessMessage"] = "Review deleted successfully.";
            return RedirectToAction("Details", "Home", new { id = movieId });
        }
    }
}
