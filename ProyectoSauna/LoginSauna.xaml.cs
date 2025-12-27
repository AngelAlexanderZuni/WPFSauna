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
using ProyectoSauna.Helpers;
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

                    // 🔍 Logging para debug
                    System.Diagnostics.Debug.WriteLine($"🔐 Intentando conectar con usuario: {usuario}");
                    System.Diagnostics.Debug.WriteLine($"🔗 Connection String: {DatabaseConfig.GetConnectionString()}");
                    
                    conn.Open();
                    System.Diagnostics.Debug.WriteLine("✅ Conexión exitosa a la base de datos");
                    
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
            catch (SqlException sqlEx)
            {
                // 🚨 ERRORES ESPECÍFICOS DE SQL SERVER
                string errorDetallado = sqlEx.Number switch
                {
                    -1 => "❌ Error de conexión:\n\nNo se puede conectar al servidor SQL Server.\n\n" +
                          "Verifica:\n" +
                          "1. El nombre del servidor es correcto: DESKTOP-HG4U4IK\\Luis\n" +
                          "2. SQL Server está ejecutándose\n" +
                          "3. Windows Authentication está habilitada\n" +
                          "4. Tu usuario de Windows tiene permisos en la BD\n\n" +
                          $"Error técnico: {sqlEx.Message}",
                    
                    18456 => "❌ Error de autenticación:\n\n" +
                             "Tu usuario de Windows no tiene permisos para acceder a la base de datos.\n\n" +
                             "Solución:\n" +
                             "1. Abre SQL Server Management Studio\n" +
                             "2. Conéctate como administrador\n" +
                             "3. Ve a Security > Logins\n" +
                             "4. Agrega tu usuario de Windows\n" +
                             "5. Dale permisos en la base de datos ProyectoSauna\n\n" +
                             $"Error técnico: {sqlEx.Message}",
                    
                    4060 => "❌ Base de datos no encontrada:\n\n" +
                            "La base de datos 'ProyectoSauna' no existe en el servidor.\n\n" +
                            "Verifica:\n" +
                            "1. El nombre de la base de datos es correcto\n" +
                            "2. La base de datos existe en tu servidor\n" +
                            "3. Tienes permisos para acceder a ella\n\n" +
                            $"Error técnico: {sqlEx.Message}",
                    
                    2812 => "❌ Stored Procedure no encontrado:\n\n" +
                            "El procedimiento almacenado 'sp_ValidarLogin' no existe.\n\n" +
                            "Solución:\n" +
                            "1. Ejecuta el script de creación de la base de datos\n" +
                            "2. Verifica que todos los procedimientos almacenados estén creados\n\n" +
                            $"Error técnico: {sqlEx.Message}",
                    
                    _ => $"❌ Error de SQL Server:\n\n{sqlEx.Message}\n\n" +
                         $"Código de error: {sqlEx.Number}\n" +
                         $"Servidor: {sqlEx.Server}\n" +
                         $"Procedimiento: {sqlEx.Procedure}"
                };
                
                MessageBox.Show(errorDetallado, "Error de Base de Datos",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                
                System.Diagnostics.Debug.WriteLine($"❌ SQL Error {sqlEx.Number}: {sqlEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {sqlEx.StackTrace}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado en el login:\n\n{ex.Message}\n\n" +
                                $"Tipo: {ex.GetType().Name}\n\n" +
                                "Contacta al administrador del sistema.", 
                                "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                
                System.Diagnostics.Debug.WriteLine($"❌ Error General: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
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
