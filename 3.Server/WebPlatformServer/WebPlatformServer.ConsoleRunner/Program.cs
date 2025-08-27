using WebPlatformServer;

namespace WebPlatformServer.ConsoleRunner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Actividad 3 (Quiero llorar");
            Console.WriteLine();

            // Permitir al usuario configurar el puerto
            Console.Write("Eliga un puerto (default 8080): ");
            string? portInput = Console.ReadLine();
            int port = 8080;

            // Use TryParse porque ReadLine devuelve un string y necesito un int
            if (!string.IsNullOrWhiteSpace(portInput) && int.TryParse(portInput, out int parsedPort))
            {
                port = parsedPort;
            }

            // Permitir al usuario configurar el directorio estático
            Console.Write("Introduzca una direccion (default './static'): ");
            string? staticDir = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(staticDir))
            {
                staticDir = "./static";
            }

            Console.WriteLine();
            Console.WriteLine($"Iniciando... {port}");
            Console.WriteLine($"Directorio Estatico: {staticDir}");
            Console.WriteLine();

            // Crear las rutas por defecto
            var routes = new Dictionary<string, string>
            {
                { "/", "/index.html" },
                { "/home", "/main.html" },
                { "/about", "/about.html" }
            };

            // Crear y iniciar el servidor con los parámetros configurados por el usuario
            var server = new Server(port, staticDir, routes);

            try
            {
                server.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting server: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}