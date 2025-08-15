using HttpMessageParser.Models;

namespace HttpMessageParser.Tests
{
    public class ResponseWriterTests
    {
        private IResponseWriter responseWriter;

        [SetUp]
        public void Setup()
        {
            // Replace the following with an assigment of your actual implementation of IResponseWriter
            responseWriter = new HttpResponseWriter();
        }

        [Test]
        public void WriteResponse_NullResponseThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => responseWriter.WriteResponse(null));
        }

        [TestCase(null, 200, "OK")]
        [TestCase("HTTP/1.1", null, "OK")]
        [TestCase("HTTP/1.1", 200, null)]
        public void WriteResponse_ResponseMissingRequiredValuesThrowsException(string? protocol, int? statusCode, string? statusText)
        {
            HttpResponse response = new HttpResponse
            {
                Protocol = null,
                StatusCode = 200,
                StatusText = "OK",
                Headers = new Dictionary<string, string>(),
                Body = null
            };
            Assert.Throws<ArgumentException>(() => responseWriter.WriteResponse(response));
        }

        [Test]
        public void WriteResponse_CorrectlyWritesMinimalResponse()
        {
            HttpResponse response = new HttpResponse
            {
                Protocol = "HTTP/1.1",
                StatusCode = 200,
                StatusText = "OK",
                Headers = new Dictionary<string, string>(),
                Body = null
            };
            string expected = "HTTP/1.1 200 OK";
            string result = responseWriter.WriteResponse(response);
            Assert.That(result.Trim(), Does.StartWith(expected));
        }

        [Test]
        public void WriteResponse_CorrectlyWritesResponseWithHeaders()
        {
            HttpResponse response = new HttpResponse
            {
                Protocol = "HTTP/1.1",
                StatusCode = 404,
                StatusText = "Not Found",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "text/html" },
                    { "Server", "TestServer" }
                },
                Body = null
            };
            string expectedFirstLine = "HTTP/1.1 404 Not Found\n";
            string[] expectedHeadersStrings = new string[]
            {
                "\nContent-Type: text/html",
                "\nServer: TestServer"
            };

            string result = responseWriter.WriteResponse(response);

            Assert.That(result.Trim(), Does.StartWith(expectedFirstLine));
            foreach(string expectedHeader in expectedHeadersStrings)
            {
                Assert.That(result.Trim(), Does.Contain(expectedHeader));
            }
        }

        [Test]
        public void WriteResponse_CorrectlyWritesResponseWithBody()
        {
            HttpResponse response = new HttpResponse
            {
                Protocol = "HTTP/1.1",
                StatusCode = 201,
                StatusText = "Created",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Content-Length", "18" }
                },
                Body = "{\"id\":1,\"ok\":true}"
            };

            string expectedFirstLine = "HTTP/1.1 201 Created\n";
            string[] expectedHeadersStrings = new string[]
            {
                "\nContent-Type: application/json",
                "\nContent-Length: 18"
            };
            string expectedBody = "\n{\"id\":1,\"ok\":true}";

            string result = responseWriter.WriteResponse(response);
            Assert.That(result.Trim(), Does.StartWith(expectedFirstLine));
            foreach(string expectedHeader in expectedHeadersStrings)
            {
                Assert.That(result.Trim(), Does.Contain(expectedHeader));
            }
            Assert.That(result.Trim(), Does.EndWith(expectedBody));
        }
    }
}
