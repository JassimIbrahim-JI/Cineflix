using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieAppProject.Data;
using MovieAppProject.Interfaces;
using MovieAppProject.Models;
using MovieAppProject.Models.ViewModels.Movies;
using System.Diagnostics;
using System.Security.Claims;

namespace MovieAppProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMovieRepository _movieRepository;
        private readonly MovieDbContext _context;
        public HomeController(ILogger<HomeController> logger, IMovieRepository movieRepository, MovieDbContext context)
        {
            _logger = logger;
            _movieRepository = movieRepository;
            _context = context;
        }

        public async Task<IActionResult> Index(string sortOrder, string currentFilter,string searchTerm, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["TitleSortForm"] = string.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["DateSortForm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["PriceSortForm"] = sortOrder == "Price" ? "price_desc" : "Price";

            if (searchTerm != null)
            {
                pageNumber = 1;
            }
            else 
            {
              searchTerm = currentFilter;
            }
            ViewData["CurrentFilter"] = searchTerm;

            var featuredMovies = await _movieRepository.GetFeaturedMoviesAsync(5);
            ViewData["FeaturedMovies"] = featuredMovies;

            int pageSize = 6;
            var movies = await _movieRepository.GetPaginedAsync(pageNumber ?? 1, pageSize, sortOrder, searchTerm);
            return View(movies);
        }

        public async Task<IActionResult> NewAndPopular()
        {
            // Get recently released movies (last 30 days)
            var recentMovies = await _context.Movies
                .Where(m => m.ReleaseDate >= DateTime.Now.AddDays(-30))
                .OrderByDescending(m => m.ReleaseDate)
                .Take(12)
                .ToListAsync();

            // Get popular movies (highest rated)
            var popularMovies = await _context.Movies
                .Where(m => m.Rating >= 3.0)
                .OrderByDescending(m => m.Rating)
                .Take(12)
                .ToListAsync();

            var viewModel = new NewAndPopularViewModel
            {
                RecentMovies = recentMovies,
                PopularMovies = popularMovies
            };

            return View(viewModel);
        }

        [Authorize]
        public async Task<IActionResult> MyList()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wishlistItems = await _context.WishlistItems
                .Include(w => w.Movie)
                .Where(w => w.UserId == userId)
                .ToListAsync();

            return View(wishlistItems);
        }

        public async Task<IActionResult>Details(int id)
        {
            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null)
                return NotFound();

            return View(movie);
        }


        [HttpGet]
        public async Task<IActionResult> Play(int id, bool isTrailer = false)
        {
            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            if (isTrailer)
            {
                ViewData["IsTrailer"] = true;
                return View(movie);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var hasPurchased = await _context.Purchases
                .AnyAsync(p => p.UserId == userId && p.PurchaseItems.Any(pi => pi.MovieId == id));

            if (!hasPurchased && !User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            // Only validate non-YouTube URLs
            if (!movie.VideoUrl.Contains("youtube.com"))
            {
                try
                {
                    using var httpClient = new HttpClient();
                    var response = await httpClient.GetAsync(movie.VideoUrl, HttpCompletionOption.ResponseHeadersRead);
                    if (!response.IsSuccessStatusCode)
                    {
                        ViewData["VideoError"] = "Video is currently unavailable. Please try again later.";
                    }
                }
                catch
                {
                    ViewData["VideoError"] = "Video is currently unavailable. Please try again later.";
                }
            }
            ViewData["IsTrailer"] = false;
            return View(movie);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
