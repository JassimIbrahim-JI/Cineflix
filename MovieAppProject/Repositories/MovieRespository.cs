using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieAppProject.Data;
using MovieAppProject.Helpers;
using MovieAppProject.Interfaces;
using MovieAppProject.Models;

namespace MovieAppProject.Repositories
{
    public class MovieRespository : IMovieRepository
    {
        private readonly MovieDbContext _db;
        private readonly UserManager<MovieUser> _userManager;
        public MovieRespository(MovieDbContext db, UserManager<MovieUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        public async Task AddActorToMovieAsync(int movieId, int actorId, string characterName)
        {
            var movieActor = new MovieActor
            {
                ActorId = actorId,
                MovieId = movieId,
                CharacterName = characterName
            };

            await _db.MovieActors.AddAsync(movieActor);
            await _db.SaveChangesAsync();
        }

        public async Task AddAsync(Movie movie)
        {
            await _db.Movies.AddAsync(movie);
            await _db.SaveChangesAsync();
        }

        public async Task AddItemToWishlistAsync(string userId, int movieId)
        {
            var existingItem = await _db.WishlistItems.FirstOrDefaultAsync(w => w.UserId == userId && w.MovieId == movieId);
            if(existingItem == null)
            {
                var wishlistItem = new Wishlist
                {
                    UserId = userId,
                    MovieId = movieId,
                    AddedDate = DateTime.UtcNow
                };

                await _db.WishlistItems.AddAsync(wishlistItem);
                await _db.SaveChangesAsync();
            }
        }

        public async Task AddMovieToCartAsync(string userId, int movieId)
        {
            var existingCart = await _db.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.MovieId == movieId);
            if(existingCart == null)
            {
                var cartItem = new Cart
                {
                    UserId = userId,
                    MovieId = movieId,
                    Quantity = 1,
                    AddedDate = DateTime.UtcNow
                };
                await _db.CartItems.AddAsync(cartItem);
            }
            else
            {
                existingCart.Quantity++;
            }
                await _db.SaveChangesAsync();
        }

        public async Task AddReview(Review review)
        {
            await _db.Reviews.AddAsync(review);
            await _db.SaveChangesAsync();
        }

        public async Task<Purchase> CreatePurchaseAsync(string userId, string paymentIntentId, List<Cart> cartItems)
        {
            if (!cartItems.Any())
                throw new InvalidOperationException("Cart is empty.");

            var purchase = new Purchase
            {
                UserId = userId,
                PurchaseDate = DateTime.UtcNow,
                TotalAmount = cartItems.Sum(ci => ci.Movie.Price * ci.Quantity),
                PaymentIntentId = paymentIntentId, // assign here
                PaymentStatus = "Succeeded",
                PurchaseItems = cartItems.Select(ci => new PurchaseItem
                {
                    MovieId = ci.MovieId,
                    PriceAtPurchase = ci.Movie.Price
                }).ToList()
            };

            await _db.Purchases.AddAsync(purchase);

            // Clear cart
            _db.CartItems.RemoveRange(cartItems);

            await _db.SaveChangesAsync();

            return purchase;
        }


        public async Task DeleteAsync(int id)
        {
            var movie = await GetByIdAsync(id);
            if (movie is null)
                throw new KeyNotFoundException("Movie not found");
           
            _db.Movies.Remove(movie);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _db.Movies.AnyAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Movie>> GetAllAsync()
        {
            return await _db.Movies.Include(m=>m.MovieActors).ThenInclude(ma=>ma.Actor).ToListAsync();
        }

        public async Task<List<Movie>>GetFeaturedMoviesAsync(int count)
        {
            return await _db.Movies.OrderByDescending(m => m.ReleaseDate)
                .Take(count).ToListAsync();
        }
        public async Task<Movie> GetByIdAsync(int id, bool isIncludeRelated = true)
        {
            if (!isIncludeRelated)
            {
                return await _db.Movies.FindAsync(id) ?? throw new KeyNotFoundException("Movie Not found");
            }
            return await _db.Movies.Include(m => m.MovieActors)
                .ThenInclude(ma => ma.Actor)
                .Include(ma => ma.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id) ?? throw new KeyNotFoundException("Movie not found");
        }

        public async Task<Cart> GetCartItemAsync(string userId, int movieId)
        {
            return await _db.CartItems.Include(c => c.Movie)
                 .Where(c => c.UserId == userId && c.MovieId == movieId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Cart>> GetCartItemsAsync(string userId)
        {
            return await _db.CartItems.Include(c => c.Movie).Where(c => c.UserId == userId)
                 .ToListAsync();
        }

        public async Task<PaginatedList<Movie>> GetPaginedAsync(int pageIndex, int pageSize, string sortOrder, string searchTerm)
        {
            IQueryable<Movie> query = _db.Movies.Include(m => m.MovieActors).ThenInclude(ma => ma.Actor);
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(q => q.Title.Contains(searchTerm) || q.Genre.Contains(searchTerm) || q.Description.Contains(searchTerm) || q.Director.Contains(searchTerm)); 
            }
            query = sortOrder switch
            {
                "title_desc" => query.OrderByDescending(m => m.Title),
                "Date" => query.OrderBy(m => m.ReleaseDate),
                "date_desc" => query.OrderByDescending(m => m.ReleaseDate),
                "Price" => query.OrderBy(m => m.Price),
                "price_desc" =>query.OrderByDescending(m=>m.Price),
                _=>query.OrderByDescending(m=>m.Title)
            };
            return await PaginatedList<Movie>.CreateAysnc(query.AsNoTracking(), pageIndex, pageSize);
         }

        public async Task RemoveActorFromMovieAsync(int movieId, int actorId)
        {
            var movieActor = await _db.MovieActors.FindAsync(movieId, actorId);
            if (movieActor != null) 
            {
                _db.MovieActors.Remove(movieActor);
                await _db.SaveChangesAsync();
            }
        }

        public async Task RemoveFromCartAsync(string userId, int movieId)
        {
            var cartItem = await _db.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.MovieId ==movieId);
            if(cartItem is not null)
            {
                if(cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                }
                else
                {
                   _db.CartItems.Remove(cartItem);
                }
                await _db.SaveChangesAsync();
            }    
        }

        public async Task RemoveItemFromWishlistAsync(string userId, int movieId)
        {
            var wishlistItem = await _db.WishlistItems.FirstOrDefaultAsync(w => w.UserId == userId && w.MovieId == movieId);
            if (wishlistItem is not null) 
            {
                _db.WishlistItems.Remove(wishlistItem);
                await _db.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Movie movie)
        {
            _db.Movies.Update(movie);
            await _db.SaveChangesAsync();
        }
    }
}
