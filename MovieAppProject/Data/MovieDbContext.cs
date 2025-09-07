using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MovieAppProject.Models;
using System.Reflection.Emit;

namespace MovieAppProject.Data
{
    public class MovieDbContext : IdentityDbContext<MovieUser>
    {
        public MovieDbContext(DbContextOptions<MovieDbContext>options):base(options)
        {
            
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<MovieActor> MovieActors { get; set; }
        public DbSet<Cart> CartItems { get; set; }
        public DbSet<Wishlist> WishlistItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Purchase>Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<MovieActor>(entity =>
            {
                entity.HasKey(ma => new { ma.MovieId, ma.ActorId });

                entity.HasOne(ma => ma.Actor).WithMany(a => a.MovieActors)
                .HasForeignKey(ma => ma.ActorId);

                entity.HasOne(ma => ma.Movie).WithMany(m => m.MovieActors)
                .HasForeignKey(ma => ma.MovieId);
            });

            builder.Entity<Review>()
                .HasOne(r => r.Movie)
                .WithMany(m => m.Reviews)
                .HasForeignKey(r => r.MovieId);

            builder.Entity<Wishlist>()
                .HasOne(w => w.Movie)
                .WithMany(m => m.WishlistItems)
                .HasForeignKey(w => w.MovieId);

            builder.Entity<Cart>()
                .HasOne(c => c.Movie)
                .WithMany(m => m.CartItems)
                .HasForeignKey(c => c.MovieId);

            builder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Movie)
                .WithMany(m => m.Purchases)
                .HasForeignKey(pi => pi.MovieId);

            builder.Entity<PurchaseItem>()
               .HasOne(pi => pi.Purchase)
               .WithMany(p => p.PurchaseItems)
               .HasForeignKey(pi => pi.PurchaseId);

        }
    }
}
