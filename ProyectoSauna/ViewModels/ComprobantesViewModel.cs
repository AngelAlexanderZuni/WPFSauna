using System.Collections.ObjectModel;
using System.Windows.Input;
using ProyectoSauna.Commands;
using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Repositories.Interfaces;
using ProyectoSauna.Services;
using ProyectoSauna.Services.Interfaces;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ProyectoSauna.ViewModels
{
    public class ComprobantesViewModel : INotifyPropertyChanged
    {
        private readonly IComprobanteService _comprobanteService;
        private readonly IDetalleConsumoRepository _detalleConsumoRepository;
        private readonly IDetalleServicioRepository _detalleServicioRepository;

        // INotifyPropertyChanged Implementation
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

        // State
        private bool _isBusy;
        private ObservableCollection<ComprobanteDTO> _comprobantes;
        private ComprobanteDTO? _selectedComprobante;
        private bool _isDetailsVisible;
        private ObservableCollection<DetallePagoDisplay> _detallesConsumo;

        // Commands
        public ICommand CargarComprobantesCommand { get; }
        public ICommand VerDetalleCommand { get; }
        public ICommand CerrarDetalleCommand { get; }
        public ICommand ImprimirCommand { get; }
        public ICommand VolverCommand { get; }

        public ComprobantesViewModel(
            IComprobanteService comprobanteService, 
            IDetalleConsumoRepository detalleConsumoRepository,
            IDetalleServicioRepository detalleServicioRepository)
        {
            _comprobanteService = comprobanteService;
            _detalleConsumoRepository = detalleConsumoRepository;
            _detalleServicioRepository = detalleServicioRepository;

            _comprobantes = new ObservableCollection<ComprobanteDTO>();
            _detallesConsumo = new ObservableCollection<DetallePagoDisplay>();

            CargarComprobantesCommand = new AsyncRelayCommand(async _ => await CargarComprobantesAsync());
            VerDetalleCommand = new AsyncRelayCommand(async _ => await VerDetalleAsync());
            CerrarDetalleCommand = new ProyectoSauna.Commands.RelayCommand(() => { IsDetailsVisible = false; });
            ImprimirCommand = new ProyectoSauna.Commands.RelayCommand(() => { ImprimirComprobante(); });
            VolverCommand = new ProyectoSauna.Commands.RelayCommand(() => { Volver(); });

            // Auto-load on instantiation or could be triggered by View
            // Task.Run(async () => await CargarComprobantesAsync()); 
        }

        // Properties
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public ObservableCollection<ComprobanteDTO> Comprobantes
        {
            get => _comprobantes;
            set => SetProperty(ref _comprobantes, value);
        }

        public ComprobanteDTO? SelectedComprobante
        {
            get => _selectedComprobante;
            set => SetProperty(ref _selectedComprobante, value);
        }

        public bool IsDetailsVisible
        {
            get => _isDetailsVisible;
            set => SetProperty(ref _isDetailsVisible, value);
        }

        public ObservableCollection<DetallePagoDisplay> DetallesConsumo
        {
            get => _detallesConsumo;
            set => SetProperty(ref _detallesConsumo, value);
        }

        // Methods
        private async Task CargarComprobantesAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var list = await _comprobanteService.GetAllAsync();
                
                Comprobantes.Clear();
                foreach (var item in list)
                {
                    Comprobantes.Add(item);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al cargar comprobantes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task VerDetalleAsync()
        {
            if (SelectedComprobante == null) return;

            try
            {
                IsBusy = true;
                DetallesConsumo.Clear();

                var productos = await _detalleConsumoRepository.GetByCuentaAsync(SelectedComprobante.idCuenta);
                foreach (var p in productos)
                {
                    DetallesConsumo.Add(new DetallePagoDisplay
                    {
                        Descripcion = p.idProductoNavigation?.nombre ?? "Producto desconocido",
                        Tipo = "Producto",
                        Cantidad = p.cantidad,
                        PrecioUnitario = p.precioUnitario,
                        Subtotal = p.subtotal
                    });
                }

                var servicios = await _detalleServicioRepository.GetByCuentaAsync(SelectedComprobante.idCuenta);
                foreach (var s in servicios)
                {
                    DetallesConsumo.Add(new DetallePagoDisplay
                    {
                        Descripcion = s.idServicioNavigation?.nombre ?? "Servicio desconocido",
                        Tipo = "Servicio",
                        Cantidad = s.cantidad,
                        PrecioUnitario = s.precioUnitario,
                        Subtotal = s.subtotal
                    });
                }

                IsDetailsVisible = true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al cargar detalles: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ImprimirComprobante()
        {
            if (SelectedComprobante == null) return;

            MessageBox.Show($"Imprimiendo comprobante {SelectedComprobante.serie}-{SelectedComprobante.numero}...\n(Funcionalidad de impresión real pendiente de implementación)", 
                "Imprimir", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Volver()
        {
             Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is MainWindow mainWin)
                    {
                        mainWin.CambiarAModulo("Pagos y Comprobantes");
                        return;
                    }
                }
            });
        }
    }
}
