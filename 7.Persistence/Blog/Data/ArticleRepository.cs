using Blog.Models;
using Microsoft.Data.Sqlite;

namespace Blog.Data
{
    /// <summary>
    /// Implementation of <see cref="IArticleRepository"/> using SQLite as a persistence solution.
    /// </summary>
    public class ArticleRepository : IArticleRepository
    {
        private readonly string _connectionString;

        public ArticleRepository(DatabaseConfig _config)
        {
            _connectionString = _config.DefaultConnectionString ?? throw new ArgumentNullException("Connection string not found");
        }

        /// <summary>
        /// Creates the necessary tables for this application if they don't exist already.
        /// Should be called once when starting the service.
        /// </summary>
        public void EnsureCreated()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Crea tabla de los articulos
            var createArticlesTable = @"
                CREATE TABLE IF NOT EXISTS Articles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    AuthorName TEXT NOT NULL,
                    AuthorEmail TEXT NOT NULL,
                    Title TEXT NOT NULL,
                    Content TEXT NOT NULL,
                    PublishedDate TEXT NOT NULL
                )";

            using (var command = new SqliteCommand(createArticlesTable, connection))
            {
                command.ExecuteNonQuery();
            }

            // Crea la tabla de los comentarios
            var createCommentsTable = @"
                CREATE TABLE IF NOT EXISTS Comments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ArticleId INTEGER NOT NULL,
                    Content TEXT NOT NULL,
                    PublishedDate TEXT NOT NULL,
                    FOREIGN KEY (ArticleId) REFERENCES Articles(Id)
                )";

            using (var command = new SqliteCommand(createCommentsTable, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public IEnumerable<Article> GetAll()
        {
            var articles = new List<Article>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = "SELECT Id, AuthorName, AuthorEmail, Title, Content, PublishedDate FROM Articles ORDER BY PublishedDate DESC";

            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                articles.Add(MapArticleFromReader(reader));
            }

            return articles;
        }

        public IEnumerable<Article> GetByDateRange(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var articles = new List<Article>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = @"
                SELECT Id, AuthorName, AuthorEmail, Title, Content, PublishedDate 
                FROM Articles 
                WHERE PublishedDate >= @startDate AND PublishedDate <= @endDate
                ORDER BY PublishedDate DESC";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@startDate", startDate.ToString("O"));
            command.Parameters.AddWithValue("@endDate", endDate.ToString("O"));

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                articles.Add(MapArticleFromReader(reader));
            }

            return articles;
        }

        public Article? GetById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = "SELECT Id, AuthorName, AuthorEmail, Title, Content, PublishedDate FROM Articles WHERE Id = @id";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return MapArticleFromReader(reader);
            }

            return null;
        }

        public Article Create(Article article)
        {
            if (article == null)
                throw new ArgumentNullException(nameof(article));

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = @"
                INSERT INTO Articles (AuthorName, AuthorEmail, Title, Content, PublishedDate) 
                VALUES (@authorName, @authorEmail, @title, @content, @publishedDate);
                SELECT last_insert_rowid();";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@authorName", article.AuthorName);
            command.Parameters.AddWithValue("@authorEmail", article.AuthorEmail);
            command.Parameters.AddWithValue("@title", article.Title);
            command.Parameters.AddWithValue("@content", article.Content);
            command.Parameters.AddWithValue("@publishedDate", article.PublishedDate.ToString("O"));

            var id = Convert.ToInt32(command.ExecuteScalar());
            article.Id = id;

            return article;
        }

        public void AddComment(Comment comment)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));

            // Verify that the article exists
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var checkArticleQuery = "SELECT COUNT(*) FROM Articles WHERE Id = @articleId";
            using (var checkCommand = new SqliteCommand(checkArticleQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@articleId", comment.ArticleId);
                var exists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;

                if (!exists)
                    throw new ArgumentException("No article exists with the specified ID.", nameof(comment));
            }

            var insertQuery = @"
                INSERT INTO Comments (ArticleId, Content, PublishedDate) 
                VALUES (@articleId, @content, @publishedDate)";

            using var insertCommand = new SqliteCommand(insertQuery, connection);
            insertCommand.Parameters.AddWithValue("@articleId", comment.ArticleId);
            insertCommand.Parameters.AddWithValue("@content", comment.Content);
            insertCommand.Parameters.AddWithValue("@publishedDate", comment.PublishedDate.ToString("O"));

            insertCommand.ExecuteNonQuery();
        }

        public IEnumerable<Comment> GetCommentsByArticleId(int articleId)
        {
            var comments = new List<Comment>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var query = @"
                SELECT ArticleId, Content, PublishedDate 
                FROM Comments 
                WHERE ArticleId = @articleId 
                ORDER BY PublishedDate DESC";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@articleId", articleId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                comments.Add(new Comment
                {
                    ArticleId = reader.GetInt32(0),
                    Content = reader.GetString(1),
                    PublishedDate = DateTimeOffset.Parse(reader.GetString(2))
                });
            }

            return comments;
        }

        private Article MapArticleFromReader(SqliteDataReader reader)
        {
            return new Article
            {
                Id = reader.GetInt32(0),
                AuthorName = reader.GetString(1),
                AuthorEmail = reader.GetString(2),
                Title = reader.GetString(3),
                Content = reader.GetString(4),
                PublishedDate = DateTimeOffset.Parse(reader.GetString(5))
            };
        }
    }
}
