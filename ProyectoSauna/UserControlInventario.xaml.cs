using Microsoft.Extensions.DependencyInjection;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories.Base;
using ProyectoSauna.Repositories.Interfaces;
using ProyectoSauna.Services; 
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
        private readonly IProductoRepository _productoRepo;
        private readonly CategoriaProductoRepository _categoriaRepo;
        private readonly ITipoMovimientoRepository _tipoMovimientoRepo;
        private readonly IMovimientoInventarioRepository _movimientoRepo;

        private List<Producto> listaProductos = new();
        private List<CategoriaProducto> listaCategorias = new();

        private MovimientoInventario? _movimientoEnEdicion = null;
        private int _stockAnteriorMovimiento = 0;

        private bool _isLoading = false;
        private bool _isInitialized = false;

        private EventHandler? _stockChangedHandler;

        public UserControlInventario()
        {
            InitializeComponent();

            _productoRepo = App.AppHost!.Services.GetRequiredService<IProductoRepository>();
            _categoriaRepo = App.AppHost!.Services.GetRequiredService<CategoriaProductoRepository>();
            _tipoMovimientoRepo = App.AppHost!.Services.GetRequiredService<ITipoMovimientoRepository>();
            _movimientoRepo = App.AppHost!.Services.GetRequiredService<IMovimientoInventarioRepository>();

            _stockChangedHandler = (s, e) =>
            {
                _ = Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    _isLoading = false;
                    await RefrescarDatosAsync();
                });
            };
            InventoryEventService.StockChanged += _stockChangedHandler;

            Loaded += async (_, __) => await RefrescarDatosAsync();

            Unloaded += (s, e) =>
            {
                if (_stockChangedHandler != null)
                    InventoryEventService.StockChanged -= _stockChangedHandler;
            };
        }

        public async Task RefrescarDatosAsync()
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                if (!_isInitialized)
                {
                    dpFecha.SelectedDate = DateTime.Now;
                    await CargarCategoriasAsync();
                    rbEntrada.IsChecked = true;
                    _isInitialized = true;
                }

                await CargarProductosAsync();
                await CargarMovimientosRecientesAsync();
                VerificarStockBajo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al refrescar: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

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

        private async Task CargarCategoriasAsync()
        {
            listaCategorias = (await _categoriaRepo.GetAllAsync()).ToList();

            var listaFiltro = new List<CategoriaProducto> {
                new CategoriaProducto { idCategoriaProducto = 0, nombre = "Todas las categorías" }
            };
            listaFiltro.AddRange(listaCategorias);

            cbCategorias.ItemsSource = listaFiltro;
            cbCategorias.SelectedValuePath = "idCategoriaProducto";
            cbCategorias.DisplayMemberPath = "nombre";
            cbCategorias.SelectedIndex = 0;

            cbCategoriaProducto.ItemsSource = listaCategorias;
            cbCategoriaProducto.DisplayMemberPath = "nombre";
            cbCategoriaProducto.SelectedValuePath = "idCategoriaProducto";
        }

        private async Task CargarProductosAsync()
        {
            if (_productoRepo is ProyectoSauna.Repositories.ProductoRepository pr)
                listaProductos = (await pr.GetAllFreshAsync()).ToList();
            else
                listaProductos = (await _productoRepo.GetAllAsync()).ToList();
            dataGridProductos.ItemsSource = listaProductos;
            ActualizarEstadisticas();
            await ValidarUIContraBDAsync();
        }

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

                cbCategoriaProducto.SelectedValue = p.idCategoriaProducto;

                await CargarMovimientosPorProductoAsync(p.idProducto);
            }
        }

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

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            var f = (txtBuscar.Text ?? "").Trim().ToLowerInvariant();
            var data = listaProductos
                .Where(p => (!string.IsNullOrEmpty(p.nombre) && p.nombre.ToLowerInvariant().Contains(f))
                         || (!string.IsNullOrEmpty(p.codigo) && p.codigo.ToLowerInvariant().Contains(f)))
                .ToList();
            dataGridProductos.ItemsSource = data;

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
            txtCodigo.Clear();
            txtNombre.Clear();
            txtDescripcion.Clear();
            txtPrecioCompra.Clear();
            txtPrecioVenta.Clear();
            txtStockActual.Text = "0";
            txtStockMinimo.Text = "0";
            cbCategoriaProducto.SelectedIndex = -1;

            LimpiarFormularioMovimiento();

            dataGridProductos.UnselectAll();
            txtBuscar.Clear();

            txtStockActual.Background = Brushes.Transparent;
            txtStockMinimo.Background = Brushes.Transparent;
        }

        private void LimpiarFormularioMovimiento()
        {
            txtCantidad.Clear();
            txtCostoUnitario.Clear();
            txtCostoTotal.Text = "0.00";
            txtObservaciones.Clear();
            dpFecha.SelectedDate = DateTime.Now;

            rbEntrada.IsChecked = true;
            rbSalida.IsChecked = false;

            _movimientoEnEdicion = null;
            _stockAnteriorMovimiento = 0;

            dataGridMovimientos.UnselectAll();

            ActualizarBotonMovimiento();
        }

        private async void dataGridMovimientos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized || _isLoading) return;

            if (dataGridMovimientos.SelectedItem is MovimientoInventario mov)
            {
                try
                {
                    _movimientoEnEdicion = mov;

                    var producto = listaProductos.FirstOrDefault(p => p.idProducto == mov.idProducto);
                    if (producto != null)
                    {
                        var esEntrada = await EsMovimientoEntrada(mov.idTipoMovimiento);
                        _stockAnteriorMovimiento = producto.stockActual - (esEntrada ? mov.cantidad : -mov.cantidad);
                    }

                    txtCantidad.Text = mov.cantidad.ToString(CultureInfo.InvariantCulture);
                    txtCostoUnitario.Text = mov.costoUnitario.ToString(CultureInfo.InvariantCulture);
                    txtCostoTotal.Text = mov.costoTotal.ToString("0.00", CultureInfo.InvariantCulture);
                    txtObservaciones.Text = mov.observaciones ?? "";
                    dpFecha.SelectedDate = mov.fecha;

                    var esEntradaMov = await EsMovimientoEntrada(mov.idTipoMovimiento);
                    rbEntrada.IsChecked = esEntradaMov;
                    rbSalida.IsChecked = !esEntradaMov;

                    ActualizarBotonMovimiento();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar movimiento: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ActualizarBotonMovimiento()
        {
            var boton = btnRegistrarMovimiento;

            if (_movimientoEnEdicion != null)
            {
                boton.Content = "✏️ ACTUALIZAR MOVIMIENTO";
                boton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
            }
            else
            {
                boton.Content = "✓ REGISTRAR MOVIMIENTO";
                boton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16C784"));
            }
        }

        private async Task<bool> EsMovimientoEntrada(int idTipoMovimiento)
        {
            var tipo = await _tipoMovimientoRepo.GetByIdAsync(idTipoMovimiento);
            if (tipo == null) return true;

            var desc = tipo.descripcion?.ToUpperInvariant() ?? "";
            return desc.Contains("ENTRADA") || desc == "E";
        }

        private async Task<int?> GetDefaultTipoIdAsync(bool esEntrada)
        {
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
            if (_movimientoEnEdicion != null)
            {
                await ActualizarMovimiento();
            }
            else
            {
                await RegistrarNuevoMovimiento();
            }
        }

        private async Task RegistrarNuevoMovimiento()
        {
            if (dataGridProductos.SelectedItem is not Producto prod)
            {
                MessageBox.Show("Seleccione un producto.", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtCantidad.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var cantDec) ||
                !decimal.TryParse(txtCostoUnitario.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var unit))
            {
                MessageBox.Show("Cantidad y costo unitario deben ser numéricos.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
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
            int stockAntes = prod.stockActual;
            int nuevoStock = stockAntes + (esEntrada ? +cantidad : -cantidad);

            if (nuevoStock < 0)
            {
                MessageBox.Show("La operación dejaría el stock en negativo.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
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

                prod.stockActual = nuevoStock;
                await _productoRepo.UpdateAsync(prod);

                ProyectoSauna.Services.AuditLogger.LogInventario(esEntrada ? "Entrada" : "Salida", prod, stockAntes, nuevoStock, mov.idUsuario, mov.observaciones ?? "");
                InventoryEventService.NotifyStockChanged();

                await CargarProductosAsync();
                await CargarMovimientosPorProductoAsync(prod.idProducto);
                await CargarMovimientosRecientesAsync();

                dataGridProductos.SelectedItem = listaProductos.FirstOrDefault(p => p.idProducto == prod.idProducto);

                MessageBox.Show("Movimiento registrado y stock actualizado.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                LimpiarFormularioMovimiento();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar movimiento: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ActualizarMovimiento()
        {
            if (_movimientoEnEdicion == null)
            {
                MessageBox.Show("No hay movimiento seleccionado para actualizar.", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtCantidad.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var cantDec) ||
                !decimal.TryParse(txtCostoUnitario.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var unit))
            {
                MessageBox.Show("Cantidad y costo unitario deben ser numéricos.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int nuevaCantidad = (int)Math.Round(cantDec);
                var esEntrada = rbEntrada.IsChecked == true;
                var idTipoMov = await GetDefaultTipoIdAsync(esEntrada);

                if (idTipoMov is null)
                {
                    MessageBox.Show("No hay tipos configurados para el movimiento seleccionado.", "Configuración",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Obtener el producto fresco desde la BD para evitar conflictos de rastreo
                var producto = await _productoRepo.GetByIdAsync(_movimientoEnEdicion.idProducto);
                if (producto == null)
                {
                    MessageBox.Show("No se encontró el producto asociado.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int stockAntes = _stockAnteriorMovimiento;
                int stockFinal = stockAntes + (esEntrada ? +nuevaCantidad : -nuevaCantidad);

                if (stockFinal < 0)
                {
                    MessageBox.Show("La actualización dejaría el stock en negativo.", "Validación",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Crear movimiento actualizado sin navegaciones
                var movimientoActualizado = new MovimientoInventario
                {
                    idMovimiento = _movimientoEnEdicion.idMovimiento,
                    cantidad = nuevaCantidad,
                    costoUnitario = unit,
                    costoTotal = unit * nuevaCantidad,
                    fecha = dpFecha.SelectedDate ?? DateTime.Now,
                    observaciones = txtObservaciones.Text,
                    idTipoMovimiento = idTipoMov.Value,
                    idProducto = _movimientoEnEdicion.idProducto,
                    idUsuario = _movimientoEnEdicion.idUsuario
                };

                // Actualizar movimiento primero
                await _movimientoRepo.UpdateAsync(movimientoActualizado);

                // Actualizar stock del producto
                producto.stockActual = stockFinal;
                await _productoRepo.UpdateAsync(producto);

                ProyectoSauna.Services.AuditLogger.LogInventario(
                    esEntrada ? "Entrada" : "Salida",
                    producto,
                    stockAntes,
                    stockFinal,
                    _movimientoEnEdicion.idUsuario,
                    txtObservaciones.Text ?? "");

                InventoryEventService.NotifyStockChanged();

                // Recargar todos los datos
                await CargarProductosAsync();
                await CargarMovimientosPorProductoAsync(producto.idProducto);
                await CargarMovimientosRecientesAsync();

                dataGridProductos.SelectedItem = listaProductos.FirstOrDefault(p => p.idProducto == producto.idProducto);

                MessageBox.Show("Movimiento actualizado correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                LimpiarFormularioMovimiento();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar movimiento: {ex.Message}\n\nDetalles: {ex.InnerException?.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ValidarUIContraBDAsync()
        {
            try
            {
                IEnumerable<Producto> frescos;
                if (_productoRepo is ProyectoSauna.Repositories.ProductoRepository pr)
                    frescos = await pr.GetAllFreshAsync();
                else
                    frescos = await _productoRepo.GetAllAsync();

                var mapa = frescos.ToDictionary(p => p.idProducto);
                bool mismatch = false;

                foreach (var p in listaProductos)
                {
                    if (mapa.TryGetValue(p.idProducto, out var real))
                    {
                        if (p.stockActual != real.stockActual || p.precioCompra != real.precioCompra || p.precioVenta != real.precioVenta)
                        {
                            p.stockActual = real.stockActual;
                            p.precioCompra = real.precioCompra;
                            p.precioVenta = real.precioVenta;
                            mismatch = true;
                        }
                    }
                }

                if (mismatch)
                {
                    dataGridProductos.ItemsSource = null;
                    dataGridProductos.ItemsSource = listaProductos;
                    ActualizarEstadisticas();
                }
            }
            catch
            {
                // No interrumpir la UI
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
                MessageBox.Show($"Error al cargar movimientos del producto: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Error al cargar movimientos recientes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
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