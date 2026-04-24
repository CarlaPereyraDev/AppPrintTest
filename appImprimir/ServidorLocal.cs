using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace appImprimir
{
    public class ServidorLocal
    {
        public async Task ServerLocal()
        {
            HttpListener server = new HttpListener();
            var _port = 8900;

            var _client = $"http://localhost:{_port}/";

            server.Prefixes.Add(_client);

            try
            {
                // Iniciar el servidor
                server.Start();
                Console.WriteLine($"Servidor HTTP escuchando en {_client}");

                while (true)
                {
                    try
                    {
                        // Esperar por una solicitud entrante
                        HttpListenerContext _context = await server.GetContextAsync();
                        HttpListenerRequest _request = _context.Request;

                        // Manejar solicitud OPTIONS
                        if (_request.HttpMethod == "OPTIONS")
                        {
                        //    _context.Response.StatusCode = (int)HttpStatusCode.OK;
                            _context.Response.Headers.Add("Access-Control-Allow-Origin", "*"); // Permitir todos los orígenes
                            //_context.Response.Headers.Add("Access-Control-Allow-Origin", _request.UrlReferrer.AbsoluteUri);
                            _context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS"); // Métodos permitidos
                            _context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type"); // Cabeceras permitidas
                            _context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");                        
                            await _context.Response.OutputStream.WriteAsync(new byte[0], 0, 0); // Respuesta vacía
                            _context.Response.OutputStream.Close();
                        }
                        // Manejar solicitud GET en /imprimir
                        else 
                        if (_request.HttpMethod == "GET" && _request.Url.AbsolutePath == "/imprimir")
                        {
                            await HandleGetAsync(_context);
                        }
                        // Manejar solicitud POST en /imprimir
                        else if (_request.HttpMethod == "POST" && _request.Url.AbsolutePath == "/print")
                        {
                            await HandlePostAsync(_context);
                        }
                        else
                        {
                            // Si no coincide con las rutas esperadas, responder con 404
                            _context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            byte[] _errorResponse = Encoding.UTF8.GetBytes("Ruta no encontrada");
                            await _context.Response.OutputStream.WriteAsync(_errorResponse, 0, _errorResponse.Length);
                            _context.Response.OutputStream.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Capturar cualquier excepción durante la gestión de una solicitud
                        Console.WriteLine($"Error al procesar la solicitud: {ex.Message}");
                    }
                }
            }
            catch (HttpListenerException ex)
            {
                // Capturar cualquier excepción relacionada con HttpListener
                Console.WriteLine($"Error iniciando el servidor: {ex.Message}");
            }
            finally
            {
                // Detener el servidor si se ha detenido el bucle principal
                server.Stop();
                Console.WriteLine("Servidor detenido.");
            }
        }

        private static async Task HandleGetAsync(HttpListenerContext context)
        {
            Console.WriteLine("Solicitud GET en /... recibida");

            var _response = context.Response;
            string _responseString = "{\"status\": \"OK\", \"message\": \"Hola desde c#\"}";
            byte[] _buffer = Encoding.UTF8.GetBytes(_responseString);

            _response.ContentLength64 = _buffer.Length;
            _response.ContentType = "application/json";
            _response.StatusCode = (int)HttpStatusCode.OK;

            await _response.OutputStream.WriteAsync(_buffer, 0, _buffer.Length);
            _response.OutputStream.Close();
        }

        private static async Task HandlePostAsync(HttpListenerContext context)
        {
            Console.WriteLine("\nSolicitud POST en /... recibida");

            string _requestBody;

            using (var _reader = new System.IO.StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                _requestBody = await _reader.ReadToEndAsync();
            }

            //Console.WriteLine($"\nCuerpo de la solicitud recibida : \n{_requestBody}");

            List<MDatos> ticketLista;

            try
            {
                ticketLista = JsonConvert.DeserializeObject<List<MDatos>>(_requestBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializando el JSON: {ex.Message}");
                var errorResponse = context.Response;
                string errorResponseString = "{\"status\":\"error\",\"message\":\"Error en el formato de los datos enviados\"}";
                byte[] errorBuffer = Encoding.UTF8.GetBytes(errorResponseString);

                errorResponse.ContentLength64 = errorBuffer.Length;
                errorResponse.ContentType = "application/json";
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;

                await errorResponse.OutputStream.WriteAsync(errorBuffer, 0, errorBuffer.Length);
                errorResponse.OutputStream.Close();
                return;
                throw;
            }

            var _ticket = new imprTerm();
            //var _sin = _requestBody.Replace("\"","");
            _ticket.mains(_requestBody, ticketLista);

            var _response = context.Response;
            string _responseString = "{\"status\": \"OK\", \"message\": \"Se imprimió correctamente\"}";
            byte[] _buffer = Encoding.UTF8.GetBytes(_responseString);

            _response.ContentLength64 = _buffer.Length;
            _response.ContentType = "application/json";
            _response.StatusCode = (int)HttpStatusCode.OK;

            //enviar el encabezado.
            _response.Headers.Add("Access-Control-Allow-Origin", "*"); // Permitir todos los orígenes
            _response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS"); // Métodos permitidos
            _response.Headers.Add("Access-Control-Allow-Headers", "Content-Type"); // Cabeceras permitidas
            _response.Headers.Add("Access-Control-Allow-Credentials", "true");

            await _response.OutputStream.WriteAsync(_buffer, 0, _buffer.Length);
            _response.OutputStream.Close();
        }
    }
}
