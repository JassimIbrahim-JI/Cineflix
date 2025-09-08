using MovieAppProject.Helpers;
using MovieAppProject.Models;
using System.Collections;

namespace MovieAppProject.Interfaces
{
    public interface IMovieRepository
    {
        Task<Movie> GetByIdAsync(int id, bool isIncludeRelated = true);
        Task<IEnumerable<Movie>> GetAllAsync();
        Task<List<Movie>> GetFeaturedMoviesAsync(int count);
        Task<PaginatedList<Movie>> GetPaginedAsync(int pageIndex, int pageSize, string sortOrder, string searchTerm);
        Task AddAsync(Movie movie);
        Task UpdateAsync(Movie movie);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);

        // AdditionalMethods for complex relationships Like Cart, Wishlist and Actors

        Task AddActorToMovieAsync(int movieId,int actorId,string characterName);
        Task RemoveActorFromMovieAsync(int movieId, int actorId);
        Task AddItemToWishlistAsync(string userId, int movieId);
        Task RemoveItemFromWishlistAsync(string userId, int movieId);
        Task<bool> AddMovieToCartAsync(string userId, int movieId);
        Task RemoveFromCartAsync(string userId, int movieId);
        Task<Cart> GetCartItemAsync(string userId, int movieId);
        Task<IEnumerable<Cart>> GetCartItemsAsync(string userId);
        Task<Purchase> CreatePurchaseAsync(string userId, string paymentIntentId, List<Cart> cartItems);


        // Review Methods 
        Task AddReview(Review review);
        Task<Review> GetReviewByIdAsync(int reviewId);
        Task<Review> GetUserReviewForMovieAsync(string userId, int movieId);
        Task DeleteReviewAsync(int reviewId);
        Task UpdateMovieRatingAsync(int movieId);
    }
}
