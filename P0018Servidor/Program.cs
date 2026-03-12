using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;

class Servidor
{
    static string connectionString =
        "Server=Pcerda;Database=SistemasAvanzadosServidorCliente;Trusted_Connection=True; TrustServerCertificate=True";

    static void Main()
    {
        TcpListener servidor = new TcpListener(IPAddress.Any, 5005);
        servidor.Start();

        Console.WriteLine("Servidor iniciado en puerto 5005...");
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

                Estudiante? estudiante = JsonSerializer.Deserialize<Estudiante>(jsonRecibido);

                string respuesta;

                if (estudiante != null && Validar(estudiante))
                {
                    GuardarEnBaseDatos(estudiante);
                    respuesta = "Datos guardados en base de datos";
                }
                else
                {
                    respuesta = "Datos inválidos";
                }

                byte[] respuestaBytes = Encoding.UTF8.GetBytes(respuesta);
                stream.Write(respuestaBytes, 0, respuestaBytes.Length);

                MostrarRegistros();

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

    static bool Validar(Estudiante e)
    {
        return !string.IsNullOrWhiteSpace(e.Nombre)
               && e.Edad > 0
               && !string.IsNullOrWhiteSpace(e.Carrera);
    }

    static void GuardarEnBaseDatos(Estudiante estudiante)
    {
        using (SqlConnection conexion = new SqlConnection(connectionString))
        {
            conexion.Open();

            string query = "INSERT INTO Estudiantes (Nombre, Edad, Carrera) VALUES (@Nombre,@Edad,@Carrera)";

            SqlCommand comando = new SqlCommand(query, conexion);
            comando.Parameters.AddWithValue("@Nombre", estudiante.Nombre);
            comando.Parameters.AddWithValue("@Edad", estudiante.Edad);
            comando.Parameters.AddWithValue("@Carrera", estudiante.Carrera);

            comando.ExecuteNonQuery();
        }
    }

    static void MostrarRegistros()
    {
        using (SqlConnection conexion = new SqlConnection(connectionString))
        {
            conexion.Open();

            string query = "SELECT Nombre, Edad, Carrera FROM Estudiantes";

            SqlCommand comando = new SqlCommand(query, conexion);
            SqlDataReader reader = comando.ExecuteReader();

            Console.WriteLine("\nEstudiantes guardados en la base de datos:\n");

            while (reader.Read())
            {
                Console.WriteLine(
                    reader["Nombre"] + " | " +
                    reader["Edad"] + " | " +
                    reader["Carrera"]);
            }
        }
    }
}

class Estudiante
{
    public string Nombre { get; set; } = "";
    public int Edad { get; set; }
    public string Carrera { get; set; } = "";
}