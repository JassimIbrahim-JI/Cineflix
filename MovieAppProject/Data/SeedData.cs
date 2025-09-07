using Microsoft.AspNetCore.Identity;
using MovieAppProject.Models;

namespace MovieAppProject.Data
{
    public static class SeedData
    {
       public static async Task Initialize(MovieDbContext context,UserManager<MovieUser>userManager, RoleManager<IdentityRole> roleManager)
        {
            context.Database.EnsureCreated();

            string[] roles = new[] {"User","Admin"};

            foreach(var role in roles)
            {
                if(!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "admin@movie.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null) 
            {
                adminUser = new MovieUser
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = adminEmail,
                    UserName = adminEmail,
                    DateOfBirth = new DateTime(1998, 10, 19),
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(adminUser, "Admin@1122");
                await userManager.AddToRoleAsync(adminUser, "Admin");


            }

            var userEmail = "user@movie.com";
            var regularUser = await userManager.FindByEmailAsync(userEmail);
            if (regularUser == null)
            {
                regularUser = new MovieUser
                {
                    FirstName = "User",
                    LastName = "Name",
                    DateOfBirth =new DateTime(1998,10,19),
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(regularUser, "User@1122");
                await userManager.AddToRoleAsync(regularUser, "User");
            }
            // Seed movies if none exist
            if (!context.Movies.Any())
            {
                var movies = new Movie[]
                {
                    new Movie
                    {
                     Title = "The Shawshank Redemption",
                    Description = "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency.",
                    ReleaseDate = DateTime.Parse("1994-10-14"),
                    Genre = "Drama",
                    Price = 9.99M,
                    Director = "Frank Darabont",
                    Rating = 4.5f,
                    ImageUrl = "https://m.media-amazon.com/images/M/MV5BMDFkYTc0MGEtZmNhMC00ZDIzLWFmNTEtODM1ZmRlYWMwMWFmXkEyXkFqcGdeQXVyMTMxODk2OTU@._V1_.jpg",
                    VideoUrl = "https://www.youtube.com/watch?v=6hB3S9bIaco",
                    DurationMinutes = 142
                    },
                     new Movie
                    {
                    Title = "The Godfather",
                    Description = "The aging patriarch of an organized crime dynasty transfers control of his clandestine empire to his reluctant son.",
                    ReleaseDate = DateTime.Parse("1972-03-24"),
                    Genre = "Crime, Drama",
                    Price = 8.99M,
                    Director = "Francis Ford Coppola",
                    Rating = 4.9f,
                    ImageUrl = "https://m.media-amazon.com/images/M/MV5BM2MyNjYxNmUtYTAwNi00MTYxLWJmNWYtYzZlODY3ZTk3OTFlXkEyXkFqcGdeQXVyNzkwMjQ5NzM@._V1_.jpg",
                    VideoUrl = "https://www.youtube.com/watch?v=sY1S34973zA",
                    DurationMinutes = 175
                    }
                };

                context.Movies.AddRange(movies);
                await context.SaveChangesAsync();
            }

            // Seed actors if none exist
            if (!context.Actors.Any())
            {
                var actors = new Actor[]
                {
                new Actor
                {
                    Name = "Tim Robbins",
                    Bio = "American actor and filmmaker",
                    DateOfBirth = DateTime.Parse("1958-10-16"),
                    ImageUrl = "https://m.media-amazon.com/images/M/MV5BMTI1OTYxNzAxOF5BMl5BanBnXkFtZTYwNTE5ODI4._V1_.jpg"
                },
                new Actor
                {
                    Name = "Morgan Freeman",
                    Bio = "American actor, director, and narrator",
                    DateOfBirth = DateTime.Parse("1937-06-01"),
                    ImageUrl = "https://m.media-amazon.com/images/M/MV5BMTc0MDMyMzI2OF5BMl5BanBnXkFtZTcwMzM2OTk1MQ@@._V1_.jpg"
                },
                    // Add more actors...
                };

                context.Actors.AddRange(actors);
                await context.SaveChangesAsync();
            }

            // Seed movie-actor relationships if none exist
            if (!context.MovieActors.Any())
            {
                var movieActors = new MovieActor[]
                {
                new MovieActor
                {
                    MovieId = 1,
                    ActorId = 1,
                    CharacterName = "Andy Dufresne"
                },
                new MovieActor
                {
                    MovieId = 1,
                    ActorId = 2,
                    CharacterName = "Ellis Boyd 'Red' Redding"
                },
                   
                };

                context.MovieActors.AddRange(movieActors);
                await context.SaveChangesAsync();
            }

        }
    }
}
