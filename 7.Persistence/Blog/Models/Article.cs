using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    /// <summary>
    /// Represents a blog article
    /// </summary>
    public class Article
    {
        /// <summary>
        /// The unique identifier for the article. Assigned at creation.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the author who wrote the article.
        /// </summary>
        [Required(ErrorMessage = "Author name is required")]
        [Display(Name = "Author Name")]
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters")]
        public string AuthorName { get; set; }

        /// <summary>
        /// The email of the author who wrote the article.
        /// </summary>
        [Required(ErrorMessage = "Author email is required")]
        [Display(Name = "Author Email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string AuthorEmail { get; set; }

        /// <summary>
        /// The title of the article. Specified by the user.
        /// It is limited to 100 characters.
        /// </summary>
        [Required(ErrorMessage = "Title is required")]
        [Display(Name = "Article Title")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; }

        /// <summary>
        /// The full content of the article. 
        /// </summary>
        [Required(ErrorMessage = "Content is required")]
        [Display(Name = "Article Content")]
        [MinLength(10, ErrorMessage = "Content must be at least 10 characters long")]
        public string Content { get; set; }

        /// <summary>
        /// Represents the moment the article was published
        /// </summary>
        [Display(Name = "Published Date")]
        public DateTimeOffset PublishedDate { get; set; }
    }
}
