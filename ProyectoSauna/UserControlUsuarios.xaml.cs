using Microsoft.Extensions.DependencyInjection;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ProyectoSauna
{
    /// <summary>
    /// Lógica de interacción para UserControlUsuarios.xaml
    /// </summary>
    public partial class UserControlUsuarios : UserControl
    {
        private List<Usuario> _usuarios;
        private List<Rol> _roles;
        private Usuario? _usuarioSeleccionado;

        public UserControlUsuarios()
        {
            InitializeComponent();
            
            _usuarios = new List<Usuario>();
            _roles = new List<Rol>();
            _ = InicializarAsync();
        }

        private async Task InicializarAsync()
        {
            try
            {
                await CargarRolesAsync();
                await CargarUsuariosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CargarRolesAsync()
        {
            try
            {
                // Crear scope específico
                using var scope = App.AppHost!.Services.CreateScope();
                var rolRepository = scope.ServiceProvider.GetRequiredService<IRolRepository>();
                
                _roles = (await rolRepository.GetAllAsync()).ToList();
                cmbRol.ItemsSource = _roles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar roles: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CargarUsuariosAsync()
        {
            // Usar el filtro seleccionado actual
            if (cmbFiltroEstado?.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                await CargarUsuariosPorFiltroAsync(item.Tag?.ToString() ?? "activos");
            }
            else
            {
                // Por defecto, cargar activos
                await CargarUsuariosPorFiltroAsync("activos");
            }
        }

        private async Task CargarUsuariosPorFiltroAsync(string filtro)
        {
            try
            {
                using var scope = App.AppHost!.Services.CreateScope();
                var usuarioRepository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();

                switch (filtro)
                {
                    case "activos":
                        _usuarios = await usuarioRepository.ObtenerActivosAsync();
                        break;
                    case "inactivos":
                        _usuarios = await usuarioRepository.ObtenerInactivosAsync();
                        break;
                    case "todos":
                        _usuarios = await usuarioRepository.ObtenerTodosConRelacionesAsync();
                        break;
                }

                dgUsuarios.ItemsSource = _usuarios;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar usuarios: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            var esValido = await ValidarFormulario();
            if (!esValido) return;

            try
            {
                // Crear scope específico
                using var scope = App.AppHost!.Services.CreateScope();
                var usuarioRepository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();

                if (_usuarioSeleccionado == null)
                {
                    var nuevoUsuario = new Usuario
                    {
                        nombreUsuario = txtNombreUsuario.Text.Trim(),
                        contraseniaHash = HashPassword(txtPassword.Password),
                        correo = string.IsNullOrWhiteSpace(txtCorreo.Text) ? null : txtCorreo.Text.Trim(),
                        fechaCreacion = DateTime.Now,
                        activo = chkActivo.IsChecked ?? true,
                        idRol = (int)cmbRol.SelectedValue
                    };

                    await usuarioRepository.AddAsync(nuevoUsuario);
                    MessageBox.Show("Usuario creado exitosamente", "Éxito", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _usuarioSeleccionado.nombreUsuario = txtNombreUsuario.Text.Trim();
                    _usuarioSeleccionado.correo = string.IsNullOrWhiteSpace(txtCorreo.Text) ? null : txtCorreo.Text.Trim();
                    _usuarioSeleccionado.activo = chkActivo.IsChecked ?? true;
                    _usuarioSeleccionado.idRol = (int)cmbRol.SelectedValue;

                    if (!string.IsNullOrEmpty(txtPassword.Password))
                    {
                        _usuarioSeleccionado.contraseniaHash = HashPassword(txtPassword.Password);
                    }

                    await usuarioRepository.UpdateAsync(_usuarioSeleccionado);
                    MessageBox.Show("Usuario actualizado exitosamente", "Éxito", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LimpiarFormulario();
                await CargarUsuariosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar usuario: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void DgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Método vacío 
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Usuario usuario)
            {
                _usuarioSeleccionado = usuario;
                CargarDatosEnFormulario(usuario);

                btnGuardar.Content = "ACTUALIZAR";
                txtNombreUsuario.Focus(); 
            }
        }

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Usuario usuario)
            {
                var result = MessageBox.Show(
                    $"¿Está seguro de desactivar el usuario '{usuario.nombreUsuario}'?\n\nPodrá reactivarlo más tarde desde el filtro 'Inactivos'.",
                    "Confirmar desactivación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using var scope = App.AppHost!.Services.CreateScope();
                        var usuarioRepository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
                        
                        await usuarioRepository.DesactivarAsync(usuario.idUsuario);
                        
                        MessageBox.Show("Usuario desactivado exitosamente", "Éxito", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        LimpiarFormulario();
                        await CargarUsuariosAsync(); 
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar usuario: {ex.Message}", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async Task<bool> ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtNombreUsuario.Text))
            {
                MessageBox.Show("El nombre de usuario es obligatorio", "Error de validación", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombreUsuario.Focus();
                return false;
            }

            if (_usuarioSeleccionado == null && string.IsNullOrEmpty(txtPassword.Password))
            {
                MessageBox.Show("La contraseña es obligatoria para usuarios nuevos", "Error de validación", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPassword.Focus();
                return false;
            }

            if (cmbRol.SelectedValue == null)
            {
                MessageBox.Show("Debe seleccionar un rol", "Error de validación", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbRol.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtCorreo.Text) && !IsValidEmail(txtCorreo.Text))
            {
                MessageBox.Show("El formato del correo electrónico no es válido", "Error de validación", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCorreo.Focus();
                return false;
            }

            try
            {
                using var scope = App.AppHost!.Services.CreateScope();
                var usuarioRepository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
                
                var existeUsuario = await usuarioRepository.ExisteNombreUsuarioAsync(
                    txtNombreUsuario.Text.Trim(), 
                    _usuarioSeleccionado?.idUsuario);

                if (existeUsuario)
                {
                    MessageBox.Show("Ya existe un usuario con ese nombre", "Error de validación", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNombreUsuario.Focus();
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al validar usuario: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void LimpiarFormulario()
        {
            txtNombreUsuario.Clear();
            txtPassword.Clear();
            txtCorreo.Clear();
            cmbRol.SelectedIndex = -1;
            chkActivo.IsChecked = true;
            _usuarioSeleccionado = null;
            dgUsuarios.SelectedItem = null;

            btnGuardar.Content = "GUARDAR";
        }

        private void CargarDatosEnFormulario(Usuario usuario)
        {
            txtNombreUsuario.Text = usuario.nombreUsuario;
            txtPassword.Password = ""; // Por seguridad, no mostrar la contraseña
            txtCorreo.Text = usuario.correo ?? "";
            cmbRol.SelectedValue = usuario.idRol;
            chkActivo.IsChecked = usuario.activo;
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var saltedPassword = password + "SaunaSalt2024";
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // NUEVO: Método para manejar el cambio de filtro
        private async void CmbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFiltroEstado.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                await CargarUsuariosPorFiltroAsync(item.Tag.ToString());
            }
        }
    }
}
