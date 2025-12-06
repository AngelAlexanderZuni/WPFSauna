using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProyectoSauna.Models;
using ProyectoSauna.Models.Entities;
using ProyectoSauna.Repositories;
using ProyectoSauna.Repositories.Interfaces;
using ProyectoSauna.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ProyectoSauna.ViewModels
{
    public class CuentasViewModel : INotifyPropertyChanged
    {
        private readonly ICuentaRepository _cuentaRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IServicioRepository _servicioRepository;
        private readonly IDetalleConsumoRepository _detalleConsumoRepository;
        private readonly IDetalleServicioRepository _detalleServicioRepository;
        private readonly IMovimientoInventarioRepository _movimientoInventarioRepository;
        private readonly ITipoMovimientoRepository _tipoMovimientoRepository;
        private readonly Services.DescuentoService _descuentoService;
        private DispatcherTimer _timer;
        private DispatcherTimer _searchTimerProductos;
        private DispatcherTimer _searchTimerServicios;

        public CuentasViewModel()
        {
            _cuentaRepository = new CuentaRepository();

            using var context = new SaunaDbContext();
            _productoRepository = new ProductoRepository(context);
            _servicioRepository = new ServicioRepository(context);
            _detalleConsumoRepository = new DetalleConsumoRepository(context);
            _detalleServicioRepository = new DetalleServicioRepository(context);
            _movimientoInventarioRepository = new MovimientoInventarioRepository(context);
            _tipoMovimientoRepository = new TipoMovimientoRepository(context);
            
            // Inicializar DescuentoService
            var promocionesRepo = App.AppHost!.Services.GetRequiredService<IPromocionesRepository>();
            var clienteRepo = App.AppHost!.Services.GetRequiredService<IClienteRepository>();
            _descuentoService = new Services.DescuentoService(promocionesRepo, clienteRepo);

            CuentasPendientes = new ObservableCollection<CuentaPendiente>();
            ProductosDisponibles = new ObservableCollection<Producto>();
            ServiciosDisponibles = new ObservableCollection<Servicio>();
            ConsumosCuentaActual = new ObservableCollection<ConsumoItem>();

            ActualizarListaCommand = new RelayCommand(async () => await CargarCuentasPendientesAsync());
            BuscarClienteCommand = new RelayCommand(async () => await BuscarClienteAsync());
            CrearCuentaCommand = new RelayCommand(async () => await CrearCuentaAsync());
            LimpiarBusquedaCommand = new RelayCommand(async () => await LimpiarBusquedaAsync());
            CerrarCuentaCommand = new RelayCommand(async () => 
            {
                try
                {
                    await NavegarAPagosAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ ERROR en CerrarCuentaCommand: {ex.Message}");
                    MessageBox.Show($"Error al ejecutar comando de pago: {ex.Message}", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            EliminarCuentaCommand = new RelayCommand(async () => await EliminarCuentaAsync());
            AbrirModificarClienteCommand = new RelayCommand(AbrirPanelModificarCliente);
            CerrarModificarClienteCommand = new RelayCommand(CerrarPanelModificarCliente);
            ConfirmarModificarClienteCommand = new RelayCommand(async () => await ConfirmarModificarClienteAsync());

            BuscarProductosCommand = new RelayCommand(async () => await BuscarProductosAsync());
            BuscarServiciosCommand = new RelayCommand(async () => await BuscarServiciosAsync());
            AgregarProductoACuentaCommand = new RelayCommand(async () => await AgregarProductoACuentaAsync());
            AgregarServicioACuentaCommand = new RelayCommand(async () => await AgregarServicioACuentaAsync());
            EliminarConsumoCommand = new RelayCommand<ConsumoItem>(async (item) => await EliminarConsumoAsync(item));

            DevolverProductoCommand = new RelayCommand(async () => await DevolverProductoAsync());

            // ✅ NUEVO COMANDO PARA SELECCIONAR CUENTA
            SeleccionarCuentaCommand = new RelayCommand<CuentaPendiente>(async (cuenta) => await SeleccionarCuentaAsync(cuenta));

            // ✅ NUEVO COMANDO PARA LIMPIAR CUENTA ACTIVA
            LimpiarCuentaActivaCommand = new RelayCommand(async () => await LimpiarCuentaActiva());

            // 🔍 NUEVO COMANDO PARA LIMPIAR FILTRO
            LimpiarFiltroCommand = new RelayCommand(() => { LimpiarFiltroCuentas(); return Task.CompletedTask; });

            _ = CargarCuentasPendientesAsync();
            _ = CargarProductosAsync();
            _ = CargarServiciosAsync();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _timer.Tick += (s, e) => ActualizarTiempos();
            _timer.Start();

            _searchTimerProductos = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchTimerProductos.Tick += async (s, e) =>
            {
                _searchTimerProductos.Stop();
                await BuscarProductosAsync();
            };

            _searchTimerServicios = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchTimerServicios.Tick += async (s, e) =>
            {
                _searchTimerServicios.Stop();
                await BuscarServiciosAsync();
            };
        }

        #region Propiedades
        private bool _estaCargando;
        public bool EstaCargando
        {
            get => _estaCargando;
            set { _estaCargando = value; OnPropertyChanged(); }
        }

        // ✅ PROPIEDADES PARA BANNER DE CUENTA ACTIVA
        private bool _hayCuentaActiva;
        public bool HayCuentaActiva
        {
            get => _hayCuentaActiva;
            set { _hayCuentaActiva = value; OnPropertyChanged(); }
        }

        private string _nombreClienteActivo = "Sin cuenta seleccionada";
        public string NombreClienteActivo
        {
            get => _nombreClienteActivo;
            set { _nombreClienteActivo = value; OnPropertyChanged(); }
        }

        private string _dniClienteActivo = "-";
        public string DniClienteActivo
        {
            get => _dniClienteActivo;
            set { _dniClienteActivo = value; OnPropertyChanged(); }
        }

        private string _idCuentaActiva = "-";
        public string IdCuentaActiva
        {
            get => _idCuentaActiva;
            set { _idCuentaActiva = value; OnPropertyChanged(); }
        }

        private CuentaPendiente _cuentaSeleccionada;
        private int? _selectedCuentaId;
        public CuentaPendiente CuentaSeleccionada
        {
            get => _cuentaSeleccionada;
            set
            {
                _cuentaSeleccionada = value;
                _selectedCuentaId = value?.idCuenta;
                ProyectoSauna.Models.SesionActual.CuentaSeleccionadaId = value?.idCuenta ?? 0;

                // ✅ ACTUALIZAR BANNER DE CUENTA ACTIVA
                if (value != null)
                {
                    HayCuentaActiva = true;
                    NombreClienteActivo = value.NombreCliente;
                    DniClienteActivo = value.DocumentoCliente;
                    IdCuentaActiva = $"#{value.idCuenta}";
                    _ = CargarConsumosDeCuentaAsync(value.idCuenta);
                }
                else
                {
                    HayCuentaActiva = false;
                    NombreClienteActivo = "Sin cuenta seleccionada";
                    DniClienteActivo = "-";
                    IdCuentaActiva = "-";
                }

                OnPropertyChanged();
            }
        }

        private string _dniBusqueda = string.Empty;
        public string DniBusqueda
        {
            get => _dniBusqueda;
            set
            {
                _dniBusqueda = value;
                OnPropertyChanged();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    ClienteEncontrado = false;
                }
            }
        }

        private bool _clienteEncontrado;
        public bool ClienteEncontrado
        {
            get => _clienteEncontrado;
            set { _clienteEncontrado = value; OnPropertyChanged(); }
        }

        private string _nombreClienteBuscado = string.Empty;
        public string NombreClienteBuscado
        {
            get => _nombreClienteBuscado;
            set { _nombreClienteBuscado = value; OnPropertyChanged(); }
        }

        // 🎁 NUEVAS PROPIEDADES PARA PROMOCIONES
        private Services.InfoDescuentosCliente _infoDescuentos = new();
        public Services.InfoDescuentosCliente InfoDescuentos
        {
            get => _infoDescuentos;
            set { _infoDescuentos = value; OnPropertyChanged(); }
        }

        // 🔍 NUEVAS PROPIEDADES PARA FILTRO DE CUENTAS
        private string _filtroCuentas = string.Empty;
        public string FiltroCuentas
        {
            get => _filtroCuentas;
            set 
            { 
                _filtroCuentas = value; 
                OnPropertyChanged(); 
                FiltrarCuentasPendientes();
            }
        }

        private ObservableCollection<CuentaPendiente> _todasLasCuentas = new();
        public ObservableCollection<CuentaPendiente> TodasLasCuentas
        {
            get => _todasLasCuentas;
            set { _todasLasCuentas = value; OnPropertyChanged(); }
        }

        private int _idClienteEncontrado;
        public int IdClienteEncontrado
        {
            get => _idClienteEncontrado;
            set { _idClienteEncontrado = value; OnPropertyChanged(); }
        }

        private bool _mostrarPanelModificar;
        public bool MostrarPanelModificar
        {
            get => _mostrarPanelModificar;
            set { _mostrarPanelModificar = value; OnPropertyChanged(); }
        }

        private string _nuevoDniModificar = string.Empty;
        public string NuevoDniModificar
        {
            get => _nuevoDniModificar;
            set { _nuevoDniModificar = value; OnPropertyChanged(); }
        }

        private string _busquedaProducto = string.Empty;
        public string BusquedaProducto
        {
            get => _busquedaProducto;
            set
            {
                _busquedaProducto = value;
                OnPropertyChanged();
                _searchTimerProductos.Stop();
                _searchTimerProductos.Start();
            }
        }

        private Producto _productoSeleccionado;
        public Producto ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set { _productoSeleccionado = value; OnPropertyChanged(); }
        }

        private int _cantidadProducto = 1;
        public int CantidadProducto
        {
            get => _cantidadProducto;
            set { _cantidadProducto = value; OnPropertyChanged(); }
        }

        private string _busquedaServicio = string.Empty;
        public string BusquedaServicio
        {
            get => _busquedaServicio;
            set
            {
                _busquedaServicio = value;
                OnPropertyChanged();
                _searchTimerServicios.Stop();
                _searchTimerServicios.Start();
            }
        }

        private Servicio _servicioSeleccionado;
        public Servicio ServicioSeleccionado
        {
            get => _servicioSeleccionado;
            set { _servicioSeleccionado = value; OnPropertyChanged(); }
        }

        private int _cantidadServicio = 1;
        public int CantidadServicio
        {
            get => _cantidadServicio;
            set { _cantidadServicio = value; OnPropertyChanged(); }
        }

        private string _observacionesServicio = string.Empty;
        public string ObservacionesServicio
        {
            get => _observacionesServicio;
            set { _observacionesServicio = value; OnPropertyChanged(); }
        }

        private ConsumoItem _consumoSeleccionado;
        public ConsumoItem ConsumoSeleccionado
        {
            get => _consumoSeleccionado;
            set
            {
                _consumoSeleccionado = value;
                OnPropertyChanged();
                if (value != null)
                {
                    CantidadADevolver = 1;
                }
            }
        }

        private int _cantidadADevolver = 1;
        public int CantidadADevolver
        {
            get => _cantidadADevolver;
            set { _cantidadADevolver = value; OnPropertyChanged(); }
        }

        private decimal _totalProductos;
        public decimal TotalProductos
        {
            get => _totalProductos;
            set { _totalProductos = value; OnPropertyChanged(); CalcularTotalCuenta(); }
        }

        private decimal _totalServicios;
        public decimal TotalServicios
        {
            get => _totalServicios;
            set { _totalServicios = value; OnPropertyChanged(); CalcularTotalCuenta(); }
        }

        private decimal _totalCuenta;
        public decimal TotalCuenta
        {
            get => _totalCuenta;
            set { _totalCuenta = value; OnPropertyChanged(); }
        }

        private int _cantidadProductos;
        public int CantidadProductos
        {
            get => _cantidadProductos;
            set { _cantidadProductos = value; OnPropertyChanged(); ActualizarTotalItems(); }
        }

        private int _cantidadServicios;
        public int CantidadServicios
        {
            get => _cantidadServicios;
            set { _cantidadServicios = value; OnPropertyChanged(); ActualizarTotalItems(); }
        }

        private int _totalItemsCuenta;
        public int TotalItemsCuenta
        {
            get => _totalItemsCuenta;
            set { _totalItemsCuenta = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CuentaPendiente> CuentasPendientes { get; set; }
        public ObservableCollection<Producto> ProductosDisponibles { get; set; }
        public ObservableCollection<Servicio> ServiciosDisponibles { get; set; }
        public ObservableCollection<ConsumoItem> ConsumosCuentaActual { get; set; }
        #endregion

        #region Métodos principales
        // ✅ MODIFICADO: Ahora restaura la selección visual correctamente
        private async Task CargarCuentasPendientesAsync()
        {
            try
            {
                EstaCargando = true;

                var cuentasBD = await _cuentaRepository.GetCuentasPendientesAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    CuentasPendientes.Clear();
                    TodasLasCuentas.Clear(); // 🔍 LIMPIAR TAMBIÉN LA LISTA COMPLETA

                    foreach (var cuenta in cuentasBD)
                    {
                        var cuentaPendiente = new CuentaPendiente
                        {
                            idCuenta = cuenta.idCuenta,
                            NombreCliente = $"{cuenta.idClienteNavigation?.nombre} {cuenta.idClienteNavigation?.apellidos}",
                            DocumentoCliente = cuenta.idClienteNavigation?.numero_documento ?? "",
                            HoraIngreso = cuenta.fechaHoraCreacion.ToString("HH:mm"),
                            FechaHoraIngreso = cuenta.fechaHoraCreacion,
                            precioEntrada = cuenta.precioEntrada,
                            descuento = cuenta.descuento,
                            total = cuenta.total,
                            EstadoCuenta = cuenta.idEstadoCuentaNavigation?.nombre ?? ""
                        };

                        cuentaPendiente.ActualizarTiempo();
                        CuentasPendientes.Add(cuentaPendiente);
                        TodasLasCuentas.Add(cuentaPendiente); // 🔍 AGREGAR TAMBIÉN A LA LISTA COMPLETA
                    }

                    // ✅ RESTAURAR SELECCIÓN VISUAL Y LÓGICA
                    int idARestaurar = 0;

                    // Prioridad 1: Variable privada del ViewModel
                    if (_selectedCuentaId.HasValue)
                        idARestaurar = _selectedCuentaId.Value;
                    // Prioridad 2: Sesión global
                    else if (ProyectoSauna.Models.SesionActual.CuentaSeleccionadaId > 0)
                        idARestaurar = ProyectoSauna.Models.SesionActual.CuentaSeleccionadaId;

                    if (idARestaurar > 0)
                    {
                        // Buscar la cuenta en la lista recién cargada
                        var cuentaASeleccionar = CuentasPendientes.FirstOrDefault(c => c.idCuenta == idARestaurar);

                        if (cuentaASeleccionar != null)
                        {
                            // ✅ DESMARCAR TODAS LAS DEMÁS
                            foreach (var c in CuentasPendientes)
                            {
                                c.IsSelected = false;
                            }

                            // ✅ MARCAR COMO SELECCIONADA (para el RadioButton)
                            cuentaASeleccionar.IsSelected = true;

                            // ✅ ASIGNAR AL VIEWMODEL (dispara el PropertyChanged)
                            CuentaSeleccionada = cuentaASeleccionar;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Error al cargar cuentas: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
            finally
            {
                EstaCargando = false;
            }
        }

        // ✅ NUEVO MÉTODO: Seleccionar cuenta desde RadioButton
        private Task SeleccionarCuentaAsync(CuentaPendiente cuenta)
        {
            if (cuenta == null) return Task.CompletedTask;

            // Desmarcar todas las demás
            foreach (var c in CuentasPendientes)
            {
                c.IsSelected = false;
            }

            // Marcar la seleccionada
            cuenta.IsSelected = true;
            CuentaSeleccionada = cuenta;

            return Task.CompletedTask;
        }

        // ✅ NUEVO MÉTODO: Limpiar cuenta activa
        private Task LimpiarCuentaActiva()
        {
            var resultado = MessageBox.Show(
                "¿Desea dejar de trabajar con esta cuenta?\n\n" +
                "Podrá seleccionar otra cuenta después.",
                "Cambiar Cuenta",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                // Desmarcar todas
                foreach (var c in CuentasPendientes)
                {
                    c.IsSelected = false;
                }

                CuentaSeleccionada = null;
                ProyectoSauna.Models.SesionActual.CuentaSeleccionadaId = 0;
                ConsumosCuentaActual.Clear();
            }

            return Task.CompletedTask;
        }

        private async Task EnsureCuentaSeleccionadaAsync()
        {
            if (CuentaSeleccionada != null) return;

            var idSesion = ProyectoSauna.Models.SesionActual.CuentaSeleccionadaId;
            if (idSesion > 0)
            {
                var keep = CuentasPendientes?.FirstOrDefault(c => c.idCuenta == idSesion);
                if (keep != null)
                {
                    // ✅ TAMBIÉN MARCAR VISUALMENTE
                    foreach (var c in CuentasPendientes)
                    {
                        c.IsSelected = false;
                    }
                    keep.IsSelected = true;
                    CuentaSeleccionada = keep;
                    return;
                }

                // Recuperar desde BD si no está presente en la lista
                var cuenta = await _cuentaRepository.GetCuentaByIdAsync(idSesion);
                if (cuenta != null)
                {
                    var cuentaPendiente = new CuentaPendiente
                    {
                        idCuenta = cuenta.idCuenta,
                        NombreCliente = $"{cuenta.idClienteNavigation?.nombre} {cuenta.idClienteNavigation?.apellidos}",
                        DocumentoCliente = cuenta.idClienteNavigation?.numero_documento ?? "",
                        HoraIngreso = cuenta.fechaHoraCreacion.ToString("HH:mm"),
                        FechaHoraIngreso = cuenta.fechaHoraCreacion,
                        precioEntrada = cuenta.precioEntrada,
                        descuento = cuenta.descuento,
                        total = cuenta.total,
                        EstadoCuenta = cuenta.idEstadoCuentaNavigation?.nombre ?? ""
                    };
                    cuentaPendiente.ActualizarTiempo();
                    cuentaPendiente.IsSelected = true;
                    CuentaSeleccionada = cuentaPendiente;
                }
            }
        }

        private async Task BuscarClienteAsync()
        {
            if (string.IsNullOrWhiteSpace(DniBusqueda))
            {
                MessageBox.Show("Por favor, ingrese un DNI para buscar.",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (DniBusqueda.Length != 8)
            {
                MessageBox.Show("El DNI debe tener 8 dígitos.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                EstaCargando = true;

                using var context = new SaunaDbContext();
                var clienteRepo = new ClienteRepository(context);

                var cliente = await clienteRepo.GetByDNIAsync(DniBusqueda.Trim());

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (cliente != null)
                    {
                        if (cliente.activo)
                        {
                            IdClienteEncontrado = cliente.idCliente;
                            NombreClienteBuscado = $"{cliente.nombre} {cliente.apellidos}";
                            ClienteEncontrado = true;

                            // 🎁 CARGAR INFORMACIÓN DE PROMOCIONES
                            _ = CargarInfoPromocionesAsync(cliente.idCliente);

                            MessageBox.Show(
                                $"✅ Cliente encontrado:\n\n" +
                                $"Nombre: {cliente.nombre} {cliente.apellidos}\n" +
                                $"DNI: {cliente.numero_documento}\n" +
                                $"Teléfono: {cliente.telefono ?? "No registrado"}",
                                "Cliente Encontrado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        else
                        {
                            ClienteEncontrado = false;
                            MessageBox.Show(
                                "El cliente está desactivado en el sistema.\n\n" +
                                "Por favor, contacte al administrador para reactivarlo.",
                                "Cliente Inactivo",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        ClienteEncontrado = false;

                        var resultado = MessageBox.Show(
                            $"No se encontró ningún cliente con el DNI: {DniBusqueda}\n\n" +
                            "¿Desea registrar un nuevo cliente?",
                            "Cliente No Encontrado",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (resultado == MessageBoxResult.Yes)
                        {
                            MessageBox.Show(
                                "Función de registro rápido en desarrollo.\n\n" +
                                "Por favor, registre al cliente desde el módulo 'Clientes'.",
                                "Información",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                ClienteEncontrado = false;
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(
                        $"Error al buscar cliente:\n\n{ex.Message}\n\n" +
                        $"Detalle: {ex.InnerException?.Message ?? "No disponible"}",
                        "Error de Base de Datos",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task CrearCuentaAsync()
        {
            if (!ClienteEncontrado)
            {
                MessageBox.Show("Primero debe buscar y seleccionar un cliente.",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if (!SesionActual.EstaLogueado || SesionActual.IdUsuario <= 0)
            {
                MessageBox.Show("Sesión no válida. Inicie sesión nuevamente.",
                    "Sesión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // 🚫 VALIDACIÓN ESTRICTA: NO PERMITIR MÚLTIPLES CUENTAS PENDIENTES
            var cuentaExistente = CuentasPendientes.FirstOrDefault(c =>
                c.DocumentoCliente == DniBusqueda && c.EstadoCuenta == "Pendiente");

            if (cuentaExistente != null)
            {
                MessageBox.Show(
                    $"❌ OPERACIÓN NO PERMITIDA\n\n" +
                    $"El cliente '{NombreClienteBuscado}' ya tiene una cuenta pendiente.\n\n" +
                    $"📋 Detalles de la cuenta existente:\n" +
                    $"• ID Cuenta: {cuentaExistente.idCuenta}\n" +
                    $"• Fecha: {cuentaExistente.FechaHoraIngreso:dd/MM/yyyy HH:mm}\n" +
                    $"• Total: S/ {cuentaExistente.total:N2}\n\n" +
                    $"💡 Debe cerrar o cancelar la cuenta existente antes de crear una nueva.",
                    "Cuenta Pendiente Detectada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                EstaCargando = true;

                var nuevaCuenta = new Cuenta
                {
                    idCliente = IdClienteEncontrado,
                    fechaHoraCreacion = DateTime.Now,
                    precioEntrada = 0.00m,
                    descuento = 0.00m,
                    total = 0.00m,
                    saldo = 0.00m,
                    idEstadoCuenta = 1,
                    idUsuarioCreador = SesionActual.IdUsuario
                };

                var idNuevaCuenta = await _cuentaRepository.CrearCuentaAsync(nuevaCuenta);

                if (idNuevaCuenta > 0)
                {
                    MessageBox.Show(
                        $"✅ Cuenta creada exitosamente\n\n" +
                        $"Cliente: {NombreClienteBuscado}\n" +
                        $"ID Cuenta: {idNuevaCuenta}\n\n" +
                        $"Diríjase a la pestaña 'Gestión de Consumos' para agregar servicios.",
                        "Cuenta Creada",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    await CargarCuentasPendientesAsync();
                    await LimpiarBusquedaAsync();
                }
                else
                {
                    MessageBox.Show("No se pudo crear la cuenta. Intente nuevamente.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear cuenta: {ex.Message}\n\nDetalle: {ex.InnerException?.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private Task LimpiarBusquedaAsync()
        {
            DniBusqueda = string.Empty;
            ClienteEncontrado = false;
            NombreClienteBuscado = string.Empty;
            IdClienteEncontrado = 0;
            
            // 🎁 LIMPIAR INFORMACIÓN DE PROMOCIONES
            InfoDescuentos = new Services.InfoDescuentosCliente
            {
                TieneDescuentos = false,
                Mensaje = "Busque un cliente para ver promociones disponibles"
            };
            
            return Task.CompletedTask;
        }

        // 🎁 NUEVO MÉTODO: Cargar información de promociones del cliente
        private async Task CargarInfoPromocionesAsync(int idCliente)
        {
            try
            {
                // 🔄 FORZAR RECARGA DE DATOS DEL CLIENTE DESDE BD
                System.Diagnostics.Debug.WriteLine($"🔄 Recargando datos de promociones para cliente ID: {idCliente}");
                
                InfoDescuentos = await _descuentoService.ObtenerInfoDescuentosClienteAsync(idCliente);
                
                // 🐛 DEBUG: Verificar datos cargados
                System.Diagnostics.Debug.WriteLine($"✅ Promociones cargadas - Cliente: {InfoDescuentos.NombreCliente}, Visitas: {InfoDescuentos.VisitasTotales}, Descuentos: {InfoDescuentos.TieneDescuentos}");
                
                if (InfoDescuentos.TieneDescuentos)
                {
                    foreach (var descuento in InfoDescuentos.DescuentosDisponibles)
                    {
                        System.Diagnostics.Debug.WriteLine($"   💰 {descuento}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar promociones: {ex.Message}");
                InfoDescuentos = new Services.InfoDescuentosCliente
                {
                    TieneDescuentos = false,
                    Mensaje = $"Error al cargar promociones: {ex.Message}"
                };
            }
        }

        // 🔍 NUEVO MÉTODO: Filtrar cuentas pendientes
        private void FiltrarCuentasPendientes()
        {
            if (string.IsNullOrWhiteSpace(FiltroCuentas))
            {
                // Si no hay filtro, mostrar todas
                CuentasPendientes.Clear();
                foreach (var cuenta in TodasLasCuentas)
                {
                    CuentasPendientes.Add(cuenta);
                }
            }
            else
            {
                // Aplicar filtro
                var filtro = FiltroCuentas.ToLower().Trim();
                var cuentasFiltradas = TodasLasCuentas.Where(c =>
                    c.NombreCliente.ToLower().Contains(filtro) ||
                    c.DocumentoCliente.Contains(filtro) ||
                    c.idCuenta.ToString().Contains(filtro)
                ).ToList();

                CuentasPendientes.Clear();
                foreach (var cuenta in cuentasFiltradas)
                {
                    CuentasPendientes.Add(cuenta);
                }
            }
        }

        // 🔍 NUEVO MÉTODO: Limpiar filtro
        private void LimpiarFiltroCuentas()
        {
            FiltroCuentas = string.Empty;
        }

        private Task NavegarAPagosAsync()
        {
            try
            {
                if (CuentaSeleccionada == null)
                {
                    MessageBox.Show("Debe seleccionar una cuenta para proceder al pago.",
                        "Información",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return Task.CompletedTask;
                }

                // 🐛 DEBUG: Verificar datos antes de pasar a pagos
                System.Diagnostics.Debug.WriteLine($"🔍 NAVEGANDO A PAGOS - Datos de cuenta:");
                System.Diagnostics.Debug.WriteLine($"   ID Cuenta: {CuentaSeleccionada.idCuenta}");
                System.Diagnostics.Debug.WriteLine($"   Cliente: {CuentaSeleccionada.NombreCliente}");
                System.Diagnostics.Debug.WriteLine($"   Total con descuentos: S/ {CuentaSeleccionada.total:N2}");
                System.Diagnostics.Debug.WriteLine($"   Descuento aplicado: S/ {CuentaSeleccionada.descuento:N2}");

                // Pasar datos de la cuenta seleccionada al módulo de pagos
                Application.Current.Properties["IdCuenta"] = CuentaSeleccionada.idCuenta;
                Application.Current.Properties["NombreCliente"] = CuentaSeleccionada.NombreCliente;
                Application.Current.Properties["DocumentoCliente"] = CuentaSeleccionada.DocumentoCliente;
                Application.Current.Properties["TotalCuenta"] = CuentaSeleccionada.total; // ✅ TOTAL YA INCLUYE DESCUENTOS
                Application.Current.Properties["DescuentoAplicado"] = CuentaSeleccionada.descuento; // ➕ INFORMACIÓN ADICIONAL

                // 🔧 NAVEGACIÓN DIRECTA AL MÓDULO DE PAGOS
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    // Ya estamos en el hilo UI
                    CambiarAModuloPagosDirecto();
                }
                else
                {
                    // Necesitamos cambiar al hilo UI
                    Application.Current.Dispatcher.Invoke(CambiarAModuloPagosDirecto);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR general en NavegarAPagosAsync: {ex.Message}");
                MessageBox.Show($"Error al procesar la navegación: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            return Task.CompletedTask;
        }

        private void CambiarAModuloPagosDirecto()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Intentando cambiar directamente al módulo de pagos...");
                
                // 🔧 BUSCAR EN TODAS LAS VENTANAS ABIERTAS
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is MainWindow mainWin)
                    {
                        System.Diagnostics.Debug.WriteLine("✅ MainWindow encontrada, ejecutando navegación...");
                        
                        // Simular el cambio usando la misma lógica que el sidebar
                        mainWin.TituloModulo.Text = "Panel de Control - Pagos y Comprobantes";
                        mainWin.PantallaBienvenida.Visibility = Visibility.Collapsed;
                        mainWin.ContenidoPrincipal.Content = new UserControlPago();
                        
                        System.Diagnostics.Debug.WriteLine("✅ Navegación completada exitosamente");
                        return;
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("❌ ERROR: No se encontró MainWindow");
                throw new InvalidOperationException("No se pudo encontrar la ventana principal");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR en CambiarAModuloPagosDirecto: {ex.Message}");
                throw; // Re-lanzar para que sea capturado por el método padre
            }
        }

        private void ActualizarTiempos()
        {
            foreach (var cuenta in CuentasPendientes)
                cuenta.ActualizarTiempo();
        }
        #endregion

        #region Panel Modificar Cliente
        private Task AbrirPanelModificarCliente()
        {
            if (CuentaSeleccionada == null)
            {
                MessageBox.Show("Debe seleccionar una cuenta para modificar.",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return Task.CompletedTask;
            }

            NuevoDniModificar = string.Empty;
            MostrarPanelModificar = true;
            return Task.CompletedTask;
        }

        private Task CerrarPanelModificarCliente()
        {
            MostrarPanelModificar = false;
            NuevoDniModificar = string.Empty;
            return Task.CompletedTask;
        }

        private async Task ConfirmarModificarClienteAsync()
        {
            if (string.IsNullOrWhiteSpace(NuevoDniModificar) || NuevoDniModificar.Length != 8)
            {
                MessageBox.Show("Debe ingresar un DNI válido de 8 dígitos.",
                    "Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                EstaCargando = true;

                using var context = new SaunaDbContext();
                var clienteRepo = new ClienteRepository(context);

                var nuevoCliente = await clienteRepo.GetByDNIAsync(NuevoDniModificar.Trim());

                if (nuevoCliente == null)
                {
                    MessageBox.Show($"No se encontró ningún cliente con el DNI: {NuevoDniModificar}",
                        "Cliente No Encontrado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (!nuevoCliente.activo)
                {
                    MessageBox.Show("El cliente está desactivado en el sistema.",
                        "Cliente Inactivo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var cuenta = await _cuentaRepository.GetCuentaByIdAsync(CuentaSeleccionada.idCuenta);

                if (cuenta == null)
                {
                    MessageBox.Show("No se pudo obtener la información de la cuenta.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                cuenta.idCliente = nuevoCliente.idCliente;
                await _cuentaRepository.ActualizarCuentaAsync(cuenta);

                MessageBox.Show(
                    $"✅ Cliente modificado exitosamente\n\n" +
                    $"Nuevo cliente: {nuevoCliente.nombre} {nuevoCliente.apellidos}\n" +
                    $"DNI: {nuevoCliente.numero_documento}",
                    "Modificación Exitosa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await CerrarPanelModificarCliente();
                await CargarCuentasPendientesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al modificar cliente: {ex.Message}\n\nDetalle: {ex.InnerException?.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task EliminarCuentaAsync()
        {
            if (CuentaSeleccionada == null)
            {
                MessageBox.Show("Debe seleccionar una cuenta para eliminar.",
                    "Información",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var resultado = MessageBox.Show(
                $"⚠️ ¿Está seguro de eliminar la cuenta?\n\n" +
                $"Cliente: {CuentaSeleccionada.NombreCliente}\n" +
                $"ID Cuenta: {CuentaSeleccionada.idCuenta}\n\n" +
                $"Esta acción devolverá el stock de todos los productos consumidos y no se puede deshacer.",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes)
                return;

            try
            {
                EstaCargando = true;

                using var context = new SaunaDbContext();
                var repoConsumo = new DetalleConsumoRepository(context);
                var repoServicio = new DetalleServicioRepository(context);
                var productoRepo = new ProductoRepository(context);
                var movimientoRepo = new MovimientoInventarioRepository(context);

                var consumos = await repoConsumo.GetByCuentaAsync(CuentaSeleccionada.idCuenta);

                foreach (var consumo in consumos)
                {
                    var producto = await productoRepo.GetByIdAsync(consumo.idProducto);
                    if (producto != null)
                    {
                        producto.stockActual += consumo.cantidad;
                        await productoRepo.UpdateAsync(producto);

                        await RegistrarMovimientoAsync(
                            productoRepo: productoRepo,
                            movimientoRepo: movimientoRepo,
                            idProducto: consumo.idProducto,
                            cantidad: consumo.cantidad,
                            esEntrada: true,
                            observacion: $"Devolución - Cuenta #{CuentaSeleccionada.idCuenta} eliminada"
                        );
                    }
                }

                foreach (var consumo in consumos)
                {
                    await repoConsumo.DeleteAsync(consumo.idDetalle);
                }

                var servicios = await repoServicio.GetByCuentaAsync(CuentaSeleccionada.idCuenta);
                foreach (var servicio in servicios)
                {
                    await repoServicio.DeleteAsync(servicio.idDetalleServicio);
                }

                await _cuentaRepository.DeleteAsync(CuentaSeleccionada.idCuenta);

                MessageBox.Show(
                    "✅ Cuenta eliminada exitosamente.\n\n" +
                    "El stock de los productos ha sido devuelto al inventario.",
                    "Eliminación Exitosa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await CargarCuentasPendientesAsync();
                await CargarProductosAsync();

                InventoryEventService.NotifyStockChanged();

                CuentaSeleccionada = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar cuenta: {ex.Message}\n\nDetalle: {ex.InnerException?.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                EstaCargando = false;
            }
        }
        #endregion

        #region Métodos de Productos y Servicios
        private async Task CargarProductosAsync()
        {
            try
            {
                using var context = new SaunaDbContext();
                var repo = new ProductoRepository(context);
                var productos = await repo.GetAllAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ProductosDisponibles.Clear();
                    foreach (var p in productos.Where(p => p.activo))
                    {
                        ProductosDisponibles.Add(p);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CargarServiciosAsync()
        {
            try
            {
                using var context = new SaunaDbContext();
                var repo = new ServicioRepository(context);
                var servicios = await repo.GetAllAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ServiciosDisponibles.Clear();
                    foreach (var s in servicios.Where(s => s.activo))
                    {
                        ServiciosDisponibles.Add(s);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar servicios: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task BuscarProductosAsync()
        {
            if (string.IsNullOrWhiteSpace(BusquedaProducto))
            {
                await CargarProductosAsync();
                return;
            }

            try
            {
                using var context = new SaunaDbContext();
                var repo = new ProductoRepository(context);
                var productos = await repo.BuscarPorNombreAsync(BusquedaProducto);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ProductosDisponibles.Clear();
                    foreach (var p in productos.Where(p => p.activo))
                    {
                        ProductosDisponibles.Add(p);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar productos: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task BuscarServiciosAsync()
        {
            if (string.IsNullOrWhiteSpace(BusquedaServicio))
            {
                await CargarServiciosAsync();
                return;
            }

            try
            {
                using var context = new SaunaDbContext();
                var repo = new ServicioRepository(context);
                var servicios = await repo.BuscarPorNombreAsync(BusquedaServicio);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ServiciosDisponibles.Clear();
                    foreach (var s in servicios.Where(s => s.activo))
                    {
                        ServiciosDisponibles.Add(s);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar servicios: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AgregarProductoACuentaAsync()
        {
            await EnsureCuentaSeleccionadaAsync();
            if (CuentaSeleccionada == null)
            {
                MessageBox.Show("Debe seleccionar una cuenta primero.", "Información",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ProductoSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un producto.", "Información",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (CantidadProducto <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a 0.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ProductoSeleccionado.stockActual <= 0)
            {
                MessageBox.Show($"El producto '{ProductoSeleccionado.nombre}' no tiene stock disponible.",
                    "Sin Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ProductoSeleccionado.stockActual < CantidadProducto)
            {
                MessageBox.Show($"Stock insuficiente.\n\nDisponible: {ProductoSeleccionado.stockActual}\nSolicitado: {CantidadProducto}",
                    "Stock Insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                EstaCargando = true;

                using var context = new SaunaDbContext();
                var repo = new DetalleConsumoRepository(context);
                var productoRepo = new ProductoRepository(context);
                var movimientoRepo = new MovimientoInventarioRepository(context);

                var detalle = new DetalleConsumo
                {
                    idCuenta = CuentaSeleccionada.idCuenta,
                    idProducto = ProductoSeleccionado.idProducto,
                    cantidad = CantidadProducto,
                    precioUnitario = ProductoSeleccionado.precioVenta,
                    subtotal = ProductoSeleccionado.precioVenta * CantidadProducto
                };

                await repo.AddAsync(detalle);

                var productoActualizado = await productoRepo.GetByIdAsync(ProductoSeleccionado.idProducto);
                var stockAntes = productoActualizado.stockActual;
                productoActualizado.stockActual -= CantidadProducto;
                await productoRepo.UpdateAsync(productoActualizado);

                await RegistrarMovimientoAsync(
                    productoRepo: productoRepo,
                    movimientoRepo: movimientoRepo,
                    idProducto: ProductoSeleccionado.idProducto,
                    cantidad: CantidadProducto,
                    esEntrada: false,
                    observacion: $"Venta - Cuenta #{CuentaSeleccionada.idCuenta} ({CuentaSeleccionada.NombreCliente})"
                );

                ProyectoSauna.Services.AuditLogger.LogInventario(
                    "Salida",
                    productoActualizado,
                    stockAntes,
                    productoActualizado.stockActual,
                    SesionActual.IdUsuario > 0 ? SesionActual.IdUsuario : 1,
                    $"Venta - Cuenta #{CuentaSeleccionada.idCuenta} ({CuentaSeleccionada.NombreCliente})"
                );

                await ActualizarTotalCuentaEnBDAsync(CuentaSeleccionada.idCuenta);

                // ✅ MANTENER REFERENCIA ANTES DE RECARGAR
                var idCuentaActual = CuentaSeleccionada.idCuenta;

                await CargarConsumosDeCuentaAsync(idCuentaActual);
                await CargarProductosAsync();

                // ✅ Solo actualizar totales, no recargar lista completa para preservar selección

                InventoryEventService.NotifyStockChanged();

                CantidadProducto = 1;
                ProductoSeleccionado = null;

                MessageBox.Show("✅ Producto agregado y stock actualizado.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar producto: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task AgregarServicioACuentaAsync()
        {
            await EnsureCuentaSeleccionadaAsync();
            if (CuentaSeleccionada == null)
            {
                MessageBox.Show("Debe seleccionar una cuenta primero.", "Información",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ServicioSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un servicio.", "Información",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (CantidadServicio <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a 0.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                EstaCargando = true;

                using var context = new SaunaDbContext();
                var repo = new DetalleServicioRepository(context);

                var detalle = new DetalleServicio
                {
                    idCuenta = CuentaSeleccionada.idCuenta,
                    idServicio = ServicioSeleccionado.idServicio,
                    cantidad = CantidadServicio,
                    precioUnitario = ServicioSeleccionado.precio,
                    subtotal = ServicioSeleccionado.precio * CantidadServicio
                };

                await repo.AddAsync(detalle);

                await ActualizarTotalCuentaEnBDAsync(CuentaSeleccionada.idCuenta);

                // ✅ MANTENER REFERENCIA ANTES DE RECARGAR
                var idCuentaActual = CuentaSeleccionada.idCuenta;

                await CargarConsumosDeCuentaAsync(idCuentaActual);

                // ✅ Solo actualizar totales, no recargar lista completa para preservar selección

                CantidadServicio = 1;
                ServicioSeleccionado = null;
                ObservacionesServicio = string.Empty;

                MessageBox.Show("✅ Servicio agregado correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar servicio: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task CargarConsumosDeCuentaAsync(int idCuenta)
        {
            try
            {
                using var context = new SaunaDbContext();
                var repoConsumo = new DetalleConsumoRepository(context);
                var repoServicio = new DetalleServicioRepository(context);

                var consumos = await repoConsumo.GetByCuentaAsync(idCuenta);
                var servicios = await repoServicio.GetByCuentaAsync(idCuenta);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ConsumosCuentaActual.Clear();

                    decimal totalProds = 0;
                    int countProds = 0;

                    foreach (var c in consumos)
                    {
                        ConsumosCuentaActual.Add(new ConsumoItem
                        {
                            IdDetalle = c.idDetalle,
                            Tipo = "PROD",
                            NombreItem = c.idProductoNavigation?.nombre ?? "Producto",
                            cantidad = c.cantidad,
                            precioUnitario = c.precioUnitario,
                            subtotal = c.subtotal,
                            IdReferencia = c.idProducto,
                            IdCuenta = c.idCuenta
                        });
                        totalProds += c.subtotal;
                        countProds++;
                    }

                    decimal totalServs = 0;
                    int countServs = 0;

                    foreach (var s in servicios)
                    {
                        ConsumosCuentaActual.Add(new ConsumoItem
                        {
                            IdDetalle = s.idDetalleServicio,
                            Tipo = "SERV",
                            NombreItem = s.idServicioNavigation?.nombre ?? "Servicio",
                            cantidad = s.cantidad,
                            precioUnitario = s.precioUnitario,
                            subtotal = s.subtotal,
                            IdReferencia = s.idServicio,
                            IdCuenta = s.idCuenta
                        });
                        totalServs += s.subtotal;
                        countServs++;
                    }

                    TotalProductos = totalProds;
                    TotalServicios = totalServs;
                    CantidadProductos = countProds;
                    CantidadServicios = countServs;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar consumos: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task EliminarConsumoAsync(ConsumoItem item)
        {
            if (item == null) return;

            var resultado = MessageBox.Show(
                $"¿Desea eliminar COMPLETAMENTE este consumo?\n\n{item.NombreItem} x{item.cantidad}\n\nSi solo desea devolver algunas unidades, use el botón 'DEVOLVER'.",
                "Confirmar eliminación completa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes) return;

            try
            {
                EstaCargando = true;

                using var context = new SaunaDbContext();

                if (item.Tipo == "PROD")
                {
                    var repo = new DetalleConsumoRepository(context);
                    var productoRepo = new ProductoRepository(context);
                    var movimientoRepo = new MovimientoInventarioRepository(context);

                    await repo.DeleteAsync(item.IdDetalle);

                    var producto = await productoRepo.GetByIdAsync(item.IdReferencia);
                    if (producto != null)
                    {
                        producto.stockActual += item.cantidad;
                        await productoRepo.UpdateAsync(producto);

                        await RegistrarMovimientoAsync(
                            productoRepo: productoRepo,
                            movimientoRepo: movimientoRepo,
                            idProducto: item.IdReferencia,
                            cantidad: item.cantidad,
                            esEntrada: true,
                            observacion: $"Devolución completa - Cuenta #{CuentaSeleccionada.idCuenta}"
                        );
                    }

                    await CargarProductosAsync();

                    InventoryEventService.NotifyStockChanged();
                }
                else
                {
                    var repo = new DetalleServicioRepository(context);
                    await repo.DeleteAsync(item.IdDetalle);
                }

                await ActualizarTotalCuentaEnBDAsync(CuentaSeleccionada.idCuenta);

                // ✅ MANTENER REFERENCIA
                var idCuentaActual = CuentaSeleccionada.idCuenta;

                await CargarConsumosDeCuentaAsync(idCuentaActual);
                await ActualizarTotalEnListaAsync(idCuentaActual);

                MessageBox.Show("✅ Consumo eliminado correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar consumo: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task DevolverProductoAsync()
        {
            if (ConsumoSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un consumo de la lista.", "Información",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ConsumoSeleccionado.Tipo != "PROD")
            {
                MessageBox.Show("Solo se pueden devolver productos, no servicios.", "Información",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (CantidadADevolver <= 0)
            {
                MessageBox.Show("La cantidad a devolver debe ser mayor a 0.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CantidadADevolver > ConsumoSeleccionado.cantidad)
            {
                MessageBox.Show($"No puede devolver más de {ConsumoSeleccionado.cantidad} unidades.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var mensaje = CantidadADevolver == ConsumoSeleccionado.cantidad
                ? $"¿Confirma devolver TODAS las unidades?\n\n{ConsumoSeleccionado.NombreItem}\nCantidad: {CantidadADevolver}\n\nEl consumo se eliminará completamente."
                : $"¿Confirma la devolución parcial?\n\n{ConsumoSeleccionado.NombreItem}\nDevolver: {CantidadADevolver} de {ConsumoSeleccionado.cantidad}\n\nQuedarán: {ConsumoSeleccionado.cantidad - CantidadADevolver} unidades";

            var resultado = MessageBox.Show(mensaje, "Confirmar Devolución",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes) return;

            try
            {
                EstaCargando = true;

                using var context = new SaunaDbContext();
                var repo = new DetalleConsumoRepository(context);
                var productoRepo = new ProductoRepository(context);
                var movimientoRepo = new MovimientoInventarioRepository(context);

                var detalle = await repo.GetByIdAsync(ConsumoSeleccionado.IdDetalle);
                if (detalle == null)
                {
                    MessageBox.Show("No se encontró el detalle del consumo.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var producto = await productoRepo.GetByIdAsync(ConsumoSeleccionado.IdReferencia);
                if (producto != null)
                {
                    var stockAntes = producto.stockActual;
                    producto.stockActual += CantidadADevolver;
                    await productoRepo.UpdateAsync(producto);

                    var tipoDevolucion = CantidadADevolver == ConsumoSeleccionado.cantidad ? "completa" : "parcial";
                    await RegistrarMovimientoAsync(
                        productoRepo: productoRepo,
                        movimientoRepo: movimientoRepo,
                        idProducto: ConsumoSeleccionado.IdReferencia,
                        cantidad: CantidadADevolver,
                        esEntrada: true,
                        observacion: $"Devolución {tipoDevolucion} - Cuenta #{CuentaSeleccionada.idCuenta}"
                    );

                    ProyectoSauna.Services.AuditLogger.LogInventario(
                        "Entrada",
                        producto,
                        stockAntes,
                        producto.stockActual,
                        SesionActual.IdUsuario > 0 ? SesionActual.IdUsuario : 1,
                        $"Devolución {tipoDevolucion} - Cuenta #{CuentaSeleccionada.idCuenta}"
                    );
                }

                if (CantidadADevolver == ConsumoSeleccionado.cantidad)
                {
                    await repo.DeleteAsync(ConsumoSeleccionado.IdDetalle);
                }
                else
                {
                    detalle.cantidad -= CantidadADevolver;
                    detalle.subtotal = detalle.cantidad * detalle.precioUnitario;
                    await repo.UpdateAsync(detalle);
                }

                await ActualizarTotalCuentaEnBDAsync(CuentaSeleccionada.idCuenta);

                // ✅ MANTENER REFERENCIA
                var idCuentaActual = CuentaSeleccionada.idCuenta;

                await CargarConsumosDeCuentaAsync(idCuentaActual);
                await CargarProductosAsync();
                await ActualizarTotalEnListaAsync(idCuentaActual);

                InventoryEventService.NotifyStockChanged();

                ConsumoSeleccionado = null;
                CantidadADevolver = 1;

                MessageBox.Show($"✅ Se devolvieron {CantidadADevolver} unidades correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar devolución: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                EstaCargando = false;
            }
        }

        private async Task RegistrarMovimientoAsync(
            IProductoRepository productoRepo,
            IMovimientoInventarioRepository movimientoRepo,
            int idProducto,
            int cantidad,
            bool esEntrada,
            string observacion)
        {
            try
            {
                var producto = await productoRepo.GetByIdAsync(idProducto);
                if (producto == null) return;

                using var context = new SaunaDbContext();
                var tipoMovRepo = new TipoMovimientoRepository(context);

                int? idTipoMovimiento = null;

                if (esEntrada)
                {
                    var tipos = await tipoMovRepo.GetByTipoAsync("Entrada");
                    idTipoMovimiento = tipos.FirstOrDefault()?.idTipoMovimiento;

                    if (idTipoMovimiento == null)
                    {
                        tipos = await tipoMovRepo.GetByTipoAsync("Devolución");
                        idTipoMovimiento = tipos.FirstOrDefault()?.idTipoMovimiento;
                    }
                }
                else
                {
                    var tipos = await tipoMovRepo.GetByTipoAsync("Salida");
                    idTipoMovimiento = tipos.FirstOrDefault()?.idTipoMovimiento;
                }

                if (idTipoMovimiento == null) return;

                var movimiento = new MovimientoInventario
                {
                    idProducto = idProducto,
                    cantidad = cantidad,
                    costoUnitario = producto.precioCompra,
                    costoTotal = producto.precioCompra * cantidad,
                    fecha = DateTime.Now,
                    observaciones = observacion,
                    idTipoMovimiento = idTipoMovimiento.Value,
                    idUsuario = SesionActual.IdUsuario > 0 ? SesionActual.IdUsuario : 1
                };

                await movimientoRepo.AddAsync(movimiento);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al registrar movimiento: {ex.Message}");
            }
        }

        private void CalcularTotalCuenta()
        {
            if (CuentaSeleccionada == null)
            {
                TotalCuenta = 0;
                return;
            }

            // ✅ El total ya incluye el descuento calculado automáticamente
            TotalCuenta = CuentaSeleccionada.precioEntrada - CuentaSeleccionada.descuento + TotalProductos + TotalServicios;
        }

        private void ActualizarTotalItems()
        {
            TotalItemsCuenta = CantidadProductos + CantidadServicios;
        }

        private async Task ActualizarTotalCuentaEnBDAsync(int idCuenta)
        {
            try
            {
                using var context = new SaunaDbContext();

                var totalProductos = await context.DetalleConsumo
                    .Where(dc => dc.idCuenta == idCuenta)
                    .SumAsync(dc => (decimal?)dc.subtotal) ?? 0;

                var totalServicios = await context.DetalleServicio
                    .Where(ds => ds.idCuenta == idCuenta)
                    .SumAsync(ds => (decimal?)ds.subtotal) ?? 0;

                decimal subtotalConsumos = totalProductos + totalServicios;
                decimal montoBase = subtotalConsumos; // Subtotal de consumos para calcular descuento

                var cuenta = await context.Cuenta.FindAsync(idCuenta);

                if (cuenta != null)
                {
                    // 🎁 CALCULAR DESCUENTOS AUTOMÁTICAMENTE
                    decimal descuentoCalculado = 0;
                    if (montoBase > 0 && cuenta.idCliente > 0)
                    {
                        try
                        {
                            var resultadoDescuento = await _descuentoService.CalcularDescuentosDisponiblesAsync(cuenta.idCliente, montoBase);
                            descuentoCalculado = resultadoDescuento.TotalDescuento;
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error al calcular descuentos: {ex.Message}");
                        }
                    }

                    cuenta.subtotalConsumos = subtotalConsumos;
                    cuenta.descuento = descuentoCalculado; // ✅ Actualizar descuento automáticamente
                    cuenta.total = cuenta.precioEntrada + subtotalConsumos - cuenta.descuento;

                    context.Entry(cuenta).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    await context.SaveChangesAsync();

                    // 🔄 ACTUALIZAR CUENTA SELECCIONADA EN TIEMPO REAL
                    if (CuentaSeleccionada != null && CuentaSeleccionada.idCuenta == idCuenta)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            CuentaSeleccionada.descuento = descuentoCalculado;
                            CuentaSeleccionada.total = cuenta.total;
                            CalcularTotalCuenta(); // 🎯 FORZAR RECÁLCULO DE TOTAL PARA UI
                        });
                    }

                    // 🔄 ACTUALIZAR LISTA DE CUENTAS PENDIENTES EN TIEMPO REAL
                    await ActualizarTotalEnListaAsync(idCuenta);
                    
                    // ✅ NO RECARGAR LISTA COMPLETA PARA PRESERVAR SELECCIÓN
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al actualizar total: {ex.Message}");
            }
        }

        // ✅ NUEVO MÉTODO: Actualizar solo el total en la lista sin recargar todo
        private async Task ActualizarTotalEnListaAsync(int idCuenta)
        {
            try
            {
                using var context = new SaunaDbContext();
                var cuentaDB = await context.Cuenta
                    .Include(c => c.idClienteNavigation)
                    .Include(c => c.idEstadoCuentaNavigation)
                    .FirstOrDefaultAsync(c => c.idCuenta == idCuenta);

                if (cuentaDB != null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        // 🔄 ACTUALIZAR EN LA LISTA
                        var cuentaEnLista = CuentasPendientes.FirstOrDefault(c => c.idCuenta == idCuenta);
                        if (cuentaEnLista != null)
                        {
                            cuentaEnLista.total = cuentaDB.total;
                            cuentaEnLista.descuento = cuentaDB.descuento;
                        }

                        // 🎯 ACTUALIZAR CUENTA SELECCIONADA SI ES LA MISMA
                        if (CuentaSeleccionada != null && CuentaSeleccionada.idCuenta == idCuenta)
                        {
                            CuentaSeleccionada.total = cuentaDB.total;
                            CuentaSeleccionada.descuento = cuentaDB.descuento;
                            System.Diagnostics.Debug.WriteLine($"🎯 Cuenta seleccionada actualizada: #{CuentaSeleccionada.idCuenta} - Total: S/. {CuentaSeleccionada.total:N2}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al actualizar total en lista: {ex.Message}");
            }
        }
        /// <summary>
        /// Recarga la lista de cuentas pendientes manteniendo la cuenta seleccionada actualmente
        /// </summary>
        private async Task CargarCuentasPendientesPreservandoSeleccionAsync()
        {
            // 💾 GUARDAR SELECCIÓN ACTUAL
            var cuentaSeleccionadaId = CuentaSeleccionada?.idCuenta;
            
            // 🔄 RECARGAR LISTA
            await CargarCuentasPendientesAsync();
            
            // 🎯 RESTAURAR SELECCIÓN SI EXISTÍA
            if (cuentaSeleccionadaId.HasValue && CuentasPendientes != null)
            {
                var cuentaARestaurar = CuentasPendientes.FirstOrDefault(c => c.idCuenta == cuentaSeleccionadaId.Value);
                if (cuentaARestaurar != null)
                {
                    CuentaSeleccionada = cuentaARestaurar;
                    System.Diagnostics.Debug.WriteLine($"🎯 Selección restaurada: Cuenta #{cuentaARestaurar.idCuenta} - {cuentaARestaurar.NombreCliente}");
                }
            }
        }
        #endregion

        #region Comandos
        public ICommand ActualizarListaCommand { get; }
        public ICommand BuscarClienteCommand { get; }
        public ICommand CrearCuentaCommand { get; }
        public ICommand LimpiarBusquedaCommand { get; }
        public ICommand CerrarCuentaCommand { get; }
        public ICommand BuscarProductosCommand { get; }
        public ICommand BuscarServiciosCommand { get; }
        public ICommand AgregarProductoACuentaCommand { get; }
        public ICommand AgregarServicioACuentaCommand { get; }
        public ICommand EliminarConsumoCommand { get; }
        public ICommand EliminarCuentaCommand { get; }
        public ICommand AbrirModificarClienteCommand { get; }
        public ICommand CerrarModificarClienteCommand { get; }
        public ICommand ConfirmarModificarClienteCommand { get; }
        public ICommand DevolverProductoCommand { get; }
        public ICommand SeleccionarCuentaCommand { get; }
        public ICommand LimpiarCuentaActivaCommand { get; } // ✅ NUEVO
        public ICommand LimpiarFiltroCommand { get; } // 🔍 NUEVO COMANDO FILTRO
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    // ✅ MODIFICADO: Agregada propiedad IsSelected
    public class CuentaPendiente : INotifyPropertyChanged
    {
        public int idCuenta { get; set; }
        public string NombreCliente { get; set; }
        public string DocumentoCliente { get; set; }
        public string HoraIngreso { get; set; }
        public DateTime FechaHoraIngreso { get; set; }
        public decimal precioEntrada { get; set; }
        public decimal descuento { get; set; }
        public decimal total { get; set; }
        public string EstadoCuenta { get; set; }

        private string _tiempoTranscurrido;
        public string TiempoTranscurrido
        {
            get => _tiempoTranscurrido;
            set { _tiempoTranscurrido = value; OnPropertyChanged(); }
        }

        // ✅ NUEVA PROPIEDAD PARA RADIO BUTTON
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public void ActualizarTiempo()
        {
            var tiempo = DateTime.Now - FechaHoraIngreso;
            TiempoTranscurrido = $"{(int)tiempo.TotalHours:D2}:{tiempo.Minutes:D2}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public async void Execute(object parameter) => await _execute();
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Func<T, Task> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Func<T, Task> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;

        public async void Execute(object parameter) => await _execute((T)parameter);
    }
}