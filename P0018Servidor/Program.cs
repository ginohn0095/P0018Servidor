using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Servidor
{
    static string archivo = "estudiantes.txt";

    static void Main()
    {
        TcpListener servidor = new TcpListener(IPAddress.Any, 5005);
        servidor.Start();

        Console.WriteLine("Servidor iniciado en puerto 5005...\n");

        // Mostrar datos guardados cuando inicia el servidor
        if (File.Exists(archivo))
        {
            Console.WriteLine("Datos almacenados previamente:\n");
            string contenido = File.ReadAllText(archivo);
            Console.WriteLine(contenido);
            Console.WriteLine("---------------------------------\n");
        }

        Console.WriteLine("Esperando clientes...\n");

        while (true)
        {
            try
            {
                TcpClient cliente = servidor.AcceptTcpClient();
                Console.WriteLine("Cliente conectado.");

                NetworkStream stream = cliente.GetStream();

                byte[] buffer = new byte[1024];
                int bytesLeidos = stream.Read(buffer, 0, buffer.Length);

                string jsonRecibido = Encoding.UTF8.GetString(buffer, 0, bytesLeidos);

                Console.WriteLine("JSON recibido:");
                Console.WriteLine(jsonRecibido);

                // Guardar en archivo TXT
                Guardar(jsonRecibido);

                // Mostrar todo el historial guardado
                Console.WriteLine("\nHistorial de estudiantes:");
                Console.WriteLine(File.ReadAllText(archivo));

                string respuesta = "Datos recibidos y guardados";

                byte[] respuestaBytes = Encoding.UTF8.GetBytes(respuesta);
                stream.Write(respuestaBytes, 0, respuestaBytes.Length);

                stream.Close();
                cliente.Close();

                Console.WriteLine("\nCliente desconectado.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }

    static void Guardar(string json)
    {
        File.AppendAllText(archivo, json + Environment.NewLine);
    }
}