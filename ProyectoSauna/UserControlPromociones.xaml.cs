// UserControlPromociones.xaml.cs - CON FUNCIONALIDAD DE AGREGAR TIPO
using Microsoft.Extensions.DependencyInjection;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProyectoSauna
{
    public partial class UserControlPromociones : UserControl
    {
        private readonly IPromocionesRepository _promocionesRepo;
        private readonly ITipoDescuentoRepository _tipoDescuentoRepo;
        private List<Promociones> _promociones = new();
        private List<TipoDescuento> _tiposDescuento = new();
        private bool _isInitialized = false;

        public UserControlPromociones()
        {
            InitializeComponent();
            _promocionesRepo = App.AppHost!.Services.GetRequiredService<IPromocionesRepository>();
            _tipoDescuentoRepo = App.AppHost!.Services.GetRequiredService<ITipoDescuentoRepository>();
            Loaded += async (_, __) => await InicializarAsync();
        }

        private async System.Threading.Tasks.Task InicializarAsync()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            try
            {
                await CargarTiposDescuentoAsync();
                await CargarPromocionesAsync();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task CargarTiposDescuentoAsync()
        {
            _tiposDescuento = (await _tipoDescuentoRepo.GetAllAsync()).ToList();

            // Actualizar ComboBox del formulario
            cmbTipoDescuento.ItemsSource = null;
            cmbTipoDescuento.ItemsSource = _tiposDescuento;

            // Actualizar ComboBox de filtro
            var tiposFiltro = new List<TipoDescuento> { new TipoDescuento { idTipoDescuento = 0, nombre = "Todos" } };
            tiposFiltro.AddRange(_tiposDescuento);
            cmbTipoFiltro.ItemsSource = null;
            cmbTipoFiltro.ItemsSource = tiposFiltro;
            cmbTipoFiltro.SelectedIndex = 0;
        }

        private async System.Threading.Tasks.Task CargarPromocionesAsync()
        {
            _promociones = (await _promocionesRepo.ObtenerTodasConTipoAsync()).ToList();
            AplicarFiltros();
        }

        private void Filtros_Changed(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            if (dataGridPromociones == null) return;

            IEnumerable<Promociones> datos = _promociones;

            var textoBusqueda = (txtBuscar?.Text ?? string.Empty).Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(textoBusqueda))
            {
                datos = datos.Where(p =>
                    p.nombreDescuento.ToLower().Contains(textoBusqueda) ||
                    p.motivo.ToLower().Contains(textoBusqueda) ||
                    (p.idTipoDescuentoNavigation?.nombre ?? "").ToLower().Contains(textoBusqueda));
            }

            if (cmbTipoFiltro?.SelectedValue != null && (int)cmbTipoFiltro.SelectedValue > 0)
            {
                var idTipo = (int)cmbTipoFiltro.SelectedValue;
                datos = datos.Where(p => p.idTipoDescuento == idTipo);
            }

            if (chkSoloActivos?.IsChecked == true)
            {
                datos = datos.Where(p => p.activo);
            }

            dataGridPromociones.ItemsSource = datos.ToList();
        }

        // NUEVO: Mostrar popup para agregar tipo
        private void btnAgregarTipo_Click(object sender, RoutedEventArgs e)
        {
            txtNuevoTipo.Clear();
            popupAgregarTipo.Visibility = Visibility.Visible;
            txtNuevoTipo.Focus();
        }

        // NUEVO: Guardar nuevo tipo
        private async void btnGuardarTipo_Click(object sender, RoutedEventArgs e)
        {
            var nuevoNombre = txtNuevoTipo.Text.Trim();

            if (string.IsNullOrWhiteSpace(nuevoNombre))
            {
                MessageBox.Show("El nombre del tipo es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_tiposDescuento.Any(t => t.nombre.Equals(nuevoNombre, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Ya existe un tipo con ese nombre.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var nuevoTipo = new TipoDescuento { nombre = nuevoNombre };
                await _tipoDescuentoRepo.AddAsync(nuevoTipo);

                // Recargar tipos y actualizar ComboBox en tiempo real
                await CargarTiposDescuentoAsync();

                // Seleccionar el nuevo tipo automáticamente
                cmbTipoDescuento.SelectedValue = nuevoTipo.idTipoDescuento;

                MessageBox.Show("Tipo de descuento agregado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                popupAgregarTipo.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // NUEVO: Cancelar agregar tipo
        private void btnCancelarTipo_Click(object sender, RoutedEventArgs e)
        {
            popupAgregarTipo.Visibility = Visibility.Collapsed;
        }

        private void dataGridPromociones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;

            if (dataGridPromociones.SelectedItem is Promociones promo)
            {
                txtNombreDescuento.Text = promo.nombreDescuento;
                cmbTipoDescuento.SelectedValue = promo.idTipoDescuento;
                txtMontoDescuento.Text = promo.montoDescuento.ToString(CultureInfo.InvariantCulture);
                txtValorCondicion.Text = promo.valorCondicion.ToString(CultureInfo.InvariantCulture);
                txtMotivo.Text = promo.motivo;
                chkActivo.IsChecked = promo.activo;

                btnGuardarActualizar.Content = "✏️ Actualizar";
                btnGuardarActualizar.Style = (Style)FindResource("PrimaryButton");
                btnEliminar.Visibility = Visibility.Visible;
            }
            else
            {
                btnGuardarActualizar.Content = "💾 Guardar";
                btnGuardarActualizar.Style = (Style)FindResource("SuccessButton");
                btnEliminar.Visibility = Visibility.Collapsed;
            }
            UpdateGuardarEnabled();
        }

        private async void btnGuardarActualizar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs(out var nombreDescuento, out var idTipo, out var monto, out var valorCondicion, out var motivo, out var activo))
                return;

            try
            {
                if (dataGridPromociones.SelectedItem is Promociones promo)
                {
                    promo.nombreDescuento = nombreDescuento;
                    promo.idTipoDescuento = idTipo;
                    promo.montoDescuento = monto;
                    promo.valorCondicion = valorCondicion;
                    promo.motivo = motivo;
                    promo.activo = activo;

                    await _promocionesRepo.UpdateAsync(promo);
                    MessageBox.Show("Promoción actualizada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var nueva = new Promociones
                    {
                        nombreDescuento = nombreDescuento,
                        idTipoDescuento = idTipo,
                        montoDescuento = monto,
                        valorCondicion = valorCondicion,
                        motivo = motivo,
                        activo = activo
                    };

                    await _promocionesRepo.AddAsync(nueva);
                    MessageBox.Show("Promoción guardada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                await CargarPromocionesAsync();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridPromociones.SelectedItem is not Promociones promo)
                return;

            var result = MessageBox.Show(
                $"¿Está seguro de eliminar la promoción '{promo.nombreDescuento}'?\n\nEsta acción no se puede deshacer.",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _promocionesRepo.DeleteAsync(promo.idPromocion);
                MessageBox.Show("Promoción eliminada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                await CargarPromocionesAsync();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            txtNombreDescuento.Clear();
            cmbTipoDescuento.SelectedIndex = -1;
            txtMontoDescuento.Clear();
            txtValorCondicion.Clear();
            txtMotivo.Clear();
            chkActivo.IsChecked = true;
            dataGridPromociones.UnselectAll();
            btnGuardarActualizar.Content = "💾 Guardar";
            btnGuardarActualizar.Style = (Style)FindResource("SuccessButton");
            btnEliminar.Visibility = Visibility.Collapsed;
            UpdateGuardarEnabled();
        }

        private void Formulario_Changed(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            UpdateGuardarEnabled();
        }

        private void UpdateGuardarEnabled()
        {
            if (btnGuardarActualizar == null) return;

            // ✅ VALIDACIONES MEJORADAS EN TIEMPO REAL
            var nombre = txtNombreDescuento?.Text?.Trim() ?? string.Empty;
            var nombreOk = !string.IsNullOrWhiteSpace(nombre) && nombre.Length >= 3 && nombre.Length <= 100;
            
            var tipoOk = cmbTipoDescuento?.SelectedValue != null;
            
            var montoTexto = txtMontoDescuento?.Text?.Trim() ?? string.Empty;
            var montoOk = !string.IsNullOrWhiteSpace(montoTexto) && 
                         decimal.TryParse(montoTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out var m) && m > 0;
            
            var condicionTexto = txtValorCondicion?.Text?.Trim() ?? string.Empty;
            var condicionOk = !string.IsNullOrWhiteSpace(condicionTexto) && 
                             int.TryParse(condicionTexto, out var c) && c > 0;
            
            var motivo = txtMotivo?.Text?.Trim() ?? string.Empty;
            var motivoOk = !string.IsNullOrWhiteSpace(motivo) && motivo.Length >= 5 && motivo.Length <= 200;

            btnGuardarActualizar.IsEnabled = nombreOk && tipoOk && montoOk && condicionOk && motivoOk;
        }

        private void NumericDecimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var box = sender as TextBox;
            var proposed = (box?.Text ?? string.Empty) + e.Text;
            e.Handled = !decimal.TryParse(proposed, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
        }

        private void OnPasteNumericDecimal(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                var text = e.DataObject.GetData(DataFormats.Text) as string;
                if (!decimal.TryParse(text ?? string.Empty, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        // 🔢 VALIDACIÓN PARA NÚMEROS ENTEROS (CONDICIÓN)
        private void NumericInteger_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var box = sender as TextBox;
            var proposed = (box?.Text ?? string.Empty) + e.Text;
            
            // Solo permitir dígitos (números enteros positivos)
            e.Handled = !int.TryParse(proposed, out var result) || result < 0;
        }

        private void OnPasteNumericInteger(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                var text = e.DataObject.GetData(DataFormats.Text) as string;
                if (!int.TryParse(text ?? string.Empty, out var result) || result < 0)
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool ValidateInputs(out string nombreDescuento, out int idTipo, out decimal monto, out int valorCondicion, out string motivo, out bool activo)
        {
            nombreDescuento = string.Empty;
            idTipo = 0;
            monto = 0;
            valorCondicion = 0;
            motivo = string.Empty;
            activo = chkActivo.IsChecked == true;

            // ✅ VALIDACIÓN NOMBRE (OBLIGATORIO + LONGITUD MÍNIMA)
            nombreDescuento = txtNombreDescuento.Text.Trim();
            if (string.IsNullOrWhiteSpace(nombreDescuento))
            {
                MessageBox.Show("❌ El nombre de la promoción es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombreDescuento.Focus();
                return false;
            }
            if (nombreDescuento.Length < 3)
            {
                MessageBox.Show("❌ El nombre debe tener al menos 3 caracteres.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombreDescuento.Focus();
                return false;
            }
            if (nombreDescuento.Length > 100)
            {
                MessageBox.Show("❌ El nombre no puede exceder 100 caracteres.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombreDescuento.Focus();
                return false;
            }

            // ✅ VALIDACIÓN TIPO (OBLIGATORIO)
            if (cmbTipoDescuento.SelectedValue == null)
            {
                MessageBox.Show("❌ Debe seleccionar un tipo de descuento.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbTipoDescuento.Focus();
                return false;
            }
            idTipo = (int)cmbTipoDescuento.SelectedValue;

            // ✅ VALIDACIÓN MONTO (OBLIGATORIO + POSITIVO)
            if (string.IsNullOrWhiteSpace(txtMontoDescuento.Text))
            {
                MessageBox.Show("❌ El monto de descuento es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMontoDescuento.Focus();
                return false;
            }
            if (!decimal.TryParse(txtMontoDescuento.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out monto))
            {
                MessageBox.Show("❌ El monto debe ser un número válido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMontoDescuento.Focus();
                return false;
            }
            if (monto <= 0)
            {
                MessageBox.Show("❌ El monto debe ser mayor que 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMontoDescuento.Focus();
                return false;
            }

            // ✅ VALIDACIÓN CONDICIÓN (OBLIGATORIO + ENTERO POSITIVO)
            if (string.IsNullOrWhiteSpace(txtValorCondicion.Text))
            {
                MessageBox.Show("❌ El valor de condición es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtValorCondicion.Focus();
                return false;
            }
            if (!int.TryParse(txtValorCondicion.Text, out valorCondicion))
            {
                MessageBox.Show("❌ El valor de condición debe ser un número entero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtValorCondicion.Focus();
                return false;
            }
            if (valorCondicion <= 0)
            {
                MessageBox.Show("❌ El valor de condición debe ser mayor que 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtValorCondicion.Focus();
                return false;
            }

            // ✅ VALIDACIÓN MOTIVO (OBLIGATORIO + LONGITUD MÍNIMA)
            motivo = txtMotivo.Text.Trim();
            if (string.IsNullOrWhiteSpace(motivo))
            {
                MessageBox.Show("❌ El motivo/descripción es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMotivo.Focus();
                return false;
            }
            if (motivo.Length < 5)
            {
                MessageBox.Show("❌ El motivo debe tener al menos 5 caracteres.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMotivo.Focus();
                return false;
            }
            if (motivo.Length > 200)
            {
                MessageBox.Show("❌ El motivo no puede exceder 200 caracteres.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMotivo.Focus();
                return false;
            }

            return true;
        }
    }
}