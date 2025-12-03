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
using System.Windows.Media;

namespace ProyectoSauna
{
    public partial class UserControlInventario : UserControl
    {
        // ======== REPOS ========
        private readonly IProductoRepository _productoRepo;
        private readonly CategoriaProductoRepository _categoriaRepo;
        private readonly ITipoMovimientoRepository _tipoMovimientoRepo;
        private readonly IMovimientoInventarioRepository _movimientoRepo;

        // ======== CACHE ========
        private List<Producto> listaProductos = new();
        private List<CategoriaProducto> listaCategorias = new();

        // control de carga
        private bool _isLoading = false;
        private bool _isInitialized = false;

        public UserControlInventario()
        {
            InitializeComponent();

            _productoRepo = App.AppHost!.Services.GetRequiredService<IProductoRepository>();
            _categoriaRepo = App.AppHost!.Services.GetRequiredService<CategoriaProductoRepository>();
            _tipoMovimientoRepo = App.AppHost!.Services.GetRequiredService<ITipoMovimientoRepository>();
            _movimientoRepo = App.AppHost!.Services.GetRequiredService<IMovimientoInventarioRepository>();

            Loaded += async (_, __) => await InicializarAsync();
        }

        private async Task InicializarAsync()
        {
            if (_isLoading || _isInitialized) return;
            _isLoading = true;
            try
            {
                dpFecha.SelectedDate = DateTime.Now;

                await CargarCategoriasAsync();
                await CargarProductosAsync();

                rbEntrada.IsChecked = true; // por defecto
                await CargarMovimientosRecientesAsync();

                VerificarStockBajo();

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { _isLoading = false; }
        }

        // ============== STOCKMINIMO =================

        private void VerificarStockBajo()
        {
            var productosCriticos = listaProductos
                .Where(p => p.stockActual <= p.stockMinimo)
                .ToList();

            if (productosCriticos.Any())
            {
                string mensaje = "⚠ Productos con stock crítico:\n\n" +
                    string.Join("\n", productosCriticos.Select(p =>
                        $"- {p.codigo}: {p.nombre} (Stock: {p.stockActual} / Mínimo: {p.stockMinimo})"));

                MessageBox.Show(mensaje, "Alerta de Stock Bajo",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        // ============== CARGAS =================

        private async Task CargarCategoriasAsync()
        {
            listaCategorias = (await _categoriaRepo.GetAllAsync()).ToList();

            // Para el FILTRO (cbCategorias) - con "Todas las categorías"
            var listaFiltro = new List<CategoriaProducto> {
                new CategoriaProducto { idCategoriaProducto = 0, nombre = "Todas las categorías" }
            };
            listaFiltro.AddRange(listaCategorias);

            cbCategorias.ItemsSource = listaFiltro;
            cbCategorias.SelectedValuePath = "idCategoriaProducto";
            cbCategorias.DisplayMemberPath = "nombre";
            cbCategorias.SelectedIndex = 0;

            // Para el FORMULARIO (cbCategoriaProducto) - sin "Todas"
            cbCategoriaProducto.ItemsSource = listaCategorias;
            cbCategoriaProducto.DisplayMemberPath = "nombre";
            cbCategoriaProducto.SelectedValuePath = "idCategoriaProducto";
        }

        private async Task CargarProductosAsync()
        {
            listaProductos = (await _productoRepo.GetAllAsync()).ToList();
            dataGridProductos.ItemsSource = listaProductos;
            ActualizarEstadisticas(); // Actualizar KPIs
        }

        // ============== ESTADÍSTICAS KPI ==============

        private void ActualizarEstadisticas()
        {
            if (listaProductos == null || listaProductos.Count == 0)
            {
                txtTotalProductos.Text = "0";
                txtStockTotal.Text = "0";
                txtStockBajo.Text = "0";
                return;
            }

            txtTotalProductos.Text = listaProductos.Count.ToString();
            txtStockTotal.Text = listaProductos.Sum(p => p.stockActual).ToString();
            txtStockBajo.Text = listaProductos.Count(p => p.stockActual <= p.stockMinimo).ToString();
        }

        private async void cbCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized || _isLoading) return;
            if (cbCategorias.SelectedItem is not CategoriaProducto cat) return;

            if (cat.idCategoriaProducto == 0)
            {
                await CargarProductosAsync();
            }
            else
            {
                var filtrado = await _productoRepo.ObtenerPorCategoriaAsync(cat.idCategoriaProducto);
                var listaFiltrada = filtrado.ToList();
                dataGridProductos.ItemsSource = listaFiltrada;

                // Actualizar estadísticas con los datos filtrados
                txtTotalProductos.Text = listaFiltrada.Count.ToString();
                txtStockTotal.Text = listaFiltrada.Sum(p => p.stockActual).ToString();
                txtStockBajo.Text = listaFiltrada.Count(p => p.stockActual <= p.stockMinimo).ToString();
            }
        }

        private async void dataGridProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized || _isLoading) return;

            if (dataGridProductos.SelectedItem is Producto p)
            {
                txtCodigo.Text = p.codigo;
                txtNombre.Text = p.nombre;
                txtDescripcion.Text = p.descripcion;
                txtPrecioCompra.Text = p.precioCompra.ToString(CultureInfo.InvariantCulture);
                txtPrecioVenta.Text = p.precioVenta.ToString(CultureInfo.InvariantCulture);
                txtStockActual.Text = p.stockActual.ToString(CultureInfo.InvariantCulture);
                txtStockMinimo.Text = p.stockMinimo.ToString(CultureInfo.InvariantCulture);

                // Seleccionar categoría en el combo del formulario
                cbCategoriaProducto.SelectedValue = p.idCategoriaProducto;

                await CargarMovimientosPorProductoAsync(p.idProducto);
            }
        }

        // ============== CRUD PRODUCTO ==============

        private async void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                    string.IsNullOrWhiteSpace(txtNombre.Text) ||
                    string.IsNullOrWhiteSpace(txtPrecioCompra.Text) ||
                    string.IsNullOrWhiteSpace(txtStockActual.Text) ||
                    string.IsNullOrWhiteSpace(txtStockMinimo.Text) ||
                    cbCategoriaProducto.SelectedValue == null)
                {
                    MessageBox.Show("Complete los campos obligatorios (incluyendo categoría).", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var nuevo = new Producto
                {
                    codigo = txtCodigo.Text.Trim(),
                    nombre = txtNombre.Text.Trim(),
                    descripcion = txtDescripcion.Text?.Trim(),
                    precioCompra = decimal.Parse(txtPrecioCompra.Text, CultureInfo.InvariantCulture),
                    precioVenta = string.IsNullOrWhiteSpace(txtPrecioVenta.Text) ? 0 :
                                         decimal.Parse(txtPrecioVenta.Text, CultureInfo.InvariantCulture),
                    stockActual = int.Parse(txtStockActual.Text, CultureInfo.InvariantCulture),
                    stockMinimo = int.Parse(txtStockMinimo.Text, CultureInfo.InvariantCulture),
                    idCategoriaProducto = (int)cbCategoriaProducto.SelectedValue
                };

                await _productoRepo.AddAsync(nuevo);
                await CargarProductosAsync();
                MessageBox.Show("Producto agregado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnModificar_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridProductos.SelectedItem is not Producto p) return;

            try
            {
                if (cbCategoriaProducto.SelectedValue == null)
                {
                    MessageBox.Show("Seleccione una categoría.", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                p.codigo = txtCodigo.Text.Trim();
                p.nombre = txtNombre.Text.Trim();
                p.descripcion = txtDescripcion.Text?.Trim();
                p.precioCompra = decimal.Parse(txtPrecioCompra.Text, CultureInfo.InvariantCulture);
                p.precioVenta = decimal.Parse(txtPrecioVenta.Text, CultureInfo.InvariantCulture);
                p.stockActual = int.Parse(txtStockActual.Text, CultureInfo.InvariantCulture);
                p.stockMinimo = int.Parse(txtStockMinimo.Text, CultureInfo.InvariantCulture);
                p.idCategoriaProducto = (int)cbCategoriaProducto.SelectedValue;

                await _productoRepo.UpdateAsync(p);
                await CargarProductosAsync();
                MessageBox.Show("Producto modificado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al modificar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridProductos.SelectedItem is not Producto p) return;

            if (MessageBox.Show("¿Eliminar producto?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                await _productoRepo.DeleteAsync(p.idProducto);
                await CargarProductosAsync();
                MessageBox.Show("Producto eliminado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============== BUSCAR / LIMPIAR ==============

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            var f = (txtBuscar.Text ?? "").Trim().ToLowerInvariant();
            var data = listaProductos
                .Where(p => (!string.IsNullOrEmpty(p.nombre) && p.nombre.ToLowerInvariant().Contains(f))
                         || (!string.IsNullOrEmpty(p.codigo) && p.codigo.ToLowerInvariant().Contains(f)))
                .ToList();
            dataGridProductos.ItemsSource = data;

            // Actualizar estadísticas con resultados de búsqueda
            txtTotalProductos.Text = data.Count.ToString();
            txtStockTotal.Text = data.Sum(p => p.stockActual).ToString();
            txtStockBajo.Text = data.Count(p => p.stockActual <= p.stockMinimo).ToString();
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            dataGridProductos.UnselectAll();
        }

        private void LimpiarFormulario()
        {
            // --- Sección de Información del Producto ---
            txtCodigo.Clear();
            txtNombre.Clear();
            txtDescripcion.Clear();
            txtPrecioCompra.Clear();
            txtPrecioVenta.Clear();
            txtStockActual.Text = "0";
            txtStockMinimo.Text = "0";
            cbCategoriaProducto.SelectedIndex = -1;

            // --- Sección de Movimiento ---
            txtCantidad.Clear();
            txtCostoUnitario.Clear();
            txtCostoTotal.Text = "0.00";
            txtObservaciones.Clear();
            dpFecha.SelectedDate = DateTime.Now;

            // --- RadioButtons de tipo de movimiento ---
            rbEntrada.IsChecked = false;
            rbSalida.IsChecked = false;

            // --- DataGrid y otros ---
            dataGridProductos.UnselectAll();  // Deselecciona cualquier producto
            txtBuscar.Clear();              // Limpia campo de búsqueda si lo usas

            // --- (Opcional) Restablecer color de indicadores ---
            txtStockActual.Background = Brushes.Transparent;
            txtStockMinimo.Background = Brushes.Transparent;
        }


        // ============== MOVIMIENTOS (AUTOMÁTICOS) ==============

        private async Task<int?> GetDefaultTipoIdAsync(bool esEntrada)
        {
            // Soporta valores: "Entrada", "Salida", "Entrada/Proveedor" y también "E"/"S" si existieran
            var lista = new List<TipoMovimiento>();

            if (esEntrada)
            {
                lista.AddRange((await _tipoMovimientoRepo.GetByTipoAsync("Entrada")).ToList());
                lista.AddRange((await _tipoMovimientoRepo.GetByTipoAsync("Entrada/Proveedor")).ToList());
                if (lista.Count == 0) lista.AddRange((await _tipoMovimientoRepo.GetByTipoAsync("E")).ToList());
            }
            else
            {
                lista.AddRange((await _tipoMovimientoRepo.GetByTipoAsync("Salida")).ToList());
                if (lista.Count == 0) lista.AddRange((await _tipoMovimientoRepo.GetByTipoAsync("S")).ToList());
            }

            return lista.OrderBy(t => t.descripcion).FirstOrDefault()?.idTipoMovimiento;
        }

        private void RecalcularCostoTotal(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtCantidad.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var c) &&
                decimal.TryParse(txtCostoUnitario.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var u))
                txtCostoTotal.Text = (c * u).ToString("0.00", CultureInfo.InvariantCulture);
            else
                txtCostoTotal.Text = "0.00";
        }

        private async void btnRegistrarMovimiento_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridProductos.SelectedItem is not Producto prod)
            {
                MessageBox.Show("Seleccione un producto.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtCantidad.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var cantDec) ||
                !decimal.TryParse(txtCostoUnitario.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var unit))
            {
                MessageBox.Show("Cantidad y costo unitario deben ser numéricos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var esEntrada = rbEntrada.IsChecked == true;
            var idTipoMov = await GetDefaultTipoIdAsync(esEntrada);

            if (idTipoMov is null)
            {
                MessageBox.Show("No hay tipos configurados para el movimiento seleccionado.", "Configuración",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int cantidad = (int)Math.Round(cantDec);
            int nuevoStock = prod.stockActual + (esEntrada ? +cantidad : -cantidad);
            if (nuevoStock < 0)
            {
                MessageBox.Show("La operación dejaría el stock en negativo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var mov = new MovimientoInventario
            {
                cantidad = cantidad,
                costoUnitario = unit,
                costoTotal = unit * cantidad,
                fecha = dpFecha.SelectedDate ?? DateTime.Now,
                observaciones = txtObservaciones.Text,
                idTipoMovimiento = idTipoMov.Value,
                idProducto = prod.idProducto,
                idUsuario = 5
            };

            try
            {
                await _movimientoRepo.AddAsync(mov);

                // Actualizar stock del producto
                prod.stockActual = nuevoStock;
                await _productoRepo.UpdateAsync(prod);

                await CargarProductosAsync();
                await CargarMovimientosPorProductoAsync(prod.idProducto);
                await CargarMovimientosRecientesAsync();

                dataGridProductos.SelectedItem = listaProductos.FirstOrDefault(p => p.idProducto == prod.idProducto);

                txtCantidad.Clear();
                txtCostoUnitario.Clear();
                txtCostoTotal.Text = "0.00";
                txtObservaciones.Clear();
                dpFecha.SelectedDate = DateTime.Now;

                MessageBox.Show("Movimiento registrado y stock actualizado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar movimiento: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CargarMovimientosPorProductoAsync(int idProducto)
        {
            try
            {
                var datos = (await _movimientoRepo.GetByProductoAsync(idProducto)).ToList();
                dataGridMovimientos.ItemsSource = datos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar movimientos del producto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CargarMovimientosRecientesAsync()
        {
            try
            {
                var datos = (await _movimientoRepo.GetRecentAsync(20)).ToList();
                dataGridMovimientos.ItemsSource = datos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar movimientos recientes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Radio handlers (solo por si luego quieres hacer algo al cambiar)
        private async void rbEntrada_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            await Task.CompletedTask;
        }

        private async void rbSalida_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            await Task.CompletedTask;
        }
    }
}
