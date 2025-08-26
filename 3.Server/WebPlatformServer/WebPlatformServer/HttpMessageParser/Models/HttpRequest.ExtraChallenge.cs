using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace HttpMessageParser.Models
{
    public partial class HttpRequest
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

        public IDictionary<string, string> GetQueryParameters()
        {
            var queryParams = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(RequestTarget))
            {
                return queryParams;
            }

            int questionMarkIndex = RequestTarget.IndexOf('?');
            if (questionMarkIndex == -1 || questionMarkIndex == RequestTarget.Length - 1)
            {
                return queryParams;
            }

            string queryString = RequestTarget.Substring(questionMarkIndex + 1);

            if (string.IsNullOrWhiteSpace(queryString))
            {
                return queryParams;
            }

            string[] pairs = queryString.Split('&');
            foreach (string pair in pairs)
            {
                if (string.IsNullOrWhiteSpace(pair))
                {
                    continue;
                }

                int equalIndex = pair.IndexOf('=');
                if (equalIndex == -1)
                {
                    throw new FormatException($"Malformed query string: parameter '{pair}' is missing a value");
                }

                string key = pair.Substring(0, equalIndex);
                string value = pair.Substring(equalIndex + 1);

                if (string.IsNullOrEmpty(key))
                {
                    throw new FormatException($"Malformed query string: empty parameter name in '{pair}'");
                }

                key = HttpUtility.UrlDecode(key);
                value = HttpUtility.UrlDecode(value);

                queryParams[key] = value;
            }

            return queryParams;
        }

        public IDictionary<string, string> GetFormData()
        {
            var formData = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(Body))
            {
                return formData;
            }

            string contentType = GetHeaderValue("Content-Type");
            if (string.IsNullOrEmpty(contentType))
            {
                return formData;
            }

            if (contentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
            {
                return ParseUrlEncodedFormData(Body);
            }
            else if (contentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
            {
                return ParseMultipartFormData(Body, contentType);
            }

            return formData;
        }

        private IDictionary<string, string> ParseUrlEncodedFormData(string body)
        {
            var formData = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(body))
            {
                return formData;
            }

            string[] pairs = body.Split('&');
            foreach (string pair in pairs)
            {
                if (string.IsNullOrWhiteSpace(pair))
                {
                    continue;
                }

                int equalIndex = pair.IndexOf('=');
                if (equalIndex == -1)
                {
                    throw new FormatException($"Malformed form data: field '{pair}' is missing a value");
                }

                string key = pair.Substring(0, equalIndex);
                string value = pair.Substring(equalIndex + 1);

                if (string.IsNullOrEmpty(key))
                {
                    throw new FormatException($"Malformed form data: empty field name in '{pair}'");
                }

                key = HttpUtility.UrlDecode(key);
                value = HttpUtility.UrlDecode(value);

                formData[key] = value;
            }

            return formData;
        }

        private IDictionary<string, string> ParseMultipartFormData(string body, string contentType)
        {
            var formData = new Dictionary<string, string>();

            string boundary = ExtractBoundary(contentType);
            if (string.IsNullOrEmpty(boundary))
            {
                throw new FormatException("Malformed multipart/form-data: boundary not found in Content-Type");
            }

            string[] parts = body.Split(new[] { "--" + boundary }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                if (part.Trim() == "--" || string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }

                ParseMultipartPart(part.Trim(), formData);
            }

            return formData;
        }

        private string ExtractBoundary(string contentType)
        {
            string[] parts = contentType.Split(';');
            foreach (string part in parts)
            {
                string trimmed = part.Trim();
                if (trimmed.StartsWith("boundary=", StringComparison.OrdinalIgnoreCase))
                {
                    string boundary = trimmed.Substring(9).Trim();
                    if (boundary.StartsWith("\"") && boundary.EndsWith("\""))
                    {
                        boundary = boundary.Substring(1, boundary.Length - 2);
                    }
                    return boundary;
                }
            }
            return null;
        }

        private void ParseMultipartPart(string part, Dictionary<string, string> formData)
        {
            string[] lines = part.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            string fieldName = null;
            int contentStartIndex = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (string.IsNullOrEmpty(line))
                {
                    contentStartIndex = i + 1;
                    break;
                }

                if (line.StartsWith("Content-Disposition:", StringComparison.OrdinalIgnoreCase))
                {
                    fieldName = ExtractFieldName(line);
                }
            }

            if (string.IsNullOrEmpty(fieldName))
            {
                throw new FormatException("Malformed multipart/form-data: missing field name in Content-Disposition");
            }

            if (contentStartIndex >= 0 && contentStartIndex < lines.Length)
            {
                var contentLines = new List<string>();
                for (int i = contentStartIndex; i < lines.Length; i++)
                {
                    contentLines.Add(lines[i]);
                }

                string content = string.Join("\n", contentLines).Trim();
                formData[fieldName] = content;
            }
            else
            {
                formData[fieldName] = string.Empty;
            }
        }

        private string ExtractFieldName(string contentDisposition)
        {
            string[] parts = contentDisposition.Split(';');
            foreach (string part in parts)
            {
                string trimmed = part.Trim();
                if (trimmed.StartsWith("name=", StringComparison.OrdinalIgnoreCase))
                {
                    string name = trimmed.Substring(5).Trim();
                    if (name.StartsWith("\"") && name.EndsWith("\""))
                    {
                        name = name.Substring(1, name.Length - 2);
                    }
                    return name;
                }
            }
            return null;
        }
    }
}