using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Models; // For SesionActual
using ProyectoSauna.Services;
using ProyectoSauna.Repositories;
using ProyectoSauna.Repositories.Interfaces;
using ProyectoSauna.Commands;

using System.ComponentModel; // Required
using System.Runtime.CompilerServices; // Required

namespace ProyectoSauna.ViewModels
{
    public class PagosViewModel : INotifyPropertyChanged
    {
        private readonly PagoService _pagoService;
        private readonly MetodoPagoService _metodoPagoService;
        private readonly ICuentaRepository _cuentaRepository;
        private readonly IDetalleConsumoRepository _detalleConsumoRepository;
        private readonly IDetalleServicioRepository _detalleServicioRepository;

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        // State
        private bool _isBusy;
        private CuentaDTO? _cuentaActual;
        private ObservableCollection<MetodoPagoDTO> _metodosPago;
        private ObservableCollection<DetallePagoDisplay> _detallesCuenta;
        private MetodoPagoDTO? _metodoPagoSeleccionado;
        private string _totalPagarDisplay;

        // Commands
        public ICommand ProcesarPagoCommand { get; }
        public ICommand VolverCommand { get; }
        public ICommand CargarDatosCommand { get; }

        public PagosViewModel(
            PagoService pagoService, 
            MetodoPagoService metodoPagoService,
            ICuentaRepository cuentaRepository,
            IDetalleConsumoRepository detalleConsumoRepository,
            IDetalleServicioRepository detalleServicioRepository)
        {
            _pagoService = pagoService;
            _metodoPagoService = metodoPagoService;
            _cuentaRepository = cuentaRepository;
            _detalleConsumoRepository = detalleConsumoRepository;
            _detalleServicioRepository = detalleServicioRepository;

            _metodosPago = new ObservableCollection<MetodoPagoDTO>();
            _detallesCuenta = new ObservableCollection<DetallePagoDisplay>();
            _totalPagarDisplay = "S/ 0.00"; // Initial placeholder

            // Commands
            // Using explicit lambdas to avoid method group conversion issues
            ProcesarPagoCommand = new ProyectoSauna.Commands.AsyncRelayCommand(async (o) => await ProcesarPagoAsync(o), CanProcesarPago);
            VolverCommand = new ProyectoSauna.Commands.RelayCommand(Volver);
            
            // Wrapper for CargarDatosCommand which expects an int parameter
            CargarDatosCommand = new ProyectoSauna.Commands.AsyncRelayCommand(async (object? param) => 
            {
                if (param is int id)
                    await CargarDatosAsync(id);
                else if (param != null && int.TryParse(param.ToString(), out int parsedId))
                    await CargarDatosAsync(parsedId);
            });
        }

        // Properties
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public CuentaDTO? CuentaActual
        {
            get => _cuentaActual;
            set => SetProperty(ref _cuentaActual, value);
        }

        public ObservableCollection<MetodoPagoDTO> MetodosPago
        {
            get => _metodosPago;
            set => SetProperty(ref _metodosPago, value);
        }

        public ObservableCollection<DetallePagoDisplay> DetallesCuenta
        {
            get => _detallesCuenta;
            set => SetProperty(ref _detallesCuenta, value);
        }

        public MetodoPagoDTO? MetodoPagoSeleccionado
        {
            get => _metodoPagoSeleccionado;
            set
            {
                if (SetProperty(ref _metodoPagoSeleccionado, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string TotalPagarDisplay
        {
            get => _totalPagarDisplay;
            set => SetProperty(ref _totalPagarDisplay, value);
        }

        // Methods
        public async Task CargarDatosAsync(int idCuenta)
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                
                // 1. Load Account
                var cuentaEntity = await _cuentaRepository.GetCuentaByIdAsync(idCuenta);
                
                if (cuentaEntity == null)
                {
                    MessageBox.Show("No se encontró la cuenta especificada.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Volver(null);
                    return;
                }

                // Map to DTO
                CuentaActual = new CuentaDTO 
                { 
                    idCuenta = cuentaEntity.idCuenta,
                    NombreCliente = (cuentaEntity.idClienteNavigation?.nombre ?? "") + " " + (cuentaEntity.idClienteNavigation?.apellidos ?? ""),
                    DocumentoCliente = cuentaEntity.idClienteNavigation?.numero_documento ?? "",
                    total = cuentaEntity.total,
                    descuento = cuentaEntity.descuento,
                    // Map other fields if needed by DTO
                    idCliente = cuentaEntity.idCliente, // Removed ?? 0 as it is non-nullable int
                    idEstadoCuenta = cuentaEntity.idEstadoCuenta
                };
                
                TotalPagarDisplay = $"S/ {CuentaActual.total:N2}";

                
                // 2. Load Payment Methods
                var metodos = await _metodoPagoService.GetMetodosAsync();
                
                 MetodosPago.Clear();
                 foreach(var m in metodos)
                 {
                     MetodosPago.Add(m);
                 }
                
                // Default selection
                MetodoPagoSeleccionado = MetodosPago.FirstOrDefault();

                // 3. Load Details (Products and Services)
                DetallesCuenta.Clear();

                var productos = await _detalleConsumoRepository.GetByCuentaAsync(idCuenta);
                foreach(var p in productos)
                {
                    DetallesCuenta.Add(new DetallePagoDisplay
                    {
                        Descripcion = p.idProductoNavigation?.nombre ?? "Producto desconocido",
                        Tipo = "Producto",
                        Cantidad = p.cantidad,
                        PrecioUnitario = p.precioUnitario,
                        Subtotal = p.subtotal
                    });
                }

                var servicios = await _detalleServicioRepository.GetByCuentaAsync(idCuenta);
                foreach(var s in servicios)
                {
                    DetallesCuenta.Add(new DetallePagoDisplay
                    {
                        Descripcion = s.idServicioNavigation?.nombre ?? "Servicio desconocido",
                        Tipo = "Servicio",
                        Cantidad = s.cantidad,
                        PrecioUnitario = s.precioUnitario,
                        Subtotal = s.subtotal
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanProcesarPago(object? parameter)
        {
            return !IsBusy && CuentaActual != null && MetodoPagoSeleccionado != null;
        }

        private async Task ProcesarPagoAsync(object? parameter)
        {
            if (CuentaActual == null || MetodoPagoSeleccionado == null) return;

            // Confirmación Simple
            if (MessageBox.Show($"¿Confirmar pago de {TotalPagarDisplay} con {MetodoPagoSeleccionado.nombre}?", 
                "Confirmar Pago", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                IsBusy = true;

                var nuevoPago = new PagoDTO
                {
                    idCuenta = CuentaActual.idCuenta,
                    monto = CuentaActual.total,
                    idMetodoPago = MetodoPagoSeleccionado.idMetodoPago,
                    fechaHora = DateTime.Now,
                    // Optional reference number if UI has a textbox for it
                };

                // Transaction handled by Service
                var result = await _pagoService.CrearPagoAsync(nuevoPago);

                if (result.exito)
                {
                    MessageBox.Show("Pago registrado correctamente. Comprobante generado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Navigation logic to return
                    Volver(true); // Signal success
                }
                else
                {
                     MessageBox.Show($"Hubo un problema al registrar el pago: {result.mensaje}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                 MessageBox.Show($"Error critico procesando pago: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Volver(object? parameter)
        {
            // Navigate back to Cuentas y Consumos
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is MainWindow mainWin)
                    {
                        mainWin.CambiarAModulo("Cuentas y Consumos");
                        return;
                    }
                }
            });
        }
    }
}
