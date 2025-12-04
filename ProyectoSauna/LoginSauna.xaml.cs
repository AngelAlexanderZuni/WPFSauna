using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ProyectoSauna.Data;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using ProyectoSauna.Repositories.Interfaces;

namespace ProyectoSauna
{
    public partial class LoginSauna : Window
    {
        public LoginSauna()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string clave = txtPassword.Password.Trim();

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(clave))
            {
                MessageBox.Show("Debe ingresar usuario y contraseña.", "Advertencia",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Hash de la contraseña ingresada
                string claveHasheada = HashPassword(clave);

                using (SqlConnection conn = new SqlConnection(DatabaseConfig.GetConnectionString()))
                using (SqlCommand cmd = new SqlCommand("sp_ValidarLogin", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@identificador", usuario);
                    cmd.Parameters.AddWithValue("@contraseniaHash", claveHasheada); // Ahora usa hash

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string rol = reader["Rol"].ToString();
                        string nombreUsuario = reader["nombreUsuario"].ToString();
                        int idUsuario = Convert.ToInt32(reader["idUsuario"]);

                        ProyectoSauna.Models.SesionActual.IdUsuario = idUsuario;
                        ProyectoSauna.Models.SesionActual.NombreCompleto = nombreUsuario;
                        ProyectoSauna.Models.SesionActual.Rol = rol;

                        MessageBox.Show($"Bienvenido {nombreUsuario} ({rol})", "Acceso correcto",
                                        MessageBoxButton.OK, MessageBoxImage.Information);

                        MainWindow main = new MainWindow(rol, nombreUsuario);
                        main.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Usuario o contraseña incorrectos.", "Error",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en el login: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //  Método de hash igual al del UserControl (consistencia)
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var saltedPassword = password + "SaunaSalt2024";
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashedBytes);
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnCerrarLogin_Click(object sender, RoutedEventArgs e)
        {
            // Cerrar la aplicación completamente
            Application.Current.Shutdown();
        }
    }
}
