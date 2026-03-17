using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;

class Servidor
{
    static string connectionString =
        "Server=Pcerda;Database=SocketsBD;Trusted_Connection=True; TrustServerCertificate=True";

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

                Usuario? usuario = JsonSerializer.Deserialize<Usuario>(jsonRecibido);

                string respuesta;

                if (usuario != null && Validar(usuario))
                {
                    GuardarEnBaseDatos(usuario);
                    respuesta = "Usuario guardado en base de datos";
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

    static bool Validar(Usuario u)
    {
        return !string.IsNullOrWhiteSpace(u.Nombre)
               && u.Edad > 0
               && !string.IsNullOrWhiteSpace(u.Correo)
               && !string.IsNullOrWhiteSpace(u.Ciudad)
               && !string.IsNullOrWhiteSpace(u.Telefono);
    }

    static void GuardarEnBaseDatos(Usuario usuario)
    {
        using (SqlConnection conexion = new SqlConnection(connectionString))
        {
            conexion.Open();

            string query = @"INSERT INTO Usuarios 
                            (Nombre, Edad, Correo, Ciudad, Telefono) 
                            VALUES (@Nombre,@Edad,@Correo,@Ciudad,@Telefono)";

            SqlCommand comando = new SqlCommand(query, conexion);
            comando.Parameters.AddWithValue("@Nombre", usuario.Nombre);
            comando.Parameters.AddWithValue("@Edad", usuario.Edad);
            comando.Parameters.AddWithValue("@Correo", usuario.Correo);
            comando.Parameters.AddWithValue("@Ciudad", usuario.Ciudad);
            comando.Parameters.AddWithValue("@Telefono", usuario.Telefono);

            comando.ExecuteNonQuery();
        }
    }

    static void MostrarRegistros()
    {
        using (SqlConnection conexion = new SqlConnection(connectionString))
        {
            conexion.Open();

            string query = "SELECT Nombre, Edad, Correo, Ciudad, Telefono FROM Usuarios";

            SqlCommand comando = new SqlCommand(query, conexion);
            SqlDataReader reader = comando.ExecuteReader();

            Console.WriteLine("\nUsuarios guardados en la base de datos:\n");

            while (reader.Read())
            {
                Console.WriteLine(
                    reader["Nombre"] + " | " +
                    reader["Edad"] + " | " +
                    reader["Correo"] + " | " +
                    reader["Ciudad"] + " | " +
                    reader["Telefono"]);
            }
        }
    }
}

class Usuario
{
    public string Nombre { get; set; } = string.Empty;
    public int Edad { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}