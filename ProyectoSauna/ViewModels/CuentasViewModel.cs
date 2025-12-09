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
    public class CuentasViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ICuentaRepository _cuentaRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IServicioRepository _servicioRepository;
        private readonly IDetalleConsumoRepository _detalleConsumoRepository;
        private readonly IDetalleServicioRepository _detalleServicioRepository;
        private readonly IMovimientoInventarioRepository _movimientoInventarioRepository;
        private readonly ITipoMovimientoRepository _tipoMovimientoRepository;
        private readonly Services.DescuentoService _descuentoService;
        
        // 🛡️ SERVICIOS DE SEGURIDAD Y CONCURRENCIA
        private readonly Services.CuentaValidacionService _validacionService;
        private readonly Services.ConcurrencyService? _concurrencyService;
        private readonly Services.CuentaUnicaService _cuentaUnicaService;
        private readonly Services.InventoryEventService _inventoryEventService;
        private readonly Services.CuentaEnEdicionService _cuentaEnEdicionService; // 🔒 NUEVO SERVICIO
        private readonly SaunaDbContext _sharedContext;
        
        // 🔄 CONTROL DE ACTUALIZACIÓN INTELIGENTE
        private bool _actualizacionHabilitada = true;
        private bool _hayPendienteActualizacion = false;
        
        // 🔒 CONTROL DE USUARIO ACTUAL PARA EVITAR MENSAJES INNECESARIOS
        private string _usuarioActual => string.IsNullOrEmpty(ProyectoSauna.Models.SesionActual.NombreCompleto) 
            ? Environment.UserName ?? "Usuario" 
            : ProyectoSauna.Models.SesionActual.NombreCompleto;
        
        private DispatcherTimer _timer;
        private DispatcherTimer _searchTimerProductos;
        private DispatcherTimer _searchTimerServicios;
        private DispatcherTimer _actualizacionTimer;
        private DispatcherTimer _verificacionBloqueoTimer; // 🔒 TIMER PARA VERIFICACIÓN DE BLOQUEOS // 🔄 Timer para actualizaciones inteligentes

        public CuentasViewModel()
        {
            _cuentaRepository = new CuentaRepository();

            _sharedContext = new SaunaDbContext();
            _productoRepository = new ProductoRepository(_sharedContext);
            _servicioRepository = new ServicioRepository(_sharedContext);
            _detalleConsumoRepository = new DetalleConsumoRepository(_sharedContext);
            _detalleServicioRepository = new DetalleServicioRepository(_sharedContext);
            _movimientoInventarioRepository = new MovimientoInventarioRepository(_sharedContext);
            _tipoMovimientoRepository = new TipoMovimientoRepository(_sharedContext);
            
            // 🛡️ INICIALIZAR SERVICIOS DE SEGURIDAD
            _validacionService = new Services.CuentaValidacionService();
            _concurrencyService = new Services.ConcurrencyService(_sharedContext);
            _cuentaUnicaService = new Services.CuentaUnicaService();
            _cuentaEnEdicionService = new Services.CuentaEnEdicionService(); // 🔒 INICIALIZAR SERVICIO DE EDICIÓN
            
            // Inicializar DescuentoService
            try
            {
                var promocionesRepo = App.AppHost?.Services.GetRequiredService<IPromocionesRepository>();
                var clienteRepo = App.AppHost?.Services.GetRequiredService<IClienteRepository>();
                if (promocionesRepo != null && clienteRepo != null)
                {
                    _descuentoService = new Services.DescuentoService(promocionesRepo, clienteRepo);
                }
                else
                {
                    // Fallback si no se pueden obtener los servicios
                    _descuentoService = null!;
                }
            }
            catch
            {
                _descuentoService = null!;
            }

            CuentasPendientes = new ObservableCollection<CuentaPendiente>();
            ProductosDisponibles = new ObservableCollection<Producto>();
            ServiciosDisponibles = new ObservableCollection<Servicio>();
            ConsumosCuentaActual = new ObservableCollection<ConsumoItem>();

            ActualizarListaCommand = new RelayCommand(async () => await ActualizarListaCuentasAsync());
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

            // ✅ COMANDO PARA SELECCIONAR CUENTA - PROTEGIDO CONTRA DOBLE EJECUCIÓN
            SeleccionarCuentaCommand = new Commands.AsyncRelayCommand((object? parameter) => 
                SeleccionarCuentaAsync(parameter as CuentaPendiente));

            // ✅ NUEVO COMANDO PARA LIMPIAR CUENTA ACTIVA
            LimpiarCuentaActivaCommand = new RelayCommand(async () => await LimpiarCuentaActiva());

            // 🔍 NUEVO COMANDO PARA LIMPIAR FILTRO
            LimpiarFiltroCommand = new RelayCommand(() => { LimpiarFiltroCuentas(); return Task.CompletedTask; });

            // 🔄 SUSCRIPCIÓN A EVENTOS DE SINCRONIZACIÓN ENTRE VENTANAS
            _inventoryEventService = InventoryEventService.Instance;
            _inventoryEventService.StockChanged += OnStockChanged_SincronizarCuentas;
            
            // 🧹 LIMPIAR BLOQUEOS FANTASMA AL INICIALIZAR
            LimpiarBloqueosFantasma();

            _ = CargarCuentasPendientesAsync();
            _ = CargarProductosAsync();
            _ = CargarServiciosAsync();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _timer.Tick += (s, e) => 
            {
                ActualizarTiempos();
                VerificarEstadoEdicionCuentas(); // 🔒 VERIFICAR ESTADO DE EDICIÓN
            };
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

            // 🔄 CONFIGURAR TIMER DE ACTUALIZACIÓN INTELIGENTE
            _actualizacionTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            _actualizacionTimer.Tick += async (s, e) => await VerificarActualizacionPendiente();
            _actualizacionTimer.Start();
            
            // 🔒 CONFIGURAR TIMER PARA VERIFICACIÓN DE BLOQUEOS EN TIEMPO REAL
            _verificacionBloqueoTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _verificacionBloqueoTimer.Tick += (s, e) => VerificarEstadoEdicionCuentas();
            _verificacionBloqueoTimer.Start();
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
        
        // 🎯 PROPIEDAD PARA MANEJAR SELECCIÓN DEL DATAGRID
        private CuentaPendiente _selectedDataGridItem;
        private bool _isUpdatingDataGridSelection = false;
        private bool _creandoCuentaNueva = false; // 🆕 FLAG PARA EVITAR VERIFICACION DURANTE CREACION
        
        public CuentaPendiente SelectedDataGridItem
        {
            get => _selectedDataGridItem;
            set
            {
                // 🛡️ VERIFICACIÓN PREVIA: Si es una cuenta diferente, verificar si está disponible
                // ⚠️ NO VERIFICAR SI ESTAMOS CREANDO CUENTA NUEVA (evita conflictos)
                if (value != null && value != CuentaSeleccionada && !_isUpdatingDataGridSelection && !_creandoCuentaNueva)
                {
                    var estadoBloqueo = _cuentaEnEdicionService.VerificarCuentaEnEdicion(value.idCuenta);
                    if (estadoBloqueo.enEdicion && !estadoBloqueo.usuarioEditor.Equals(_usuarioActual, StringComparison.OrdinalIgnoreCase))
                    {
                        // 🚫 CUENTA BLOQUEADA POR OTRO USUARIO - SOLO MOSTRAR MENSAJE
                        var mensaje = $"⚠️ La cuenta '{value.NombreCliente}' ya está siendo editada por {estadoBloqueo.usuarioEditor}.\n\n" +
                                      "No puedes seleccionar esta cuenta mientras esté en edición.";
                        
                        MessageBox.Show(mensaje, "🔒 Cuenta en Edición", MessageBoxButton.OK, MessageBoxImage.Warning);
                        
                        return; // NO CAMBIAR LA SELECCIÓN
                    }
                }
                
                if (_selectedDataGridItem != value && !_isUpdatingDataGridSelection)
                {
                    System.Diagnostics.Debug.WriteLine($"🎯 SelectedDataGridItem cambiado a: {value?.idCuenta} - {value?.NombreCliente}");
                    _selectedDataGridItem = value;
                    OnPropertyChanged();
                    
                    // 🔄 NO DISPARAR SI VIENE DESDE CÓDIGO
                    if (value != null && !_isUpdatingDataGridSelection)
                    {
                        _ = Task.Run(async () => await SeleccionarCuentaAsync(value));
                    }
                }
            }
        }
        
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
        
        /// <summary>
        /// 🧹 Limpia bloqueos fantasma que puedan estar activos sin cuentas realmente seleccionadas
        /// </summary>
        private void LimpiarBloqueosFantasma()
        {
            try
            {
                var usuarioActual = string.IsNullOrEmpty(ProyectoSauna.Models.SesionActual.NombreCompleto) 
                    ? Environment.UserName ?? "Usuario" 
                    : ProyectoSauna.Models.SesionActual.NombreCompleto;
                
                // 🔓 LIBERAR TODOS LOS BLOQUEOS DEL USUARIO ACTUAL AL INICIAR
                _cuentaEnEdicionService.LiberarTodosBloqueosUsuario(usuarioActual);
                
                System.Diagnostics.Debug.WriteLine($"🧹 Bloqueos fantasma limpiados para usuario: {usuarioActual}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error limpiando bloqueos fantasma: {ex.Message}");
            }
        }
        
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
                            EstadoCuenta = cuenta.idEstadoCuentaNavigation?.nombre ?? "",
                            ParentViewModel = this // 🔗 ASIGNAR REFERENCIA AL VIEWMODEL PADRE
                        };

                        cuentaPendiente.ActualizarTiempo();
                        
                        // 🔒 VERIFICAR ESTADO DE EDICIÓN INICIAL Y CONFIGURAR RADIOBUTTON
                        var estadoEdicion = _cuentaEnEdicionService.VerificarCuentaEnEdicion(cuenta.idCuenta);
                        cuentaPendiente.EstaSiendoEditada = estadoEdicion.enEdicion;
                        cuentaPendiente.UsuarioEditor = estadoEdicion.usuarioEditor ?? "";
                        
                        // 🎯 CONFIGURAR ESTADO DEL RADIOBUTTON
                        if (estadoEdicion.enEdicion && !estadoEdicion.usuarioEditor.Equals(_usuarioActual, StringComparison.OrdinalIgnoreCase))
                        {
                            // Cuenta bloqueada por otro usuario - RadioButton deshabilitado
                            cuentaPendiente.IsRadioButtonEnabled = false;
                            System.Diagnostics.Debug.WriteLine($"🔒 RadioButton deshabilitado para cuenta {cuenta.idCuenta} (editada por {estadoEdicion.usuarioEditor})");
                        }
                        else
                        {
                            // Cuenta disponible - RadioButton habilitado
                            cuentaPendiente.IsRadioButtonEnabled = true;
                        }
                        
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
                            // 🔒 INTENTAR BLOQUEAR LA CUENTA PARA RESTAURACIÓN
                            var usuarioActual = string.IsNullOrEmpty(ProyectoSauna.Models.SesionActual.NombreCompleto) 
                                ? Environment.UserName ?? "Usuario" 
                                : ProyectoSauna.Models.SesionActual.NombreCompleto;
                            var bloqueo = _cuentaEnEdicionService.IntentarBloquearCuenta(cuentaASeleccionar.idCuenta, _usuarioActual);
                            
                            if (bloqueo.exito)
                            {
                                // ✅ DESMARCAR TODAS LAS DEMÁS
                                foreach (var c in CuentasPendientes)
                                {
                                    c.SetIsSelectedFromCommand(false);
                                }

                                // ✅ MARCAR COMO SELECCIONADA (para el RadioButton)
                                cuentaASeleccionar.SetIsSelectedFromCommand(true);

                                // ✅ ASIGNAR AL VIEWMODEL (dispara el PropertyChanged)
                                CuentaSeleccionada = cuentaASeleccionar;
                                
                                System.Diagnostics.Debug.WriteLine($"🎯 Cuenta restaurada y bloqueada: {cuentaASeleccionar.NombreCliente} (ID: {cuentaASeleccionar.idCuenta}) por {usuarioActual}");
                            }
                            else
                            {
                                // Si no se puede bloquear, limpiar la selección previa
                                ProyectoSauna.Models.SesionActual.CuentaSeleccionadaId = 0;
                                _selectedCuentaId = null;
                                System.Diagnostics.Debug.WriteLine($"⚠️ No se pudo restaurar cuenta {cuentaASeleccionar.idCuenta}: {bloqueo.mensaje}");
                            }
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

        // ✅ MODIFICADO: Seleccionar cuenta desde RadioButton con control de edición simultánea
        public async Task SeleccionarCuentaAsync(CuentaPendiente cuenta)
        {
            if (cuenta == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"🎯 SeleccionarCuentaAsync llamado para cuenta: {cuenta.idCuenta} - {cuenta.NombreCliente}");

                // 🛡️ VERIFICACIÓN PREVIA: NO PERMITIR SELECCIÓN SI ESTÁ BLOQUEADA
                var estadoBloqueo = _cuentaEnEdicionService.VerificarCuentaEnEdicion(cuenta.idCuenta);
                if (estadoBloqueo.enEdicion)
                {
                    // 🔒 VERIFICAR SI EL USUARIO ACTUAL ES EL QUE ESTÁ EDITANDO
                    if (estadoBloqueo.usuarioEditor.Equals(_usuarioActual, StringComparison.OrdinalIgnoreCase))
                    {
                        // ✅ ES EL MISMO USUARIO - PERMITIR SELECCIÓN SIN MENSAJE
                        System.Diagnostics.Debug.WriteLine($"🔄 Usuario {_usuarioActual} regresando a su propia cuenta {cuenta.idCuenta}");
                    }
                    else
                    {
                        // 🚫 CUENTA BLOQUEADA POR OTRO USUARIO - NO CAMBIAR SELECCIÓN
                        var mensaje = $"⚠️ La cuenta '{cuenta.NombreCliente}' ya está siendo editada por {estadoBloqueo.usuarioEditor}.\n\n" +
                                      "No es posible seleccionarla mientras esté en uso.";
                        
                        MessageBox.Show(mensaje, "🔒 Cuenta en Edición", MessageBoxButton.OK, MessageBoxImage.Warning);
                        
                        // 🔄 RESTAURAR ESTADO DEL RADIOBUTTON Y FILA SIN CAMBIAR SELECCIÓN
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            cuenta.SetIsSelectedFromCommand(false); // Desmarcar el RadioButton
                        });
                        
                        return; // ❌ SALIR SIN CAMBIAR LA SELECCIÓN ACTUAL
                    }
                }

                // 🔒 VERIFICAR SI LA CUENTA YA ESTÁ SIENDO EDITADA
                var verificacion = _cuentaEnEdicionService.VerificarCuentaEnEdicion(cuenta.idCuenta);
                var usuarioActual = string.IsNullOrEmpty(ProyectoSauna.Models.SesionActual.NombreCompleto) 
                    ? Environment.UserName ?? "Usuario" 
                    : ProyectoSauna.Models.SesionActual.NombreCompleto;
                    
                // ✅ SI YA ESTÁ BLOQUEADA POR EL MISMO USUARIO, PERMITIR SELECCIÓN
                if (verificacion.enEdicion && verificacion.usuarioEditor.Equals(usuarioActual, StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Cuenta {cuenta.idCuenta} ya está bloqueada por el mismo usuario: {usuarioActual}");
                    
                    // 🔓 LIBERAR CUENTA ANTERIORMENTE SELECCIONADA DIFERENTE
                    if (CuentaSeleccionada != null && CuentaSeleccionada.idCuenta != cuenta.idCuenta)
                    {
                        _cuentaEnEdicionService.LiberarBloqueCuenta(CuentaSeleccionada.idCuenta, usuarioActual);
                        CuentaSeleccionada.SetIsSelectedFromCommand(false);
                        System.Diagnostics.Debug.WriteLine($"🔓 Liberada cuenta anterior: {CuentaSeleccionada.idCuenta}");
                    }
                    
                    // 📝 DESMARCAR TODAS Y MARCAR LA SELECCIONADA
                    if (CuentasPendientes != null)
                    {
                        foreach (var c in CuentasPendientes)
                        {
                            c.SetIsSelectedFromCommand(c.idCuenta == cuenta.idCuenta);
                        }
                    }
                    
                    CuentaSeleccionada = cuenta;
                    
                    // 🔄 SINCRONIZAR CON SELECCIÓN DEL DATAGRID
                    _isUpdatingDataGridSelection = true;
                    SelectedDataGridItem = cuenta;
                    _isUpdatingDataGridSelection = false;
                    
                    // ⏸️ PAUSAR ACTUALIZACIONES AUTOMÁTICAS
                    PausarActualizaciones();
                    return;
                }
                
                if (verificacion.enEdicion && !verificacion.usuarioEditor.Equals(usuarioActual, StringComparison.OrdinalIgnoreCase))
                {
                    var usuario = string.IsNullOrEmpty(verificacion.usuarioEditor) ? "otro usuario" : verificacion.usuarioEditor;
                    System.Diagnostics.Debug.WriteLine($"❌ Cuenta {cuenta.idCuenta} ya en edición por: {usuario}");
                    
                    MessageBox.Show(
                        $"⚠️ La cuenta de {cuenta.NombreCliente} ya está siendo utilizada por {usuario}.\n\n" +
                        "No puede acceder a esta cuenta hasta que el otro usuario termine de trabajar con ella.",
                        "Cuenta en Uso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    
                    // 🔄 ASEGURAR QUE EL RADIOBUTTON NO SE MARQUE
                    cuenta.SetIsSelectedFromCommand(false);
                    return;
                }

                        // 🔒 INTENTAR BLOQUEAR LA CUENTA PARA EDICIÓN
                        var bloqueo = _cuentaEnEdicionService.IntentarBloquearCuenta(cuenta.idCuenta, _usuarioActual);                if (!bloqueo.exito)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ No se pudo bloquear cuenta {cuenta.idCuenta}: {bloqueo.mensaje}");
                    
                    MessageBox.Show(
                        $"❌ {bloqueo.mensaje}\n\n" +
                        "No puede trabajar con esta cuenta en este momento.",
                        "Cuenta No Disponible",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    
                    // 🔄 ASEGURAR QUE EL RADIOBUTTON NO SE MARQUE
                    cuenta.SetIsSelectedFromCommand(false);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"✅ Cuenta {cuenta.idCuenta} bloqueada exitosamente por {usuarioActual}");

                // 🔓 LIBERAR CUENTA ANTERIORMENTE SELECCIONADA
                if (CuentaSeleccionada != null && CuentaSeleccionada.idCuenta != cuenta.idCuenta)
                {
                    _cuentaEnEdicionService.LiberarBloqueCuenta(CuentaSeleccionada.idCuenta, usuarioActual);
                    System.Diagnostics.Debug.WriteLine($"🔓 Liberada cuenta anterior: {CuentaSeleccionada.idCuenta}");
                }

                // ✅ SELECCIONAR LA NUEVA CUENTA
                System.Diagnostics.Debug.WriteLine($"🎯 Marcando cuentas - Total cuentas: {CuentasPendientes?.Count ?? 0}");
                
                // Desmarcar todas y marcar solo la seleccionada
                if (CuentasPendientes != null)
                {
                    foreach (var c in CuentasPendientes)
                    {
                        bool debeEstarSeleccionada = c.idCuenta == cuenta.idCuenta;
                        c.SetIsSelectedFromCommand(debeEstarSeleccionada);
                        System.Diagnostics.Debug.WriteLine($"🔘 Cuenta {c.idCuenta} marcada: {debeEstarSeleccionada}");
                    }
                }
                
                CuentaSeleccionada = cuenta;
                
                // 🔄 SINCRONIZAR CON SELECCIÓN DEL DATAGRID
                _isUpdatingDataGridSelection = true;
                SelectedDataGridItem = cuenta;
                _isUpdatingDataGridSelection = false;
                
                // ⏸️ PAUSAR ACTUALIZACIONES AUTOMÁTICAS AL SELECCIONAR CUENTA
                PausarActualizaciones();
                
                // 📡 SINCRONIZACIÓN INMEDIATA: Forzar verificación para que otras ventanas vean el bloqueo
                System.Diagnostics.Debug.WriteLine($"📡 Forzando sincronización tras bloqueo de cuenta {cuenta.idCuenta}");
                _ = Task.Run(async () =>
                {
                    await Task.Delay(100); // Pequeña pausa para asegurar que el bloqueo se escribió
                    Application.Current?.Dispatcher.InvokeAsync(() => 
                    {
                        VerificarEstadoEdicionCuentas(forzarActualizacion: true);
                    });
                });
                
                System.Diagnostics.Debug.WriteLine($"🎯 Cuenta seleccionada y bloqueada: {cuenta.NombreCliente} (ID: {cuenta.idCuenta}) por {usuarioActual}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al seleccionar cuenta: {ex.Message}");
                MessageBox.Show($"Error al seleccionar cuenta: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ MODIFICADO: Limpiar cuenta activa con liberación de bloqueo
        private async Task LimpiarCuentaActiva()
        {
            if (CuentaSeleccionada == null) return;

            var resultado = MessageBox.Show(
                "¿Desea dejar de trabajar con esta cuenta?\n\n" +
                "Podrá seleccionar otra cuenta después.",
                "Cambiar Cuenta",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    // 🔓 LIBERAR BLOQUEO DE LA CUENTA
                    var usuarioActual = string.IsNullOrEmpty(ProyectoSauna.Models.SesionActual.NombreCompleto) 
                        ? Environment.UserName ?? "Usuario" 
                        : ProyectoSauna.Models.SesionActual.NombreCompleto;
                    var idCuentaALiberar = CuentaSeleccionada.idCuenta;
                    _cuentaEnEdicionService.LiberarBloqueCuenta(idCuentaALiberar, usuarioActual);
                    
                    System.Diagnostics.Debug.WriteLine($"🔓 Cuenta liberada: {idCuentaALiberar} por {usuarioActual}");

                    // Desmarcar todas
                    foreach (var c in CuentasPendientes)
                    {
                        c.SetIsSelectedFromCommand(false);
                    }

                    CuentaSeleccionada = null;
                    ProyectoSauna.Models.SesionActual.CuentaSeleccionadaId = 0;
                    ConsumosCuentaActual.Clear();
                    
                    // ▶️ REACTIVAR ACTUALIZACIONES AUTOMÁTICAS AL LIMPIAR SELECCIÓN
                    await ReactivarActualizacionesAsync();
                    System.Diagnostics.Debug.WriteLine("🧙 Cuenta deseleccionada y liberada - Actualizaciones reactivadas");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error al liberar bloqueo: {ex.Message}");
                    MessageBox.Show($"Error al liberar cuenta: {ex.Message}", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
                    // 🔒 INTENTAR BLOQUEAR LA CUENTA ENCONTRADA
                    var usuarioActual = _usuarioActual;
                    var bloqueo = _cuentaEnEdicionService.IntentarBloquearCuenta(keep.idCuenta, usuarioActual);
                    
                    if (bloqueo.exito)
                    {
                        // ✅ TAMBIÉN MARCAR VISUALMENTE
                        foreach (var c in CuentasPendientes)
                        {
                            c.SetIsSelectedFromCommand(false);
                        }
                        keep.SetIsSelectedFromCommand(true);
                        CuentaSeleccionada = keep;
                        System.Diagnostics.Debug.WriteLine($"🔒 Cuenta asegurada y bloqueada: {keep.idCuenta}");
                    }
                    else
                    {
                        // Si no se puede bloquear, limpiar selección
                        ProyectoSauna.Models.SesionActual.CuentaSeleccionadaId = 0;
                        System.Diagnostics.Debug.WriteLine($"⚠️ No se pudo asegurar cuenta {keep.idCuenta}: {bloqueo.mensaje}");
                    }
                    return;
                }

                // Recuperar desde BD si no está presente en la lista
                var cuenta = await _cuentaRepository.GetCuentaByIdAsync(idSesion);
                if (cuenta != null)
                {
                    // 🔒 INTENTAR BLOQUEAR ANTES DE CREAR LA CUENTA PENDIENTE
                    var usuarioActual = _usuarioActual;
                    var bloqueo = _cuentaEnEdicionService.IntentarBloquearCuenta(cuenta.idCuenta, usuarioActual);
                    
                    if (bloqueo.exito)
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
                            EstadoCuenta = cuenta.idEstadoCuentaNavigation?.nombre ?? "",
                            ParentViewModel = this // 🔗 ASIGNAR REFERENCIA AL VIEWMODEL PADRE
                        };
                        cuentaPendiente.ActualizarTiempo();
                        cuentaPendiente.SetIsSelectedFromCommand(true);
                        CuentaSeleccionada = cuentaPendiente;
                        System.Diagnostics.Debug.WriteLine($"🔒 Cuenta desde BD bloqueada: {cuenta.idCuenta}");
                    }
                    else
                    {
                        // Si no se puede bloquear, limpiar selección
                        ProyectoSauna.Models.SesionActual.CuentaSeleccionadaId = 0;
                        System.Diagnostics.Debug.WriteLine($"⚠️ No se pudo bloquear cuenta desde BD {cuenta.idCuenta}: {bloqueo.mensaje}");
                    }
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

            try
            {
                EstaCargando = true;

                // 🛡️ VALIDACIÓN AVANZADA: PREVENIR CUENTAS SIMULTÁNEAS
                var validacionUnica = await _cuentaUnicaService.ValidarCreacionCuentaAsync(IdClienteEncontrado);
                if (!validacionUnica.puedeCrear)
                {
                    var resultado = MessageBox.Show(
                        validacionUnica.mensaje + "\n\n¿Desea abrir la cuenta existente en su lugar?",
                        "Cuenta Existente Detectada",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (resultado == MessageBoxResult.Yes && validacionUnica.idCuentaExistente.HasValue)
                    {
                        // Buscar la cuenta existente y seleccionarla
                        await CargarCuentasPendientesAsync();
                        var cuentaExistente = CuentasPendientes.FirstOrDefault(c => c.idCuenta == validacionUnica.idCuentaExistente.Value);
                        if (cuentaExistente != null)
                        {
                            await SeleccionarCuentaAsync(cuentaExistente);
                            MessageBox.Show($"✅ Cuenta #{cuentaExistente.idCuenta} seleccionada correctamente.",
                                "Cuenta Abierta", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    return;
                }

                // 🛡️ CREAR CUENTA DE FORMA SEGURA CONTRA CONCURRENCIA
                var creacionSegura = await _cuentaUnicaService.CrearCuentaSeguraAsync(
                    IdClienteEncontrado,
                    15.00m, // Precio entrada por defecto
                    SesionActual.IdUsuario);

                if (!creacionSegura.exito)
                {
                    MessageBox.Show(creacionSegura.mensaje,
                        "Conflicto de Creación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    
                    // Recargar lista para mostrar la cuenta que otro usuario pudo haber creado
                    await CargarCuentasPendientesAsync();
                    return;
                }

                MessageBox.Show(creacionSegura.mensaje,
                    "Cuenta Creada Exitosamente",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // 🔓 LIBERAR BLOQUEO DE CUENTA ANTERIOR ANTES DE SELECCIONAR NUEVA
                if (CuentaSeleccionada != null)
                {
                    var usuarioActual = string.IsNullOrEmpty(ProyectoSauna.Models.SesionActual.NombreCompleto) 
                        ? Environment.UserName ?? "Usuario" 
                        : ProyectoSauna.Models.SesionActual.NombreCompleto;
                    _cuentaEnEdicionService.LiberarBloqueCuenta(CuentaSeleccionada.idCuenta, usuarioActual);
                    System.Diagnostics.Debug.WriteLine($"🔓 Liberado bloqueo de cuenta anterior {CuentaSeleccionada.idCuenta} antes de crear nueva");
                }

                // Recargar lista y seleccionar la cuenta recién creada
                await CargarCuentasPendientesAsync();
                if (creacionSegura.idCuentaCreada.HasValue)
                {
                    var cuentaNueva = CuentasPendientes.FirstOrDefault(c => c.idCuenta == creacionSegura.idCuentaCreada.Value);
                    if (cuentaNueva != null)
                    {
                        // 🆕 ACTIVAR FLAG PARA EVITAR VERIFICACION INNECESARIA
                        _creandoCuentaNueva = true;
                        try
                        {
                            await SeleccionarCuentaAsync(cuentaNueva);
                            System.Diagnostics.Debug.WriteLine($"✅ Cuenta nueva {cuentaNueva.idCuenta} seleccionada después de liberar anterior");
                        }
                        finally
                        {
                            // ✅ DESACTIVAR FLAG DESPUÉS DE LA SELECCIÓN
                            _creandoCuentaNueva = false;
                        }
                    }
                }

                await LimpiarBusquedaAsync();

                // 🔄 SINCRONIZAR CON TODAS LAS VENTANAS (CON DELAY PARA EVITAR AUTO-SINCRONIZACIÓN)
                // Notificar a otras instancias sobre el cambio
                _ = Task.Run(async () =>
                {
                    // Pequeño delay para asegurar que la selección se haya completado
                    await Task.Delay(200);
                    _inventoryEventService?.OnStockChanged(new StockChangedEventArgs
                    {
                        ProductoId = 0, // No es producto, es cuenta nueva
                        NuevoStock = 0,
                        TipoMovimiento = "CUENTA_CREADA",
                        IdCuenta = creacionSegura.idCuentaCreada
                    });
                });
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

        /// <summary>
        /// 🔒 Verifica el estado de edición de todas las cuentas para mostrar indicadores visuales Y sincronizar RadioButtons dinámicamente
        /// </summary>
        private void VerificarEstadoEdicionCuentas(bool forzarActualizacion = false)
        {
            try
            {
                var usuarioActual = _usuarioActual;

                System.Diagnostics.Debug.WriteLine($"🔍 Verificando estado de edición de cuentas... (Forzado: {forzarActualizacion})");
                
                if (CuentasPendientes == null) return;
                
                // 🎯 SOLO SINCRONIZAR RADIOBUTTONS SI NO HAY CUENTA SELECCIONADA O ES FORZADO
                bool puedeActualizarSelecciones = forzarActualizacion || CuentaSeleccionada == null;
                
                foreach (var cuenta in CuentasPendientes)
                {
                    var estadoAnterior = cuenta.EstaSiendoEditada;
                    var usuarioAnterior = cuenta.UsuarioEditor;
                    var seleccionAnterior = cuenta.IsSelected;
                    
                    // 🔍 VERIFICAR ESTADO ACTUAL DEL BLOQUEO
                    var estado = _cuentaEnEdicionService.VerificarCuentaEnEdicion(cuenta.idCuenta);
                    cuenta.EstaSiendoEditada = estado.enEdicion;
                    cuenta.UsuarioEditor = estado.usuarioEditor ?? "";
                    
                    // 🎯 ACTUALIZAR ESTADO DEL RADIOBUTTON SEGÚN BLOQUEO
                    bool radioButtonDebeEstarHabilitado = !estado.enEdicion || 
                        (estado.enEdicion && estado.usuarioEditor.Equals(usuarioActual, StringComparison.OrdinalIgnoreCase));
                    
                    if (cuenta.IsRadioButtonEnabled != radioButtonDebeEstarHabilitado)
                    {
                        cuenta.IsRadioButtonEnabled = radioButtonDebeEstarHabilitado;
                        System.Diagnostics.Debug.WriteLine($"📱 RadioButton cuenta {cuenta.idCuenta}: {(radioButtonDebeEstarHabilitado ? "HABILITADO" : "DESHABILITADO")}");
                    }
                    
                    // 🎯 SOLO ACTUALIZAR RADIOBUTTONS SI ESTÁ PERMITIDO
                    if (puedeActualizarSelecciones)
                    {
                        // 🎯 SINCRONIZACIÓN DINÁMICA DE RADIOBUTTON CON BLOQUEO
                        bool debeEstarSeleccionada = estado.enEdicion && 
                            !string.IsNullOrEmpty(estado.usuarioEditor) &&
                            estado.usuarioEditor.Equals(usuarioActual, StringComparison.OrdinalIgnoreCase);
                        
                        // 🔄 ACTUALIZAR RADIOBUTTON SI HAY CAMBIO EN LA SELECCIÓN
                        if (seleccionAnterior != debeEstarSeleccionada)
                        {
                            System.Diagnostics.Debug.WriteLine($"🔄 Sincronizando RadioButton cuenta {cuenta.idCuenta}: {seleccionAnterior} → {debeEstarSeleccionada} (Usuario: {estado.usuarioEditor})");
                            
                            // Actualizar en UI thread
                            Application.Current?.Dispatcher.InvokeAsync(() =>
                            {
                                cuenta.SetIsSelectedFromCommand(debeEstarSeleccionada);
                                
                                // 🎯 ACTUALIZAR CUENTA SELECCIONADA Y DATAGRID SOLO SI ES NECESARIO
                                if (debeEstarSeleccionada)
                                {
                                    // ✅ ESTA CUENTA AHORA ESTÁ SELECCIONADA
                                    if (CuentaSeleccionada?.idCuenta != cuenta.idCuenta)
                                    {
                                        CuentaSeleccionada = cuenta;
                                        _isUpdatingDataGridSelection = true;
                                        SelectedDataGridItem = cuenta;
                                        _isUpdatingDataGridSelection = false;
                                        System.Diagnostics.Debug.WriteLine($"✅ CuentaSeleccionada actualizada dinámicamente: {cuenta.idCuenta}");
                                    }
                                }
                                else if (CuentaSeleccionada?.idCuenta == cuenta.idCuenta)
                                {
                                    // ❌ ESTA CUENTA YA NO ESTÁ SELECCIONADA
                                    CuentaSeleccionada = null;
                                    _isUpdatingDataGridSelection = true;
                                    SelectedDataGridItem = null;
                                    _isUpdatingDataGridSelection = false;
                                    
                                    // ▶️ REANUDAR ACTUALIZACIONES AL DESELECCIONAR
                                    _ = ReactivarActualizacionesAsync();
                                    System.Diagnostics.Debug.WriteLine($"❌ CuentaSeleccionada liberada dinámicamente: {cuenta.idCuenta}");
                                }
                            });
                        }
                    }
                    
                    // 🔓 DETECTAR LIBERACIÓN DE CUENTA POR OTRO USUARIO (SIEMPRE MOSTRAR)
                    if (estadoAnterior && !estado.enEdicion && 
                        !string.IsNullOrEmpty(usuarioAnterior) && 
                        !usuarioAnterior.Equals(usuarioActual, StringComparison.OrdinalIgnoreCase))
                    {
                        System.Diagnostics.Debug.WriteLine($"🔓 Cuenta {cuenta.idCuenta} liberada por {usuarioAnterior}");
                    }
                    
                    // 🔒 DETECTAR NUEVA SELECCIÓN POR OTRO USUARIO (SIEMPRE MOSTRAR)  
                    if (!estadoAnterior && estado.enEdicion && 
                        !string.IsNullOrEmpty(estado.usuarioEditor) &&
                        !estado.usuarioEditor.Equals(usuarioActual, StringComparison.OrdinalIgnoreCase))
                    {
                        System.Diagnostics.Debug.WriteLine($"🔒 Cuenta {cuenta.idCuenta} ahora en edición por {estado.usuarioEditor}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error verificando estado de edición: {ex.Message}");
            }
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

            try
            {
                // 🛡️ VALIDAR ESTADO DE CUENTA
                var validacionCuenta = await _validacionService.ValidarCuentaParaModificacionAsync(CuentaSeleccionada.idCuenta);
                if (!validacionCuenta.esValida)
                {
                    MessageBox.Show(validacionCuenta.mensaje, "Cuenta No Modificable",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 🛡️ VERIFICAR SI TIENE CONSUMOS Y MOSTRAR INFORMACIÓN
                var validacionConsumos = await _validacionService.ValidarCuentaTieneConsumosAsync(CuentaSeleccionada.idCuenta);
                
                string mensajeConfirmacion = $"⚠️ ¿Está seguro de eliminar la cuenta?\n\n" +
                                           $"Cliente: {CuentaSeleccionada.NombreCliente}\n" +
                                           $"ID Cuenta: {CuentaSeleccionada.idCuenta}\n\n";
                
                if (validacionConsumos.tieneConsumos)
                {
                    mensajeConfirmacion += $"🔍 ATENCIÓN: {validacionConsumos.mensaje}\n\n" +
                                         $"Al eliminar la cuenta se devolverá automáticamente el stock de todos los productos.\n\n" +
                                         $"💡 RECOMENDACIÓN: Considere procesar el pago en lugar de eliminar la cuenta.\n\n";
                }

                mensajeConfirmacion += "Esta acción no se puede deshacer.";

                var resultado = MessageBox.Show(mensajeConfirmacion,
                    "Confirmar Eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (resultado != MessageBoxResult.Yes)
                    return;

                EstaCargando = true;

                using var context = new SaunaDbContext();
                var repoConsumo = new DetalleConsumoRepository(context);
                var repoServicio = new DetalleServicioRepository(context);
                var productoRepo = new ProductoRepository(context);
                var movimientoRepo = new MovimientoInventarioRepository(context);

                var consumos = await repoConsumo.GetByCuentaAsync(CuentaSeleccionada.idCuenta);

                // 🛡️ DEVOLUCIÓN SEGURA DE STOCK CON TRY-CATCH
                foreach (var consumo in consumos)
                {
                    try
                    {
                        var producto = await productoRepo.GetByIdAsync(consumo.idProducto);
                        if (producto != null)
                        {
                            var stockAntes = producto.stockActual;
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
                    catch (DbUpdateConcurrencyException)
                    {
                        MessageBox.Show($"Conflicto al devolver stock del producto {consumo.idProducto}. Se continuará con otros productos.",
                            "Advertencia de Concurrencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al devolver stock: {ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                // 🔄 SINCRONIZAR CON TODAS LAS VENTANAS
                var cuentaEliminada = CuentaSeleccionada.idCuenta;
                await CargarCuentasPendientesAsync();
                await CargarProductosAsync();

                // Notificar eliminación de cuenta a todas las instancias
                _inventoryEventService?.OnStockChanged(new StockChangedEventArgs
                {
                    ProductoId = 0,
                    NuevoStock = 0,
                    TipoMovimiento = "CUENTA_ELIMINADA",
                    IdCuenta = cuentaEliminada
                });

                InventoryEventService.NotifyStockChanged();

                CuentaSeleccionada = null;
                
                // ▶️ REACTIVAR ACTUALIZACIONES AL ELIMINAR CUENTA SELECCIONADA
                await ReactivarActualizacionesAsync();
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
                // 🛡️ VALIDAR ESTADO DE CUENTA
                var validacionCuenta = await _validacionService.ValidarCuentaParaModificacionAsync(CuentaSeleccionada.idCuenta);
                if (!validacionCuenta.esValida)
                {
                    MessageBox.Show(validacionCuenta.mensaje, "Cuenta No Modificable",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 🛡️ VALIDAR STOCK CON SERVICIO DE VALIDACIÓN
                var validacionStock = await _validacionService.ValidarStockProductoAsync(
                    ProductoSeleccionado.idProducto, CantidadProducto);
                
                if (!validacionStock.hayStock)
                {
                    MessageBox.Show(validacionStock.mensaje, "Stock Insuficiente",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
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

        #region Sincronización entre Ventanas
        
        /// <summary>
        /// 🔄 Maneja la sincronización entre ventanas cuando se crean o eliminan cuentas
        /// INTELIGENTE: Actualiza lista pero preserva selección actual cuando hay cuenta seleccionada
        /// </summary>
        private async void OnStockChanged_SincronizarCuentas(object sender, StockChangedEventArgs e)
        {
            try
            {
                // 🚫 NO SINCRONIZAR SI ESTAMOS CREANDO CUENTA NUEVA (evita conflictos)
                if (_creandoCuentaNueva)
                {
                    System.Diagnostics.Debug.WriteLine("⏸️ Sincronización pausada - creando cuenta nueva");
                    return;
                }
                
                // Solo sincronizar cuando se trata de eventos de cuentas
                if (e.TipoMovimiento == "CUENTA_CREADA" || e.TipoMovimiento == "CUENTA_ELIMINADA")
                {
                    await Application.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        System.Diagnostics.Debug.WriteLine($"🔄 Evento recibido: {e.TipoMovimiento} - ID: {e.IdCuenta}");
                        
                        // 🆕 NUEVA LÓGICA: Siempre actualizar, pero preservar selección cuando existe
                        if (CuentaSeleccionada != null)
                        {
                            // Guardar selección actual para restaurarla después
                            var cuentaSeleccionadaId = CuentaSeleccionada.idCuenta;
                            System.Diagnostics.Debug.WriteLine($"💾 Preservando selección de cuenta {cuentaSeleccionadaId} durante actualización");
                            
                            // Actualizar lista preservando selección
                            await ActualizarListaCuentasConSeleccionAsync(cuentaSeleccionadaId);
                        }
                        else
                        {
                            // No hay selección, actualización normal
                            System.Diagnostics.Debug.WriteLine("🔄 Actualizando lista sin selección activa");
                            await ActualizarListaCuentasAsync();
                        }
                        
                        // Si una cuenta fue eliminada y es la que teníamos seleccionada, limpiar selección
                        if (e.TipoMovimiento == "CUENTA_ELIMINADA" && e.IdCuenta.HasValue)
                        {
                            if (CuentaSeleccionada?.idCuenta == e.IdCuenta.Value)
                            {
                                System.Diagnostics.Debug.WriteLine($"🗑️ La cuenta seleccionada ({e.IdCuenta.Value}) fue eliminada, limpiando selección");
                                await LimpiarCuentaActiva();
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en sincronización de cuentas: {ex.Message}");
            }
        }
        
        #region Control de Actualización Inteligente
        
        /// <summary>
        /// 🔄 Actualiza la lista de cuentas de manera segura
        /// </summary>
        private async Task ActualizarListaCuentasAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Actualizando lista de cuentas...");
                
                // 🔄 DESELECCIONAR CUENTAS AL RECARGAR PRIMERO
                DeseleccionarTodasLasCuentas();
                
                // 🔓 LIBERAR BLOQUEOS DE CUENTAS ANTES DE RECARGAR
                if (CuentaSeleccionada != null)
                {
                    LiberarBloqueoCuentaActual();
                }
                
                // 🔄 FORZAR ACTUALIZACIÓN HABILITÁNDOLA TEMPORALMENTE
                var estadoAnterior = _actualizacionHabilitada;
                _actualizacionHabilitada = true;
                
                await CargarCuentasPendientesAsync();
                
                // 🔄 FORZAR VERIFICACIÓN DE ESTADOS DESPUÉS DE CARGAR
                VerificarEstadoEdicionCuentas(forzarActualizacion: true);
                
                _hayPendienteActualizacion = false;
                _actualizacionHabilitada = estadoAnterior;
                
                System.Diagnostics.Debug.WriteLine("✅ Lista de cuentas actualizada y deseleccionada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al actualizar lista de cuentas: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 🔄 Actualiza la lista de cuentas preservando la selección especificada
        /// </summary>
        private async Task ActualizarListaCuentasConSeleccionAsync(int cuentaSeleccionadaId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔄 Actualizando lista preservando selección de cuenta {cuentaSeleccionadaId}...");
                
                // Guardar referencia al usuario actual
                var usuarioActual = string.IsNullOrEmpty(ProyectoSauna.Models.SesionActual.NombreCompleto) 
                    ? Environment.UserName ?? "Usuario" 
                    : ProyectoSauna.Models.SesionActual.NombreCompleto;
                
                // Temporal: Forzar actualización
                var estadoAnterior = _actualizacionHabilitada;
                _actualizacionHabilitada = true;
                
                // Cargar nuevas cuentas
                await CargarCuentasPendientesAsync();
                
                // Intentar restaurar la selección después de la actualización
                var cuentaARestaurar = CuentasPendientes.FirstOrDefault(c => c.idCuenta == cuentaSeleccionadaId);
                if (cuentaARestaurar != null)
                {
                    // Verificar que el bloqueo sigue activo y es nuestro
                    var estadoBloqueo = _cuentaEnEdicionService.VerificarCuentaEnEdicion(cuentaSeleccionadaId);
                    if (estadoBloqueo.enEdicion && estadoBloqueo.usuarioEditor == usuarioActual)
                    {
                        // Restaurar selección visual
                        foreach (var c in CuentasPendientes)
                        {
                            c.SetIsSelectedFromCommand(c.idCuenta == cuentaSeleccionadaId);
                        }
                        
                        // Restaurar selección en el ViewModel
                        CuentaSeleccionada = cuentaARestaurar;
                        System.Diagnostics.Debug.WriteLine($"✅ Selección restaurada para cuenta {cuentaSeleccionadaId}");
                    }
                    else
                    {
                        // El bloqueo se perdió, limpiar selección
                        System.Diagnostics.Debug.WriteLine($"⚠️ Bloqueo perdido para cuenta {cuentaSeleccionadaId}, limpiando selección");
                        await LimpiarCuentaActiva();
                    }
                }
                else
                {
                    // La cuenta ya no existe, limpiar selección
                    System.Diagnostics.Debug.WriteLine($"⚠️ Cuenta {cuentaSeleccionadaId} ya no existe, limpiando selección");
                    await LimpiarCuentaActiva();
                }
                
                // Verificar estados de edición
                VerificarEstadoEdicionCuentas(forzarActualizacion: true);
                
                _hayPendienteActualizacion = false;
                _actualizacionHabilitada = estadoAnterior;
                
                System.Diagnostics.Debug.WriteLine("✅ Lista actualizada con preservación de selección");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al actualizar lista con selección: {ex.Message}");
                // En caso de error, limpiar selección por seguridad
                await LimpiarCuentaActiva();
            }
        }
        
        /// <summary>
        /// ⏸️ Pausa las actualizaciones automáticas (cuando se selecciona una cuenta)
        /// </summary>
        private void PausarActualizaciones()
        {
            _actualizacionHabilitada = false;
            System.Diagnostics.Debug.WriteLine("⏸️ Actualizaciones de lista PAUSADAS");
        }
        
        /// <summary>
        /// ▶️ Reactiva las actualizaciones automáticas (cuando no hay cuenta seleccionada)
        /// </summary>
        private async Task ReactivarActualizacionesAsync()
        {
            _actualizacionHabilitada = true;
            System.Diagnostics.Debug.WriteLine("▶️ Actualizaciones de lista REACTIVADAS");
            
            // Si había una actualización pendiente, ejecutarla ahora
            if (_hayPendienteActualizacion)
            {
                System.Diagnostics.Debug.WriteLine("🗃️ Ejecutando actualización pendiente...");
                await ActualizarListaCuentasAsync();
            }
        }
        
        /// <summary>
        /// 🔍 Verifica periódicamente si hay actualizaciones pendientes cuando no hay cuenta seleccionada
        /// </summary>
        private async Task VerificarActualizacionPendiente()
        {
            try
            {
                // Solo verificar si no hay cuenta seleccionada y las actualizaciones están habilitadas
                if (CuentaSeleccionada == null && _actualizacionHabilitada && _hayPendienteActualizacion)
                {
                    System.Diagnostics.Debug.WriteLine("🔄 Ejecutando actualización pendiente automática...");
                    await ActualizarListaCuentasAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en verificación de actualización pendiente: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 🔄 Deselecciona todas las cuentas seleccionadas (usado en recarga) con sincronización dinámica
        /// </summary>
        private void DeseleccionarTodasLasCuentas()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Deseleccionando todas las cuentas...");
                
                var usuarioActual = string.IsNullOrEmpty(ProyectoSauna.Models.SesionActual.NombreCompleto) 
                    ? Environment.UserName ?? "Usuario" 
                    : ProyectoSauna.Models.SesionActual.NombreCompleto;
                
                // 🔓 LIBERAR CUENTA SELECCIONADA ACTUAL PRIMERO
                if (CuentaSeleccionada != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🔓 Liberando cuenta seleccionada: {CuentaSeleccionada.idCuenta}");
                    _cuentaEnEdicionService.LiberarBloqueCuenta(CuentaSeleccionada.idCuenta, usuarioActual);
                    CuentaSeleccionada = null;
                }
                
                // 🔄 LIBERAR TODOS LOS BLOQUEOS Y DESMARCAR RADIOBUTTONS
                if (CuentasPendientes != null)
                {
                    foreach (var cuenta in CuentasPendientes.Where(c => c.IsSelected).ToList())
                    {
                        // Liberar bloqueo si está seleccionada
                        _cuentaEnEdicionService.LiberarBloqueCuenta(cuenta.idCuenta, usuarioActual);
                        System.Diagnostics.Debug.WriteLine($"🔓 Liberado bloqueo de cuenta {cuenta.idCuenta}");
                        
                        // Desmarcar RadioButton directamente ya que no hay interacción con comando
                        cuenta.SetIsSelectedFromCommand(false);
                    }
                }
                
                // Limpiar selección del DataGrid DESPUÉS
                SelectedDataGridItem = null;
                
                // 📡 FORZAR SINCRONIZACIÓN PARA OTRAS VENTANAS
                _ = Task.Run(async () =>
                {
                    await Task.Delay(100); // Pequeña pausa para asegurar que todas las liberaciones se escribieron
                    Application.Current?.Dispatcher.InvokeAsync(() => 
                    {
                        VerificarEstadoEdicionCuentas(forzarActualizacion: true);
                    });
                });
                
                System.Diagnostics.Debug.WriteLine("✅ Todas las cuentas deseleccionadas y liberadas");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error deseleccionando cuentas: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 🔓 Deselecciona una cuenta específica que fue liberada por otro usuario
        /// </summary>
        private void DeseleccionarCuentaLiberada(int cuentaId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔓 Deseleccionando cuenta liberada ID: {cuentaId}");
                
                var cuenta = CuentasPendientes.FirstOrDefault(c => c.idCuenta == cuentaId);
                if (cuenta != null && cuenta.IsSelected)
                {
                    cuenta.SetIsSelectedFromCommand(false);
                    
                    // Si era la cuenta seleccionada en el DataGrid, limpiarla también
                    if (SelectedDataGridItem?.idCuenta == cuentaId)
                    {
                        SelectedDataGridItem = null;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"✅ Cuenta {cuentaId} deseleccionada automáticamente");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error deseleccionando cuenta {cuentaId}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 🔓 Libera el bloqueo de la cuenta actualmente seleccionada
        /// </summary>
        private void LiberarBloqueoCuentaActual()
        {
            try
            {
                if (CuentaSeleccionada != null)
                {
                    var usuarioActual = _usuarioActual;
                        
                    System.Diagnostics.Debug.WriteLine($"🔓 Liberando bloqueo de cuenta {CuentaSeleccionada.idCuenta} por usuario {usuarioActual}");
                    
                    _cuentaEnEdicionService.LiberarBloqueCuenta(CuentaSeleccionada.idCuenta, usuarioActual);
                    
                    // 📡 FORZAR SINCRONIZACIÓN PARA OTRAS VENTANAS
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(100); // Pequeña pausa para asegurar que la liberación se escribió
                        Application.Current?.Dispatcher.InvokeAsync(() => 
                        {
                            VerificarEstadoEdicionCuentas(forzarActualizacion: true);
                        });
                    });
                    
                    System.Diagnostics.Debug.WriteLine($"✅ Bloqueo liberado para cuenta {CuentaSeleccionada.idCuenta}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error liberando bloqueo de cuenta: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 🔓 Deselecciona una cuenta específica con liberación de bloqueo y sincronización completa
        /// </summary>
        public void DeseleccionarCuentaConLiberacion(int idCuenta)
        {
            try
            {
                var usuarioActual = string.IsNullOrEmpty(ProyectoSauna.Models.SesionActual.NombreCompleto) 
                    ? Environment.UserName ?? "Usuario" 
                    : ProyectoSauna.Models.SesionActual.NombreCompleto;
                
                System.Diagnostics.Debug.WriteLine($"🔓 Deseleccionando cuenta {idCuenta} con liberación de bloqueo");
                
                // 🔓 LIBERAR BLOQUEO
                _cuentaEnEdicionService.LiberarBloqueCuenta(idCuenta, usuarioActual);
                
                // 🔄 ENCONTRAR Y DESMARCAR CUENTA EN LA LISTA
                var cuenta = CuentasPendientes?.FirstOrDefault(c => c.idCuenta == idCuenta);
                if (cuenta != null)
                {
                    cuenta.SetIsSelectedFromCommand(false);
                    System.Diagnostics.Debug.WriteLine($"🔘 RadioButton desmarcado para cuenta {idCuenta}");
                }
                
                // 🎯 LIMPIAR SELECCIÓN SI ES LA CUENTA ACTUAL
                if (CuentaSeleccionada?.idCuenta == idCuenta)
                {
                    CuentaSeleccionada = null;
                    _isUpdatingDataGridSelection = true;
                    SelectedDataGridItem = null;
                    _isUpdatingDataGridSelection = false;
                    
                    // ▶️ REANUDAR ACTUALIZACIONES
                    _ = ReactivarActualizacionesAsync();
                    System.Diagnostics.Debug.WriteLine($"🎯 CuentaSeleccionada limpiada: {idCuenta}");
                }
                
                // 📡 FORZAR SINCRONIZACIÓN PARA OTRAS VENTANAS
                _ = Task.Run(async () =>
                {
                    await Task.Delay(100); // Pequeña pausa para asegurar que la liberación se escribió
                    Application.Current?.Dispatcher.InvokeAsync(() => 
                    {
                        VerificarEstadoEdicionCuentas(forzarActualizacion: true);
                    });
                });
                
                System.Diagnostics.Debug.WriteLine($"✅ Cuenta {idCuenta} deseleccionada y liberada completamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al deseleccionar cuenta {idCuenta}: {ex.Message}");
            }
        }
        
        #endregion

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IDisposable
        /// <summary>
        /// 🔒 Libera todos los bloqueos de cuentas al cerrar la ventana
        /// </summary>
        public void Dispose()
        {
            try
            {
                // 🔓 Liberar cuenta actual si está seleccionada
                if (CuentaSeleccionada != null)
                {
                    var usuarioActual = string.IsNullOrEmpty(ProyectoSauna.Models.SesionActual.NombreCompleto) 
                        ? Environment.UserName ?? "Usuario" 
                        : ProyectoSauna.Models.SesionActual.NombreCompleto;
                    _cuentaEnEdicionService?.LiberarBloqueCuenta(CuentaSeleccionada.idCuenta, usuarioActual);
                    System.Diagnostics.Debug.WriteLine($"🔓 Cuenta liberada en Dispose: {CuentaSeleccionada.idCuenta}");
                }

                // Liberar servicios
                _cuentaEnEdicionService?.Dispose();
                _sharedContext?.Dispose();
                
                // Detener timers
                _timer?.Stop();
                _searchTimerProductos?.Stop();
                _searchTimerServicios?.Stop();
                _actualizacionTimer?.Stop();
                _verificacionBloqueoTimer?.Stop(); // 🔒 DETENER VERIFICACIÓN DE BLOQUEOS
                
                System.Diagnostics.Debug.WriteLine("🧹 CuentasViewModel disposed correctamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en Dispose CuentasViewModel: {ex.Message}");
            }
        }
        #endregion
        
        #region 🎨 Métodos de Refresh Visual
        
        #endregion
    }

    // ✅ MODIFICADO: Agregada propiedad IsSelected y EstaSiendoEditada
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

        // 🔗 REFERENCIA AL VIEWMODEL PADRE PARA EJECUTAR COMANDOS
        public CuentasViewModel ParentViewModel { get; set; }

        private string _tiempoTranscurrido;
        public string TiempoTranscurrido
        {
            get => _tiempoTranscurrido;
            set { _tiempoTranscurrido = value; OnPropertyChanged(); }
        }

        // ✅ NUEVA PROPIEDAD PARA CONTROLAR HABILITACIÓN DEL RADIOBUTTON
        private bool _isRadioButtonEnabled = true;
        public bool IsRadioButtonEnabled
        {
            get => _isRadioButtonEnabled;
            set
            {
                _isRadioButtonEnabled = value;
                OnPropertyChanged();
            }
        }
        
        // ✅ PROPIEDAD PARA RADIOBUTTON CON CONTROL INTELIGENTE
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                // Solo permitir cambios desde el comando o desde el código del ViewModel
                if (_isSettingFromCode)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
                else
                {
                    // Si viene desde la UI (RadioButton), ignorar y ejecutar comando
                    System.Diagnostics.Debug.WriteLine($"🚫 Click directo en RadioButton ignorado para cuenta {idCuenta}");
                }
            }
        }
        
        private bool _isSettingFromCode = false;
        
        // 🔄 MÉTODO PARA CAMBIAR IsSelected DESDE EL CÓDIGO
        public void SetIsSelectedFromCommand(bool value)
        {
            _isSettingFromCode = true;
            try
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    System.Diagnostics.Debug.WriteLine($"🔄 SetIsSelectedFromCommand: Cuenta {idCuenta} marcada como {value}");
                }
            }
            finally
            {
                _isSettingFromCode = false;
            }
        }

        // 🔒 NUEVA PROPIEDAD PARA MOSTRAR SI ESTÁ SIENDO EDITADA
        private bool _estaSiendoEditada;
        public bool EstaSiendoEditada
        {
            get => _estaSiendoEditada;
            set { _estaSiendoEditada = value; OnPropertyChanged(); }
        }

        private string _usuarioEditor;
        public string UsuarioEditor
        {
            get => _usuarioEditor;
            set { _usuarioEditor = value; OnPropertyChanged(); }
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