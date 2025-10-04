//No voy a mentir esto lo hice porque chat me dijo que si lo hacia en el ooriginal muy probablemente iba a romper todo jajajaja (quiero llorar)
using Blog.Models;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data
{
    public class EFArticleRepository :IArticleRepository
    {
        private readonly BlogArtDbContext _context;

        public EFArticleRepository(BlogArtDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Article> GetAll()
        {
            return _context.Articles
                .OrderByDescending(a => a.PublishedDate)
                .ToList();
        }

        public IEnumerable<Article> GetByDateRange(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            return _context.Articles
                .Where(a => a.PublishedDate >= startDate && a.PublishedDate <= endDate)
                .OrderByDescending(a => a.PublishedDate)
                .ToList();
        }

        public Article? GetById(int id)
        {
            return _context.Articles.Find(id);
        }

        public Article Create(Article article)
        {
            _context.Articles.Add(article);
            _context.SaveChanges();
            return article;
        }

        public IEnumerable<Comment> GetCommentsByArticleId(int articleId)
        {
            return _context.Comments
                .Where(c => c.ArticleId == articleId)
                .OrderByDescending(c => c.PublishedDate)
                .ToList();
        }

        public void AddComment(Comment comment)
        {
            _context.Comments.Add(comment);
            _context.SaveChanges();
        }
    }
}

