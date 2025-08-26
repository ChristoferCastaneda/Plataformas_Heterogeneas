using HttpMessageParser.Models;

namespace HttpMessageParser
{
    /// <summary>
    /// Defines a method for serializing an <see cref="HttpResponse"/> to a string representation.
    /// </summary>
    public interface IResponseWriter
    {
        /// <summary>
        /// Writes the specified <see cref="HttpResponse"/> to a string representation.
        /// </summary>
        /// <param name="response">A populated <c>HttpResponse</c> instance ready to be serialized</param>
        /// <returns>
        /// A string representation of the <paramref name="response"/> instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="response"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when any of the required properties in the <paramref name="response"/> object are null or empty.</exception>
        public string WriteResponse(HttpResponse response);
    }
}
