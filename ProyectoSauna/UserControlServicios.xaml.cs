using Microsoft.Extensions.DependencyInjection;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Base;
using ProyectoSauna.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ProyectoSauna
{
    public partial class UserControlServicios : UserControl
    {
        private readonly IServicioRepository _servicioRepo;
        private readonly CategoriaServicioRepository _categoriaRepo;

        private List<Servicio> _servicios = new();
        private List<CategoriaServicio> _categorias = new();

        public UserControlServicios()
        {
            InitializeComponent();

            _servicioRepo = App.AppHost!.Services.GetRequiredService<IServicioRepository>();
            _categoriaRepo = App.AppHost!.Services.GetRequiredService<CategoriaServicioRepository>();

            Loaded += async (_, __) => await InicializarAsync();
        }

        private async Task InicializarAsync()
        {
            _categorias = (await _categoriaRepo.GetAllAsync()).ToList();
            cbCategoria.ItemsSource = _categorias;
            
            var filtroCategorias = new List<CategoriaServicio> { new CategoriaServicio { idCategoriaServicio = 0, nombre = "Todas" } };
            filtroCategorias.AddRange(_categorias);
            cbFiltroCategoria.ItemsSource = filtroCategorias;
            cbFiltroCategoria.SelectedIndex = 0;

            await CargarServiciosAsync();
            chkActivo.IsChecked = true;
            UpdateGuardarActualizarEnabled();
        }

        private async Task CargarServiciosAsync()
        {
            _servicios = (await _servicioRepo.GetAllAsync()).ToList();
            dataGridServicios.ItemsSource = _servicios;
        }



        private void AplicarFiltrosServicios()
        {
            var texto = (txtBuscar.Text ?? string.Empty).Trim().ToLowerInvariant();
            IEnumerable<Servicio> datos = _servicios;
            if (!string.IsNullOrWhiteSpace(texto))
                datos = datos.Where(s => (s.nombre ?? string.Empty).ToLower().Contains(texto));
            var catId = cbFiltroCategoria.SelectedValue as int?;
            if (catId.HasValue && catId.Value != 0)
                datos = datos.Where(s => s.idCategoriaServicio == catId.Value);
            var mostrarInactivos = chkMostrarInactivos.IsChecked == true;
            datos = datos.Where(s => mostrarInactivos ? !s.activo : s.activo);
            dataGridServicios.ItemsSource = datos.ToList();
        }

        private async void btnGuardarActualizar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var nombre = SanitizeName(txtNombre.Text);
                if (!ValidateNombre(nombre)) return;
                if (!ValidatePrecio(txtPrecio.Text, out var precio)) return;
                if (!ValidateDuracion(txtDuracion.Text, out var duracion)) return;

                if (dataGridServicios.SelectedItem is Servicio s)
                {
                    s.nombre = nombre;
                    s.precio = precio;
                    s.duracionEstimada = duracion;
                    s.activo = chkActivo.IsChecked == true;
                    s.idCategoriaServicio = cbCategoria.SelectedValue as int?;
                    await _servicioRepo.UpdateAsync(s);
                    AplicarFiltrosServicios();
                    ActualizarBotonServicio();
                    LimpiarFormulario();
                    MessageBox.Show("Servicio actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Validar duplicados de forma más estricta
                    var servicioExistente = _servicios.FirstOrDefault(x => 
                        !string.IsNullOrWhiteSpace(x.nombre) && 
                        string.Equals(x.nombre.Trim(), nombre.Trim(), StringComparison.OrdinalIgnoreCase));
                        
                    if (servicioExistente != null)
                    {
                        MessageBox.Show($"Ya existe un servicio con el nombre '{servicioExistente.nombre}'.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    if (cbCategoria.SelectedValue == null)
                    {
                        MessageBox.Show("Seleccione una categoría.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var nuevo = new Servicio
                    {
                        nombre = nombre,
                        precio = precio,
                        duracionEstimada = duracion,
                        activo = chkActivo.IsChecked == true,
                        idCategoriaServicio = cbCategoria.SelectedValue as int?
                    };
                    await _servicioRepo.AddAsync(nuevo);
                    _servicios.Add(nuevo);
                    AplicarFiltrosServicios();
                    LimpiarFormulario();
                    MessageBox.Show("Servicio guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string SanitizeName(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            
            var s = input.Trim();
            
            // Eliminar espacios múltiples
            while (s.Contains("  ")) s = s.Replace("  ", " ");
            
            // Eliminar espacios al inicio y final de cada línea
            s = string.Join(" ", s.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            
            // Convertir a formato título (primera letra de cada palabra en mayúscula)
            var words = s.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpperInvariant(words[i][0]) + 
                              (words[i].Length > 1 ? words[i][1..].ToLowerInvariant() : "");
                }
            }
            
            return string.Join(" ", words);
        }

        private bool ValidateNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            if (nombre.Length < 3 || nombre.Length > 100)
            {
                MessageBox.Show("El nombre debe tener entre 3 y 100 caracteres.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            if (!nombre.Any(char.IsLetter))
            {
                MessageBox.Show("El nombre debe contener al menos una letra.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            // Validar que no tenga solo espacios o caracteres especiales
            if (nombre.All(c => char.IsWhiteSpace(c) || char.IsPunctuation(c) || char.IsSymbol(c)))
            {
                MessageBox.Show("El nombre debe contener texto válido, no solo espacios o símbolos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            // Validar que no contenga caracteres especiales peligrosos
            var caracteresProhibidos = new[] { '<', '>', '"', '\'', '&', '%', '#', '@', '!', '$', '^', '*', '(', ')', '[', ']', '{', '}', '|', '\\', '/', '?' };
            if (nombre.Any(c => caracteresProhibidos.Contains(c)))
            {
                MessageBox.Show("El nombre contiene caracteres no permitidos. Use solo letras, números, espacios, puntos, comas y guiones.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            return true;
        }

        private bool ValidatePrecio(string? texto, out decimal precio)
        {
            precio = 0m;
            
            if (string.IsNullOrWhiteSpace(texto))
            {
                MessageBox.Show("El precio es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            // Limpiar el texto de caracteres no numéricos excepto punto y coma
            var textoLimpio = new string(texto.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());
            
            if (!decimal.TryParse(textoLimpio, NumberStyles.Any, CultureInfo.InvariantCulture, out precio))
            {
                MessageBox.Show("El formato del precio no es válido. Use números decimales (ej: 50.00).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            if (precio <= 0)
            {
                MessageBox.Show("El precio debe ser mayor a 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            if (precio > 10000)
            {
                MessageBox.Show("El precio no puede ser mayor a S/ 10,000.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            // Limitar a 2 decimales
            precio = Math.Round(precio, 2);
            
            return true;
        }

        private bool ValidateDuracion(string? texto, out int? duracion)
        {
            duracion = null;
            
            // La duración es opcional
            if (string.IsNullOrWhiteSpace(texto)) return true;
            
            // Limpiar texto de caracteres no numéricos
            var textoLimpio = new string(texto.Where(char.IsDigit).ToArray());
            
            if (string.IsNullOrWhiteSpace(textoLimpio))
            {
                MessageBox.Show("Si especifica duración, debe ser un número válido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            if (!int.TryParse(textoLimpio, out var d))
            {
                MessageBox.Show("La duración debe ser un número entero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            if (d < 1 || d > 600)
            {
                MessageBox.Show("La duración debe estar entre 1 y 600 minutos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            
            duracion = d;
            return true;
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e) => AplicarFiltrosServicios();
        private void cbFiltroCategoria_SelectionChanged(object sender, SelectionChangedEventArgs e) => AplicarFiltrosServicios();
        private void chkMostrarInactivos_Checked(object sender, RoutedEventArgs e) => AplicarFiltrosServicios();
        private void chkMostrarInactivos_Unchecked(object sender, RoutedEventArgs e) => AplicarFiltrosServicios();

        private void FormularioServicio_Changed(object sender, RoutedEventArgs e)
        {
            UpdateGuardarActualizarEnabled();
        }

        private void UpdateGuardarActualizarEnabled()
        {
            var nombre = SanitizeName(txtNombre.Text);
            var nombreOk = !string.IsNullOrWhiteSpace(nombre) && nombre.Length >= 3 && nombre.Length <= 100 && nombre.Any(char.IsLetter);
            var precioOk = decimal.TryParse(txtPrecio.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var precio) && precio > 0 && precio <= 10000;
            var durOk = string.IsNullOrWhiteSpace(txtDuracion.Text) || (int.TryParse(txtDuracion.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) && d >= 0 && d <= 600);
            var creando = dataGridServicios.SelectedItem is not Servicio;
            var categoriaOk = cbCategoria.SelectedValue != null;
            btnGuardarActualizar.IsEnabled = nombreOk && precioOk && durOk && (!creando || categoriaOk);
        }

        private async void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridServicios.SelectedItem is not Servicio s)
            {
                MessageBox.Show("Seleccione un servicio para eliminar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (MessageBox.Show($"¿Está seguro de eliminar el servicio '{s.nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;
            
            try
            {
                await _servicioRepo.DeleteAsync(s.idServicio);
                _servicios.Remove(s);
                AplicarFiltrosServicios();
                LimpiarFormulario();
                MessageBox.Show("Servicio eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dataGridServicios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridServicios.SelectedItem is Servicio s)
            {
                txtNombre.Text = s.nombre;
                txtPrecio.Text = s.precio.ToString(CultureInfo.InvariantCulture);
                txtDuracion.Text = s.duracionEstimada?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
                chkActivo.IsChecked = s.activo;
                cbCategoria.SelectedValue = s.idCategoriaServicio;
                
                ActualizarBotonServicio();
                UpdateGuardarActualizarEnabled();
            }
            else
            {
                ActualizarBotonServicio();
                UpdateGuardarActualizarEnabled();
            }
        }



        private async void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            var texto = (txtBuscar.Text ?? string.Empty).Trim().ToLowerInvariant();
            IEnumerable<Servicio> datos = _servicios;
            
            // Filtro por nombre
            if (!string.IsNullOrWhiteSpace(texto))
                datos = datos.Where(s => s.nombre.ToLower().Contains(texto));

            // Filtro por categoría
            var catId = cbFiltroCategoria.SelectedValue as int?;
            if (catId.HasValue && catId.Value != 0)
                datos = datos.Where(s => s.idCategoriaServicio == catId.Value);

            // Filtro por estado activo/inactivo
            var mostrarInactivos = chkMostrarInactivos.IsChecked == true;
            if (!mostrarInactivos)
                datos = datos.Where(s => s.activo);

            dataGridServicios.ItemsSource = datos.ToList();
        }



        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            txtNombre.Clear();
            txtPrecio.Clear();
            txtDuracion.Clear();
            cbCategoria.SelectedIndex = -1;
            chkActivo.IsChecked = true;
            dataGridServicios.UnselectAll();
            ActualizarBotonServicio();
            UpdateGuardarActualizarEnabled();
        }

        private void ActualizarBotonServicio()
        {
            var editando = dataGridServicios.SelectedItem is Servicio;
            btnGuardarActualizar.Content = editando ? "✏️ Actualizar" : "💾 Guardar";
            btnGuardarActualizar.Style = (Style)FindResource(editando ? "PrimaryButton" : "SuccessButton");
        }


    }
}