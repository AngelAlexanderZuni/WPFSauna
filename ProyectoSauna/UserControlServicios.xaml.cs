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
        private Servicio? _servicioEnEdicion = null;

        public UserControlServicios()
        {
            InitializeComponent();

            _servicioRepo = App.AppHost!.Services.GetRequiredService<IServicioRepository>();
            _categoriaRepo = App.AppHost!.Services.GetRequiredService<CategoriaServicioRepository>();

            Loaded += async (_, __) => await InicializarAsync();
        }

        private async Task InicializarAsync()
        {
            await CargarCategoriasAsync();
            await CargarServiciosAsync();
        }

        private async Task CargarCategoriasAsync()
        {
            try
            {
                var categorias = await _categoriaRepo.GetAllAsync();
                _categorias = categorias.ToList();
                
                // Cargar categorías en filtro
                cbFiltroCategoria.Items.Clear();
                cbFiltroCategoria.Items.Add(new { idCategoriaServicio = (int?)null, nombre = "Todas las categorías" });
                foreach (var cat in _categorias.Where(c => c.activo))
                {
                    cbFiltroCategoria.Items.Add(cat);
                }
                cbFiltroCategoria.SelectedIndex = 0;

                // Cargar categorías en formulario
                cbCategoria.ItemsSource = _categorias.Where(c => c.activo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar categorías: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CargarServiciosAsync()
        {
            try
            {
                var servicios = await _servicioRepo.GetAllAsync();
                _servicios = servicios.ToList();
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar servicios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarFiltros()
        {
            var serviciosFiltrados = _servicios.AsEnumerable();

            // Filtro por estado activo/inactivo (excluyente)
            if (chkMostrarInactivos.IsChecked == true)
            {
                // Mostrar solo inactivos
                serviciosFiltrados = serviciosFiltrados.Where(s => !s.activo);
            }
            else
            {
                // Mostrar solo activos
                serviciosFiltrados = serviciosFiltrados.Where(s => s.activo);
            }

            // Filtro por texto
            if (!string.IsNullOrWhiteSpace(txtBuscar.Text))
            {
                var busqueda = txtBuscar.Text.ToLower();
                serviciosFiltrados = serviciosFiltrados.Where(s => 
                    s.nombre.ToLower().Contains(busqueda));
            }

            // Filtro por categoría
            if (cbFiltroCategoria.SelectedValue is int categoriaId)
            {
                serviciosFiltrados = serviciosFiltrados.Where(s => s.idCategoriaServicio == categoriaId);
            }

            dataGridServicios.ItemsSource = serviciosFiltrados.ToList();
        }

        // Eventos de filtros
        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e) => AplicarFiltros();
        private void cbFiltroCategoria_SelectionChanged(object sender, SelectionChangedEventArgs e) => AplicarFiltros();
        private void chkMostrarInactivos_Checked(object sender, RoutedEventArgs e) => AplicarFiltros();
        private void chkMostrarInactivos_Unchecked(object sender, RoutedEventArgs e) => AplicarFiltros();

        private void dataGridServicios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridServicios.SelectedItem is Servicio servicio)
            {
                CargarServicioEnFormulario(servicio);
                _servicioEnEdicion = servicio;
            }
            else
            {
                LimpiarFormulario();
                _servicioEnEdicion = null;
            }
        }

        private void CargarServicioEnFormulario(Servicio servicio)
        {
            txtNombre.Text = servicio.nombre;
            txtPrecio.Text = servicio.precio.ToString("0.00", CultureInfo.InvariantCulture);
            txtDuracion.Text = servicio.duracionEstimada?.ToString() ?? string.Empty;
            cbCategoria.SelectedValue = servicio.idCategoriaServicio;
            chkActivo.IsChecked = servicio.activo;

            btnGuardarActualizar.Content = "🔄 Actualizar";
        }

        private void LimpiarFormulario()
        {
            txtNombre.Clear();
            txtPrecio.Clear();
            txtDuracion.Clear();
            cbCategoria.SelectedIndex = -1;
            chkActivo.IsChecked = true;

            btnGuardarActualizar.Content = "💾 Guardar";
            _servicioEnEdicion = null;

            ValidarFormulario();
        }

        private async void btnGuardarActualizar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarDatos())
                return;

            try
            {
                var nombre = SanitizeName(txtNombre.Text.Trim());
                var precio = decimal.Parse(txtPrecio.Text, CultureInfo.InvariantCulture);
                var duracion = string.IsNullOrWhiteSpace(txtDuracion.Text) ? (int?)null : int.Parse(txtDuracion.Text);
                var idCategoria = (int?)cbCategoria.SelectedValue;
                var activo = chkActivo.IsChecked == true;

                if (_servicioEnEdicion == null)
                {
                    // Nuevo servicio
                    if (await ExisteNombreAsync(nombre))
                    {
                        MessageBox.Show("Ya existe un servicio con ese nombre.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var nuevoServicio = new Servicio
                    {
                        nombre = nombre,
                        precio = precio,
                        duracionEstimada = duracion,
                        idCategoriaServicio = idCategoria,
                        activo = activo
                    };

                    await _servicioRepo.AddAsync(nuevoServicio);
                    MessageBox.Show("Servicio creado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Actualizar servicio existente
                    if (await ExisteNombreAsync(nombre, _servicioEnEdicion.idServicio))
                    {
                        MessageBox.Show("Ya existe otro servicio con ese nombre.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _servicioEnEdicion.nombre = nombre;
                    _servicioEnEdicion.precio = precio;
                    _servicioEnEdicion.duracionEstimada = duracion;
                    _servicioEnEdicion.idCategoriaServicio = idCategoria;
                    _servicioEnEdicion.activo = activo;

                    await _servicioRepo.UpdateAsync(_servicioEnEdicion);
                    MessageBox.Show("Servicio actualizado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                await CargarServiciosAsync();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el servicio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            dataGridServicios.SelectedItem = null;
        }

        private void FormularioServicio_Changed(object sender, RoutedEventArgs e)
        {
            ValidarFormulario();
        }

        private void ValidarFormulario()
        {
            if (btnGuardarActualizar == null) return;

            var nombreValido = !string.IsNullOrWhiteSpace(txtNombre?.Text);
            var precioValido = decimal.TryParse(txtPrecio?.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var precio) && precio > 0;
            var duracionValida = string.IsNullOrWhiteSpace(txtDuracion?.Text) || (int.TryParse(txtDuracion.Text, out var dur) && dur > 0);

            btnGuardarActualizar.IsEnabled = nombreValido && precioValido && duracionValida;
        }

        private bool ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombre.Focus();
                return false;
            }

            if (!decimal.TryParse(txtPrecio.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var precio) || precio <= 0)
            {
                MessageBox.Show("El precio debe ser un valor positivo.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrecio.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtDuracion.Text) && (!int.TryParse(txtDuracion.Text, out var duracion) || duracion <= 0))
            {
                MessageBox.Show("La duración debe ser un número entero positivo.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDuracion.Focus();
                return false;
            }

            return true;
        }

        private async Task<bool> ExisteNombreAsync(string nombre, int? idExcluir = null)
        {
            var servicios = await _servicioRepo.GetAllAsync();
            return servicios.Any(s => s.nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase) && 
                                     s.idServicio != (idExcluir ?? 0));
        }

        private static string SanitizeName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower().Trim());
        }
    }
}