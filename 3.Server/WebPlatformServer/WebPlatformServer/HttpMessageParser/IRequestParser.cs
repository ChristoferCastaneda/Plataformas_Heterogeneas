using HttpMessageParser.Models;

namespace HttpMessageParser
{
    /// <summary>
    /// Defines a method for parsing raw HTTP request text into an <see cref="HttpRequest"/> object.
    /// </summary>
    public interface IRequestParser
    {
        /// <summary>
        /// Parses the specified HTTP request text and returns an <see cref="HttpRequest"/> object representing its
        /// contents.
        /// </summary>
        /// <param name="requestText">The raw HTTP request as a string. Must not be <see langword="null"/> or empty.</param>
        /// <returns>An <see cref="HttpRequest"/> object containing the parsed data from the request text.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="requestText"/> is <see langword="null"/>, empty or malformed.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="requestText"/> is <see langword="null"/>.</exception>
        public HttpRequest ParseRequest(string requestText);
    }
}
