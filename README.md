# 🎬 Cineflix – ASP.NET Core Movie App

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-blue)](https://dotnet.microsoft.com/apps/aspnet)
[![EF Core](https://img.shields.io/badge/EF_Core-8.0-green)](https://learn.microsoft.com/en-us/ef/core/)
[![Stripe](https://img.shields.io/badge/Stripe-Payments%20API-%23008CDD)](https://stripe.com)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.0-7952B3)](https://getbootstrap.com/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-Database-CC2927)](https://www.microsoft.com/en-us/sql-server)

Cineflix is a **full-stack movie streaming & purchasing platform** built with **ASP.NET Core MVC (.NET 8)**, **Entity Framework Core**, **Identity for Authentication/Authorization**, and **Stripe** for secure payments.  
It allows users to explore, wishlist, purchase, and review movies, while admins manage content through a dashboard.

---

## 📖 Table of Contents
- [Demo Video](#-demo-video)
- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Installation & Setup](#-installation--setup)
- [Example Code](#-example-code)
- [Contribution](#-contribution)
- [Author](#-author)

---

## 📽️ Demo Video
[![Watch the demo](./MovieAppProject/wwwroot/assets/demo-preview.png)](https://drive.google.com/file/d/1Vn3lUnltd97inZea9MVy4VIOscAlOgge/view?usp=drive_link)

---

## ✨ Features

### 👤 Authentication & Users
- ASP.NET Core Identity (Register, Login, Forgot/Reset Password, Roles)
- JWT-secured endpoints for RESTful APIs
- Admin role management (toggle Admin/User)

### 🎬 Movies
- Full CRUD operations for movies with actors, reviews, ratings
- Paginated, sortable, and searchable movie listing
- Featured, Recent, and Popular movies sections

### ❤️ Wishlist & 🛒 Cart
- Add/remove movies from wishlist
- Add movies to shopping cart with quantity updates
- Checkout process with **Stripe Payment Integration**

### ⭐ Reviews
- Authenticated users can add/delete reviews
- Movies have **average ratings** auto-updated

### 📊 Admin Dashboard
- Statistics (Movies count, Users, Reviews, Revenue)
- Manage movies, actors, reviews, and users
- Recent activities and quick actions

### 💳 Payments
- Stripe integration for payments
- Support for multiple items checkout
- Purchase history and invoice tracking

---

## 🛠️ Tech Stack

- **Backend:** ASP.NET Core MVC 8, C#
- **Frontend:** Razor Views, Bootstrap 5, Vanilla JS
- **Database:** SQL Server + EF Core
- **Authentication:** ASP.NET Identity (with Roles)
- **Payments:** Stripe (Payment Intents API)
- **APIs:** RESTful endpoints for movies, wishlist, and cart
- **Architecture:** Repository & Unit of Work patterns(Not Implement in this project)

---

## 📂 Project Structure

```plaintext
MovieAppSolution/
│
├── Controllers/
│   ├── HomeController.cs
│   ├── CartController.cs
│   ├── ReviewsController.cs
│   ├── WishlistController.cs
│   └── AdminController.cs
│
├── Repositories/
│   ├── IMovieRepository.cs
│   └── MovieRepository.cs
│
├── Models/
│   ├── Movie.cs
│   ├── Actor.cs
│   ├── Review.cs
│   ├── Wishlist.cs
│   ├── Cart.cs
│   └── Purchase.cs
│
├── Views/
│   ├── Home/
│   ├── Cart/
│   ├── Reviews/
│   ├── Wishlist/
│   └── Admin/
│
├── Data/
│   └── MovieDbContext.cs
│
└── wwwroot/
    ├── css/
    ├── js/
    └── images/
```

---

## 🚀 Installation & Setup

Make sure you have .NET 8 SDK and SQL Server installed.

```bash
# Clone the repository
git clone https://github.com/JassimIbrahim-JI/Cineflix.git
cd Cineflix

# Restore dependencies
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the application
dotnet run
```

---


## 📜 Example Code – Repository Pattern

```csharp
   public async Task<bool> AddMovieToCartAsync(string userId, int movieId)
   {
       var existingCart = await _db.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.MovieId == movieId);

       if (existingCart != null)
       {
          
           return false;
       }

       var cartItem = new Cart
       {
           UserId = userId,
           MovieId = movieId,
           Quantity = 1,
           AddedDate = DateTime.UtcNow
       };

       await _db.CartItems.AddAsync(cartItem);
       await _db.SaveChangesAsync();
       return true;
   }
```

---

### 🤝 Contribution

1. Fork the repo.

2. Create a feature branch:
    
    ```bash
    git checkout -b feature/my-feature
    ```

3. Commit your changes:
    
    ```bash
    git commit -m "Add my feature"
    ```

4. Push & open a PR:
    
    ```bash
    git push origin feature/my-feature
    ```

---

### 👤 Author
```markdown
Built with ❤️ by Jassim Ibrahim (JI)
