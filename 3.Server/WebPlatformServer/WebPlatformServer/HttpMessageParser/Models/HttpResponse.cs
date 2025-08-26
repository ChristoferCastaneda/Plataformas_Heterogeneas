namespace HttpMessageParser.Models
{
    /// <summary>
    /// A model to represent an HTTP response.
    /// </summary>
    public partial class HttpResponse
    {
        /// <summary>
        /// The HTTP version (e.g., HTTP/1.1, HTTP/2).
        /// </summary>
        public required string Protocol { get; set; }

        /// <summary>
        /// The status code of the response (e.g., 200, 404, 500).
        /// </summary>
        public required int StatusCode { get; set; }

        /// <summary>
        /// The status text associated with the status code (e.g., "OK", "Not Found", "Internal Server Error").
        /// </summary>
        public required string StatusText { get; set; }

        /// <summary>
        /// A collection of headers associated with the response.
        /// </summary>
        /// <remarks>
        /// If the response does not contain any headers, this property should be an empty dictionary.
        /// </remarks>
        public required Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// The raw contents of the response body, if any.
        /// </summary>
        /// <remarks>
        /// This will be null if the response does not have a body.
        /// </remarks>
        public string? Body { get; set; }
    }
}
