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
        private readonly IDetalleServicioRepository _detalleRepo;
        private readonly ICuentaRepository _cuentaRepo;

        private List<Servicio> _servicios = new();
        private List<CategoriaServicio> _categorias = new();
        private List<Cuenta> _cuentas = new();
        private DetalleServicio? _detalleEnEdicion = null;
        private List<DetalleServicio> _detallesAll = new();
        private List<DetalleServicio> _detallesFiltered = new();

        public UserControlServicios()
        {
            InitializeComponent();

            _servicioRepo = App.AppHost!.Services.GetRequiredService<IServicioRepository>();
            _categoriaRepo = App.AppHost!.Services.GetRequiredService<CategoriaServicioRepository>();
            _detalleRepo = App.AppHost!.Services.GetRequiredService<IDetalleServicioRepository>();
            _cuentaRepo = App.AppHost!.Services.GetRequiredService<ICuentaRepository>();

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
            await CargarCuentasAsync();
            await CargarHistorialGeneralAsync();
            chkActivo.IsChecked = true;
            UpdateGuardarActualizarEnabled();
        }

        private async Task CargarServiciosAsync()
        {
            _servicios = (await _servicioRepo.GetAllAsync()).ToList();
            dataGridServicios.ItemsSource = _servicios;
        }

        private async Task CargarCuentasAsync()
        {
            var selectedId = cbCuenta.SelectedValue as int?;
            _cuentas = await _cuentaRepo.GetCuentasPendientesAsync();
            cbCuenta.ItemsSource = _cuentas;
            cbCuenta.SelectedValuePath = "idCuenta";
            if (selectedId.HasValue && _cuentas.Any(c => c.idCuenta == selectedId.Value))
                cbCuenta.SelectedValue = selectedId.Value;
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
                    // Evitar duplicados por nombre
                    if (_servicios.Any(x => (x.nombre ?? "").Equals(nombre, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("Ya existe un servicio con ese nombre.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            var s = (input ?? "").Trim();
            while (s.Contains("  ")) s = s.Replace("  ", " ");
            return s;
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
                MessageBox.Show("El nombre debe contener letras.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private bool ValidatePrecio(string? texto, out decimal precio)
        {
            precio = 0m;
            if (!decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out precio))
            {
                MessageBox.Show("Precio inválido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (precio <= 0 || precio > 10000)
            {
                MessageBox.Show("El precio debe ser mayor a 0 y razonable.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private bool ValidateDuracion(string? texto, out int? duracion)
        {
            duracion = null;
            if (string.IsNullOrWhiteSpace(texto)) return true;
            if (!int.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
            {
                MessageBox.Show("Duración inválida.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (d < 0 || d > 600)
            {
                MessageBox.Show("La duración debe estar entre 0 y 600 minutos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                
                // Actualizar el título del servicio seleccionado
                txtServicioSeleccionado.Text = $"→ {s.nombre}";
                
                _ = CargarDetallesPorServicioAsync(s.idServicio);
                LimpiarDetalleFormulario();
                ActualizarBotonServicio();
                UpdateGuardarActualizarEnabled();
            }
            else
            {
                txtServicioSeleccionado.Text = "";
                // Mostrar historial general al no tener servicio seleccionado
                dataGridDetalles.ItemsSource = _detallesAll;
                _detallesFiltered = new List<DetalleServicio>(_detallesAll);
                ActualizarBotonServicio();
                UpdateGuardarActualizarEnabled();
            }
        }

        private async Task CargarDetallesPorServicioAsync(int idServicio)
        {
            _detallesFiltered = (await _detalleRepo.GetByServicioAsync(idServicio)).ToList();
            dataGridDetalles.ItemsSource = _detallesFiltered;
        }

        private async Task CargarHistorialGeneralAsync()
        {
            _detallesAll = (await _detalleRepo.GetAllWithIncludesAsync()).ToList();
            _detallesFiltered = new List<DetalleServicio>(_detallesAll);
            dataGridDetalles.ItemsSource = _detallesFiltered;
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

        private void RecalcularSubtotal(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtCantidad.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var c) &&
                decimal.TryParse(txtPrecioUnitario.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var u))
                txtSubtotal.Text = (c * u).ToString("0.00", CultureInfo.InvariantCulture);
            else
                txtSubtotal.Text = "0.00";
        }

        private async void btnActualizarDetalle_Click(object sender, RoutedEventArgs e)
        {
            if (_detalleEnEdicion == null)
            {
                MessageBox.Show("Seleccione un detalle para actualizar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            try
            {
                if (cbCuenta.SelectedValue is int idCuenta)
                    _detalleEnEdicion.idCuenta = idCuenta;
                
                if (!int.TryParse(txtCantidad.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var cantidad) || cantidad <= 0)
                {
                    MessageBox.Show("Ingrese una cantidad válida.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (!decimal.TryParse(txtPrecioUnitario.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var unit) || unit <= 0)
                {
                    MessageBox.Show("Ingrese un precio unitario válido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                _detalleEnEdicion.cantidad = cantidad;
                _detalleEnEdicion.precioUnitario = unit;
                _detalleEnEdicion.subtotal = unit * cantidad;

                await _detalleRepo.UpdateAsync(_detalleEnEdicion);
                
                // Refrescar listas
                await CargarHistorialGeneralAsync();
                if (dataGridServicios.SelectedItem is Servicio s)
                    await CargarDetallesPorServicioAsync(s.idServicio);
                
                LimpiarDetalleFormulario();
                MessageBox.Show("Detalle actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnEliminarDetalle_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridDetalles.SelectedItem is not DetalleServicio d)
            {
                MessageBox.Show("Seleccione un detalle para eliminar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (MessageBox.Show("¿Está seguro de eliminar este detalle?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;
            
            try
            {
                await _detalleRepo.DeleteAsync(d.idDetalleServicio);
                
                // Refrescar listas
                await CargarHistorialGeneralAsync();
                if (dataGridServicios.SelectedItem is Servicio s)
                    await CargarDetallesPorServicioAsync(s.idServicio);
                
                LimpiarDetalleFormulario();
                MessageBox.Show("Detalle eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dataGridDetalles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridDetalles.SelectedItem is DetalleServicio d)
            {
                _detalleEnEdicion = d;
                cbCuenta.SelectedValue = d.idCuenta;
                txtCantidad.Text = d.cantidad.ToString(CultureInfo.InvariantCulture);
                txtPrecioUnitario.Text = d.precioUnitario.ToString(CultureInfo.InvariantCulture);
                txtSubtotal.Text = d.subtotal.ToString("0.00", CultureInfo.InvariantCulture);
            }
        }

        private void btnLimpiarDetalle_Click(object sender, RoutedEventArgs e)
        {
            LimpiarDetalleFormulario();
        }

        private void LimpiarDetalleFormulario()
        {
            _detalleEnEdicion = null;
            txtCantidad.Clear();
            txtPrecioUnitario.Clear();
            txtSubtotal.Text = "0.00";
            dataGridDetalles.UnselectAll();
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
            txtServicioSeleccionado.Text = "";
            _detallesFiltered.Clear();
            dataGridDetalles.ItemsSource = _detallesAll;
            ActualizarBotonServicio();
            UpdateGuardarActualizarEnabled();
        }

        private void ActualizarBotonServicio()
        {
            var editando = dataGridServicios.SelectedItem is Servicio;
            btnGuardarActualizar.Content = editando ? "✏️ Actualizar" : "💾 Guardar";
            btnGuardarActualizar.Style = (Style)FindResource(editando ? "PrimaryButton" : "SuccessButton");
        }

        private void btnBuscarDetalle_Click(object sender, RoutedEventArgs e)
        {
            // Sin filtros adicionales: mantiene la lista actual
            dataGridDetalles.ItemsSource = (dataGridServicios.SelectedItem is Servicio) ? _detallesFiltered : _detallesAll;
        }

        private async void btnTodosDetalle_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridServicios.SelectedItem is Servicio s)
                await CargarDetallesPorServicioAsync(s.idServicio);
            else
                dataGridDetalles.ItemsSource = _detallesAll;
        }
    }
}