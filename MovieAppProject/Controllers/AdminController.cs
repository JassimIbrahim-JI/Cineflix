using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieAppProject.Interfaces;
using MovieAppProject.Models;
using MovieAppProject.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieAppProject.Models.ViewModels.User;
using MovieAppProject.Models.ViewModels.Reviews;
using MovieAppProject.Models.ViewModels.Movies;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MovieAppProject.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        private readonly IMovieRepository _movieRepository;
        private readonly MovieDbContext _context;
        private readonly UserManager<MovieUser> _userManager;
        public AdminController(IMovieRepository movieRepository, MovieDbContext context, UserManager<MovieUser> userManager)
        {
            _movieRepository = movieRepository;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Get Satrictics for dashboard UI
            ViewBag.MovieCount = await _context.Movies.CountAsync();
            ViewBag.UserCount = await _userManager.Users.CountAsync();
            ViewBag.ReviewCount = await _context.Reviews.CountAsync();
            ViewBag.TotalRevenue = await _context.Purchases.SumAsync(p=>p.TotalAmount);

            // Now i will get recent movies Order By Id DESC and take just 5 recent movies
            ViewBag.RecentMovies = await _context.Movies.OrderByDescending(m => m.Id)
                .Take(5)
                .ToListAsync();

            // also recent reviews
            ViewBag.RecentReviews = await _context.Reviews.Include(r=>r.User).Include(r=>r.Movie)
                .OrderByDescending(r => r.ReviewDate)
                .Take(5).Select(r=>new ReviewViewModel
                {
                    MovieTitle = r.Movie.Title,
                    Username = r.User.Email,
                    Rating = r.Rating,
                    ReviewDate = r.ReviewDate
                }).ToListAsync();


            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MovieList()
        {
            var movies = await _movieRepository.GetAllAsync();
            return View(movies);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new MovieViewModel
            {
                AvailableActors = _context.Actors
            .Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.Name
            }).ToList()
            };
            // Add an initial empty actor input model to the list
            model.Actors.Add(new MovieActorInputModel());

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableActors = _context.Actors
                   .Select(a => new SelectListItem
                   {
                       Value = a.Id.ToString(),
                       Text = a.Name
                   }).ToList();

                var errors = ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) });

                
                foreach (var error in errors)
                {
                    Console.WriteLine($"Field: {error.Field}, Errors: {string.Join(", ", error.Errors)}");
                }

                return View(model);
            }

            try
            {

                string imagePath = model.ImageUrl;
                if (model.ImageFile != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ImageFile.FileName)}";
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    imagePath = $"/images/{fileName}";
                }

                var movie = new Movie
                {
                    Title = model.Title,
                    Genre = model.Genre, 
                    Description = model.Description,
                    Director = model.Director,
                    DurationMinutes = model.DurationMinutes,
                    ReleaseDate = model.ReleaseDate,
                    Price = model.Price,
                    ImageUrl = imagePath,
                    VideoUrl = model.VideoUrl,
                    Rating = model.Rating,
                    MovieActors = model.Actors.Where(a=>a.ActorId != 0 && !string.IsNullOrEmpty(a.CharacterName))
                    .Select(a => new MovieActor
                    {
                        ActorId = a.ActorId,
                        CharacterName = a.CharacterName
                    }).ToList() ?? new List<MovieActor>()
                };

                await _movieRepository.AddAsync(movie);
                TempData["MovieCreated"] = "Movie created successfully!";
                return RedirectToAction(nameof(MovieList));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message + " " + ex.InnerException?.Message);
                // Reload actors for the dropdown
                model.AvailableActors = _context.Actors
                   .Select(a => new SelectListItem
                   {
                       Value = a.Id.ToString(),
                       Text = a.Name
                   }).ToList();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null) return NotFound();

            var model = new MovieViewModel
            {
                Id = id,
                Title = movie.Title,
                Genre = movie.Genre,
                Description = movie.Description,
                Director = movie.Director,
                DurationMinutes = movie.DurationMinutes,
                ReleaseDate = movie.ReleaseDate,
                Price = movie.Price,
                ImageUrl = movie.ImageUrl,
                VideoUrl = movie.VideoUrl,
                Rating = movie.Rating,
                Actors = movie.MovieActors
                    .Select(ma => new MovieActorInputModel
                    {
                        ActorId = ma.ActorId,
                        CharacterName = ma.CharacterName
                    }).ToList(),
                AvailableActors = await _context.Actors.Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                }).ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MovieViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableActors = _context.Actors
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Name
                    }).ToList();
                return View(model);
            }

            try
            {
                var existingMovie = await _movieRepository.GetByIdAsync(model.Id);
                if (existingMovie == null) return NotFound();

                if (model.ImageFile != null)
                {
                   
                    if (!string.IsNullOrEmpty(existingMovie.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingMovie.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                   
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ImageFile.FileName)}";
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    using (var fileStream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    existingMovie.ImageUrl = $"/images/{fileName}";
                }

                existingMovie.Title = model.Title;
                existingMovie.Genre = model.Genre;
                existingMovie.Description = model.Description;
                existingMovie.Director = model.Director;
                existingMovie.DurationMinutes = model.DurationMinutes;
                existingMovie.ReleaseDate = model.ReleaseDate;
                existingMovie.Price = model.Price;
                existingMovie.VideoUrl = model.VideoUrl;
                existingMovie.Rating = model.Rating;

                // update actors
                existingMovie.MovieActors.Clear();
                foreach (var actorInput in model.Actors)
                {
                    existingMovie.MovieActors.Add(new MovieActor
                    {
                        ActorId = actorInput.ActorId,
                        CharacterName = actorInput.CharacterName
                    });
                }
                await _movieRepository.UpdateAsync(existingMovie);
                return RedirectToAction(nameof(MovieList));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.AvailableActors = _context.Actors
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Name
                    }).ToList();
                return View(model);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null) return NotFound();

            return View(movie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _movieRepository.GetByIdAsync(id);
            if (movie == null) return NotFound();

            if (!string.IsNullOrEmpty(movie.ImageUrl))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", movie.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await _movieRepository.DeleteAsync(id);
            return RedirectToAction(nameof(MovieList));
        }

        [HttpGet]
        public async Task<IActionResult> AllActors()
        {
            var actors = await _context.Actors.ToListAsync();
            return View(actors);
        }

        [HttpGet]
        public IActionResult CreateActor()
        {
            return View(new MovieActorViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateActor(MovieActorViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var actor = new Actor
                    {
                        Name = model.Name,
                        Bio = model.Bio,
                        DateOfBirth = model.Birthday,
                        ImageUrl = model.ImageUrl
                    };

                    _context.Actors.Add(actor);
                    await _context.SaveChangesAsync();
                    TempData["ActorSuccess"] = "Actor created successfully!";
                    return RedirectToAction(nameof(AllActors));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating actor: {ex.Message}");
                }
            }

          /*  var errors = ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) });*/

            // Set breakpoint here or log errors
          /*  foreach (var error in errors)
            {
                Console.WriteLine($"Field: {error.Field}, Errors: {string.Join(", ", error.Errors)}");
            }*/

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> EditActor(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor == null) return NotFound();

            var model = new MovieActorViewModel
            {
                Id = actor.Id,
                Name = actor.Name,
                Bio = actor.Bio,
                Birthday = actor.DateOfBirth,
                ImageUrl = actor.ImageUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditActor(MovieActorViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var actor = await _context.Actors.FindAsync(model.Id);
            if (actor == null) return NotFound();

            actor.Name = model.Name;
            actor.Bio = model.Bio;
            actor.DateOfBirth = model.Birthday;
            actor.ImageUrl = model.ImageUrl;

            _context.Actors.Update(actor);
            await _context.SaveChangesAsync();

            TempData["ActorUpdateSuccess"] = "Actor updated successfully!";
            return RedirectToAction(nameof(AllActors));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteActorConfirmed(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor != null)
            {
                _context.Actors.Remove(actor);
                await _context.SaveChangesAsync();
                TempData["ActorSuccess"] = "Actor deleted successfully!";
            }
            return RedirectToAction(nameof(AllActors));
        }


        [HttpGet]
        public async Task<IActionResult> ManageUsers() 
        {
            var users = await _userManager.Users
                .Include(u => u.Reviews)
                 .Include(u => u.Purchases)
                 .ToListAsync();

            var userViewModel = users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                DateOfBirth = u.DateOfBirth,
                ReviewCount = u.Reviews.Count,
                PurchaseCount = u.Purchases.Count,
                IsAdmin = _userManager.IsInRoleAsync(u,"Admin").Result
            }).ToList();

            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult>ToggleAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if(await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }
            return RedirectToAction(nameof(ManageUsers));
        }

        [HttpGet]
        public async Task<IActionResult> ManageReviews(int pageNumber = 1, int pageSize= 10)
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Movie)
                .OrderByDescending(r => r.ReviewDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize).ToListAsync();

            var totalCount = await _context.Reviews.CountAsync();

            var model = new ReviewListViewModel
            {
                Reviews = reviews.Select(r=>new ReviewViewModel
                {
                    Id = r.Id,
                    MovieId = r.MovieId,
                    Content = r.Content,
                    Rating = r.Rating,
                    MovieTitle = r.Movie.Title,
                    Username = r.User.FullName,
                    ReviewDate = r.ReviewDate
                }).ToList(),
                PageInfo = new PageInfo
                {
                    CurrentPage = pageNumber,
                    ItemsPerPage = pageSize,
                    TotalItems = totalCount
                }
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult>DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            // update movie rating 
            await UpdateMovieRating(review.MovieId);

            return RedirectToAction(nameof(ManageReviews));
        }

        private async Task UpdateMovieRating(int movieId)
        {
            var movie = await _movieRepository.GetByIdAsync(movieId);
            if(movie !=null && movie.Reviews.Any())
            {
                movie.Rating = (float)Math.Round(movie.Reviews.Average(r => r.Rating), 1);
                _context.Movies.Update(movie);
                await _context.SaveChangesAsync();
            }
        }
    }
}
