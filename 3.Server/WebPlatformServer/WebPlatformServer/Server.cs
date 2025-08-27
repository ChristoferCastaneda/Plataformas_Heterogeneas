using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using HttpMessageParser;
using HttpMessageParser.Models;

namespace WebPlatformServer
{
    public class Server
    {
        // Propiedades para configurar el servidor
        public int Port { get; set; } = 8080;
        public string StaticDirectory { get; set; } = "./static";
        public Dictionary<string, string> Routes { get; set; } = new Dictionary<string, string>();

        private TcpListener? _listener; // Hacer nullable para evitar warnings
        private bool _isRunning;
        private readonly HttpRequestParser _requestParser;
        private readonly HttpResponseWriter _responseWriter;

        // Constructor sin parámetros
        public Server()
        {
            _requestParser = new HttpRequestParser();
            _responseWriter = new HttpResponseWriter();

            // Rutas por defecto
            Routes = new Dictionary<string, string>
            {
                { "/", "/index.html" }
            };
        }

        // Constructor con puerto y directorio
        public Server(int port = 8080, string staticDirectory = "./static")
        {
            Port = port;
            StaticDirectory = staticDirectory;
            _requestParser = new HttpRequestParser();
            _responseWriter = new HttpResponseWriter();

            // Rutas por defecto
            Routes = new Dictionary<string, string>
            {
                { "/", "/index.html" }
            };
        }

        // Constructor con puerto, directorio y rutas (NUEVO)
        public Server(int port = 8080, string staticDirectory = "./static", Dictionary<string, string>? routes = null)
        {
            Port = port;
            StaticDirectory = staticDirectory;
            _requestParser = new HttpRequestParser();
            _responseWriter = new HttpResponseWriter();

            // Usar las rutas proporcionadas o las por defecto
            Routes = routes ?? new Dictionary<string, string>
            {
                { "/", "/index.html" }
            };
        }

        public void Start()
        {
            try
            {
                // Crear el listener en el puerto especificado
                _listener = new TcpListener(IPAddress.Any, Port);
                _listener.Start();
                _isRunning = true;

                Console.WriteLine($"Servidor iniciado en el puerto {Port}");
                Console.WriteLine($"Directorio estático: {Path.GetFullPath(StaticDirectory)}");
                Console.WriteLine("Rutas configuradas:");
                foreach (var route in Routes)
                {
                    Console.WriteLine($"  {route.Key} -> {route.Value}");
                }
                Console.WriteLine("Presiona Ctrl+C para detener el servidor\n");

                // Manejar múltiples conexiones
                while (_isRunning)
                {
                    try
                    {
                        TcpClient client = _listener.AcceptTcpClient();
                        Task.Run(() => HandleClient(client));
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al iniciar el servidor: {ex.Message}");
            }
            finally
            {
                _listener?.Stop();
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            NetworkStream? stream = null;

            try
            {
                stream = client.GetStream();

                var requestBuilder = new StringBuilder();
                byte[] buffer = new byte[1024];
                int bytesRead;

                do
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        requestBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                    }
                } while (stream.DataAvailable && bytesRead > 0);

                if (requestBuilder.Length > 0)
                {
                    string requestText = requestBuilder.ToString();
                    Console.WriteLine($"Petición recibida:\n{requestText}");

                    string response = ProcessRequest(requestText);

                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    await stream.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando cliente: {ex.Message}");

                try
                {
                    if (stream != null && stream.CanWrite)
                    {
                        string errorResponse = CreateErrorResponse(500, "Internal Server Error");
                        byte[] errorBytes = Encoding.UTF8.GetBytes(errorResponse);
                        await stream.WriteAsync(errorBytes, 0, errorBytes.Length);
                        await stream.FlushAsync();
                    }
                }
                catch { }
            }
            finally
            {
                try
                {
                    stream?.Close();
                    client?.Close();
                }
                catch { }
            }
        }

        private string ProcessRequest(string requestText)
        {
            try
            {
                HttpRequest request = _requestParser.ParseRequest(requestText);
                if (request.Method.ToUpper() != "GET")
                {
                    return CreateErrorResponse(405, "Method Not Allowed");
                }

                string requestPath = request.RequestTarget;

                int queryIndex = requestPath.IndexOf('?');
                if (queryIndex >= 0)
                {
                    requestPath = requestPath.Substring(0, queryIndex);
                }

                // Aplicar rutas configuradas
                if (Routes.ContainsKey(requestPath))
                {
                    requestPath = Routes[requestPath];
                }
                else if (requestPath == "/")
                {
                    requestPath = "/index.html";
                }

                string filePath = Path.Combine(StaticDirectory, requestPath.TrimStart('/'));

                try
                {
                    filePath = Path.GetFullPath(filePath);
                    string staticFullPath = Path.GetFullPath(StaticDirectory);

                    if (!filePath.StartsWith(staticFullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        return CreateErrorResponse(403, "Forbidden");
                    }

                    if (!File.Exists(filePath))
                    {
                        return CreateErrorResponse(404, "Not Found");
                    }

                    byte[] fileContent = File.ReadAllBytes(filePath);
                    string contentType = GetContentType(filePath);

                    return CreateSuccessResponse(fileContent, contentType);
                }
                catch (ArgumentException)
                {
                    return CreateErrorResponse(404, "Not Found");
                }
                catch (DirectoryNotFoundException)
                {
                    return CreateErrorResponse(404, "Not Found");
                }
                catch (FileNotFoundException)
                {
                    return CreateErrorResponse(404, "Not Found");
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error de formato HTTP: {ex.Message}");
                return CreateErrorResponse(400, "Bad Request");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando petición: {ex.Message}");
                return CreateErrorResponse(500, "Internal Server Error");
            }
        }

        private string CreateSuccessResponse(byte[] content, string contentType)
        {
            string? bodyContent = null;
            if (content.Length > 0)
            {
                if (IsTextContentType(contentType))
                {
                    bodyContent = Encoding.UTF8.GetString(content);
                }
                else
                {
                    bodyContent = Encoding.UTF8.GetString(content);
                }
            }

            var response = new HttpResponse
            {
                Protocol = "HTTP/1.1",
                StatusCode = 200,
                StatusText = "OK",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", contentType },
                    { "Content-Length", content.Length.ToString() },
                    { "Connection", "close" }
                },
                Body = bodyContent
            };

            return _responseWriter.WriteResponse(response);
        }

        private bool IsTextContentType(string contentType)
        {
            return contentType.StartsWith("text/") ||
                   contentType.Contains("javascript") ||
                   contentType.Contains("json") ||
                   contentType.Contains("xml");
        }

        private string CreateErrorResponse(int statusCode, string statusText)
        {
            string errorBody = $@"<!DOCTYPE html>
<html>
<head>
    <title>Error {statusCode}</title>
    <style>
        body {{ font-family: Arial, sans-serif; text-align: center; padding: 50px; }}
        h1 {{ color: #e74c3c; }}
        p {{ color: #7f8c8d; }}
    </style>
</head>
<body>
    <h1>Error {statusCode} - {statusText}</h1>
    <p>El servidor no pudo procesar su petición.</p>
</body>
</html>";

            var response = new HttpResponse
            {
                Protocol = "HTTP/1.1",
                StatusCode = statusCode,
                StatusText = statusText,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "text/html" },
                    { "Content-Length", Encoding.UTF8.GetBytes(errorBody).Length.ToString() },
                    { "Connection", "close" }
                },
                Body = errorBody
            };

            return _responseWriter.WriteResponse(response);
        }

        private string GetContentType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();

            return extension switch
            {
                ".html" => "text/html; charset=utf-8",
                ".css" => "text/css; charset=utf-8",
                ".js" => "application/javascript; charset=utf-8",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".ico" => "image/x-icon",
                ".txt" => "text/plain; charset=utf-8",
                ".json" => "application/json; charset=utf-8",
                ".xml" => "application/xml; charset=utf-8",
                _ => "application/octet-stream"
            };
        }

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
            Console.WriteLine("Servidor detenido.");
        }
    }
}