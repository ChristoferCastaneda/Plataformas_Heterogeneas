using System;
using System.Collections.Generic;
using System.Linq;
using HttpMessageParser.Models;

namespace HttpMessageParser { 
    public class HttpRequestParser : IRequestParser {
        
        public HttpRequest ParseRequest(string requestText){
            // Valida si es nulo si lo es regresa la excepcion
            if (requestText == null){
                throw new ArgumentNullException(nameof(requestText), "Request text cannot be null.");
            }

            // Valida si es vacio si lo es regresa la excepcion de manera general
            if (string.IsNullOrWhiteSpace(requestText)){
                throw new ArgumentException("Request text cannot be empty.", nameof(requestText));
            }

            // Divide el texto de la solicitud en l�neas
            string[] lines = requestText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            // Valida que la primera l�nea no est� vac�a (y que exista una)
            if (lines.Length == 0 || string.IsNullOrWhiteSpace(lines[0])){
                throw new ArgumentException("Invalid HTTP request format: Missing request line.", nameof(requestText));
            }

            // Analiza la primera l�nea de la solicitud
            string requestLine = lines[0].Trim();
            string[] requestParts = requestLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Valida que la solicitud tiene exactamente 3 partes: m�todo, objetivo y protocolo
            if (requestParts.Length != 3){
                throw new ArgumentException("Invalid HTTP request format: Request line must contain method, target, and protocol.", nameof(requestText));
            }

            string method = requestParts[0];
            string requestTarget = requestParts[1];
            string protocol = requestParts[2];

            //Aqui no se porque pero si no agregaba una validacion del nulo o vacio a veces si pasaba y fallaba 1 o mas pruebas asi que mas vale que sobre a que no que falte

            // Valida que el m�todo no est� vac�o
            if (string.IsNullOrWhiteSpace(method)){
                throw new ArgumentException("Invalid HTTP request format: Method is missing.", nameof(requestText));
            }

            // Valida que el objetivo de la solicitud no est� vac�o y contenga al menos un '/'
            if (string.IsNullOrWhiteSpace(requestTarget) || !requestTarget.Contains("/")){
                throw new ArgumentException("Invalid HTTP request format: Request target must contain at least one '/' character.", nameof(requestText));
            }

            // Valida que el protocolo no est� vac�o y comience con "HTTP"
            if (string.IsNullOrWhiteSpace(protocol) || !protocol.StartsWith("HTTP", StringComparison.OrdinalIgnoreCase)){
                throw new ArgumentException("Invalid HTTP request format: Protocol must start with 'HTTP'.", nameof(requestText));
            }

            // Crea un diccionario para almacenar los encabezados
            var headers = new Dictionary<string, string>();
            int currentLineIndex = 1;
            bool bodyStarted = false;
            int bodyStartIndex = -1;

            // Ciclo para analizar las l�neas de encabezado
            for (int i = 1; i < lines.Length; i++){
                string line = lines[i];

                // Revisa si la l�nea est� vac�a para determinar el inicio del cuerpo
                if (string.IsNullOrWhiteSpace(line)){
                    bodyStarted = true;
                    bodyStartIndex = i + 1;
                    break;
                }

                // Analiza la l�nea del encabezado
                int colonIndex = line.IndexOf(':');

                // Valida que la l�nea del encabezado contenga un ':'
                if (colonIndex <= 0){
                    throw new ArgumentException($"Invalid HTTP request format: Header line '{line}' must contain a ':' character with text before it.", nameof(requestText));
                }

                string headerName = line.Substring(0, colonIndex).Trim();
                string headerValue = colonIndex < line.Length - 1
                    ? line.Substring(colonIndex + 1).Trim()
                    : string.Empty;

                // Valida que el nombre del encabezado no est� vac�o
                if (string.IsNullOrWhiteSpace(headerName)){
                    throw new ArgumentException($"Invalid HTTP request format: Header name cannot be empty in line '{line}'.", nameof(requestText));
                }

                // Valida que el valor del encabezado no est� vac�o
                if (headerValue.Length == 0){
                    throw new ArgumentException($"Invalid HTTP request format: Header line '{line}' must have text after the ':' character.", nameof(requestText));
                }

                // Agrega el encabezado al diccionario
                headers[headerName] = headerValue;
            }

            // Extrae el cuerpo de la solicitud (Si existe)
            string body = null;
            if (bodyStarted && bodyStartIndex < lines.Length && bodyStartIndex >= 0){

                // Une las l�neas del cuerpo a partir del �ndice de inicio del cuerpo
                var bodyLines = lines.Skip(bodyStartIndex).ToArray();
                if (bodyLines.Length > 0){
                    body = string.Join("\n", bodyLines);

                    // Elimina los caracteres de nueva l�nea al final del cuerpo
                    if (!string.IsNullOrWhiteSpace(body)){
                        body = body.TrimEnd('\n', '\r');

                        // Si el cuerpo est� vac�o despu�s de eliminar los caracteres de nueva l�nea, lo establece como null
                        if (string.IsNullOrWhiteSpace(body)){
                            body = null;
                        }
                    }
                    else{
                        //Hace lo mismo pero si el cuerpo es completamente vac�o
                        body = null;
                    }
                }
            }

            // Crea y devuelve el objeto HttpRequest
            return new HttpRequest{
                Method = method,
                RequestTarget = requestTarget,
                Protocol = protocol,
                Headers = headers,
                Body = body
            };
        }
    }
}

/*
Y este es el final del c�digo. 
P.D.
Es hermoso documentar el codigo no se si es por c# pero entiende lo que esta pasando y lo documenta de una manera muy buena y de manera automatica.
 */