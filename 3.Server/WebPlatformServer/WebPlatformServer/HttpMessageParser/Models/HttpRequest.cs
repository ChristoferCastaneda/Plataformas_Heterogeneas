namespace HttpMessageParser.Models
{
    /// <summary>
    /// A model to represent an HTTP request.
    /// </summary>
    public partial class HttpRequest
    {
        /// <summary>
        /// The HTTP method (e.g., GET, POST, PUT, DELETE).
        /// </summary>
        public required string Method { get; set; }

        /// <summary>
        /// The request target, which is typically the path and query string.
        /// </summary>
        public required string RequestTarget { get; set; }

        /// <summary>
        /// The HTTP version (e.g., HTTP/1.1, HTTP/2).
        /// </summary>
        public required string Protocol { get; set; }

        /// <summary>
        /// A collection of headers associated with the request.
        /// </summary>
        /// <remarks>
        /// If the request does not contain any headers, this property should be an empty dictionary.
        /// </remarks>
        public required Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// The raw contents of the request body, if any.
        /// </summary>
        /// <remarks>
        /// This will be null if the request does not have a body.
        /// </remarks>
        public string? Body { get; set; }
    }
}
