using System;
using System.Linq;

namespace HttpMessageParser.Models
{
    public partial class HttpResponse
    {
        public string GetHeaderValue(string headerName)
        {
            if (string.IsNullOrEmpty(headerName))
            {
                throw new ArgumentException("Header name cannot be null or empty", nameof(headerName));
            }

            if (Headers == null || Headers.Count == 0)
            {
                return null;
            }

            var matchingHeader = Headers.FirstOrDefault(h =>
                string.Equals(h.Key, headerName, StringComparison.OrdinalIgnoreCase));

            return matchingHeader.Value;
        }

        public bool IsSuccess()
        {
            return StatusCode >= 200 && StatusCode < 300;
        }

        public bool IsClientError()
        {
            return StatusCode >= 400 && StatusCode < 500;
        }

        public bool IsServerError()
        {
            return StatusCode >= 500 && StatusCode < 600;
        }
    }
}