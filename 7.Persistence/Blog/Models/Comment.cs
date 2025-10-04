using Blog.Data;
using System.ComponentModel.DataAnnotations;

namespace Blog.Models
{
    public class Comment
    {
        /// <summary>
        /// The identifier of the article this comment belongs to.
        /// </summary>
        public int Id { get; set; }

        [Required]
        public int ArticleId { get; set; }

        /// <summary>
        /// The content of the comment.
        /// </summary>
        [Required]
        public string? Content { get; set; }

        /// <summary>
        /// Represents the moment the comment was posted.
        /// </summary>
        public DateTimeOffset PublishedDate { get; set; }

        public Article? Article { get; set; }
    }
}
