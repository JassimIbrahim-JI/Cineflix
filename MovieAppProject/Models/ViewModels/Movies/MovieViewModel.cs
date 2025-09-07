using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MovieAppProject.Models.ViewModels.Movies
{
    public class MovieViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The Title field is required")]
        [StringLength(100, ErrorMessage = "The Title must be at most 100 characters long.")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "The Description must be at most 500 characters long.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "The release date field is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Release Date")]
        public DateTime ReleaseDate { get; set; }

        [Required(ErrorMessage = "The Genre field is required.")]
        [StringLength(100, ErrorMessage = "The Genre must be at most 100 characters long.")]
        public string Genre { get; set; }

        [Required(ErrorMessage = "The Price field is required.")]
        [Range(0.01, 1000, ErrorMessage = "Price must be between $0.01 and $1000")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [StringLength(100, ErrorMessage = "The Director must be at most 100 characters long.")]
        public string Director { get; set; }

        [Range(1, 5, ErrorMessage = " ")]
        public float Rating { get; set; }
        public string? ImageUrl { get; set; }

        [StringLength(255, ErrorMessage = "The Video URL must be at most 255 characters long.")]
        [Display(Name = "Video URL")]
        [Url(ErrorMessage = "Please enter a valid Url")]
        public string VideoUrl { get; set; }

        [Range(1, 500, ErrorMessage = "Duration must be between 1 and 500 minutes.")]
        [Display(Name = "Duration (minutes)")]
        public int DurationMinutes { get; set; }

        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        //[Display(Name = "Actors")]
        // public List<int> SelectedActorIds { get; set; } = new List<int>();
        public List<MovieActorInputModel> Actors { get; set; } = new List<MovieActorInputModel>();
        public List<SelectListItem> AvailableActors { get; set; } = new List<SelectListItem>();
    }
}
