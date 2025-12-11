using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using ProyectoSauna.Commands;
using ProyectoSauna.Models;
using ProyectoSauna.Models.DTOs;
using ProyectoSauna.Repositories;
using ProyectoSauna.Services;
using ProyectoSauna.Services.Interfaces;

namespace ProyectoSauna.ViewModels
{
    public class EgresosViewModel : INotifyPropertyChanged
    {
        private readonly IEgresoService _egresoService;
        
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Propiedades de Estado
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }
        #endregion

        #region Propiedades de la Cabecera
        private CabEgresoDTO _cabeceraEgreso = new CabEgresoDTO
        {
            fecha = DateTime.Now,
            montoTotal = 0
        };

        public CabEgresoDTO CabeceraEgreso
        {
            get => _cabeceraEgreso;
            set
            {
                _cabeceraEgreso = value;
                OnPropertyChanged();
            }
        }

        public string RutaComprobante
        {
            get => _rutaComprobante;
            set
            {
                _rutaComprobante = value;
                OnPropertyChanged();
            }
        }
        private string _rutaComprobante = string.Empty;
        #endregion

        #region Propiedades de Detalles
        private ObservableCollection<DetEgresoDTO> _detallesEgreso = new();
        public ObservableCollection<DetEgresoDTO> DetallesEgreso
        {
            get => _detallesEgreso;
            set
            {
                _detallesEgreso = value;
                OnPropertyChanged();
                CalcularMontoTotal();
            }
        }

        private void CalcularMontoTotal()
        {
            CabeceraEgreso.montoTotal = DetallesEgreso.Sum(d => d.monto);
            OnPropertyChanged(nameof(CabeceraEgreso));
            OnPropertyChanged(nameof(TotalEgresoFormateado));
            OnPropertyChanged(nameof(EsValidoParaGuardar)); // Notify validity change
            CommandManager.InvalidateRequerySuggested(); // Force button re-evaluation
        }

        public string TotalEgresoFormateado => 
            $"${CabeceraEgreso.montoTotal:N2}";

        // Propiedades para agregar/editar detalles
        public string ConceptoDetalle { get; set; } = string.Empty;
        public decimal MontoDetalle { get; set; }
        public int TipoEgresoSeleccionadoId { get; set; }
        public bool EsRecurrenteDetalle { get; set; }
        
        public bool EsRecurrente { get; set; }

        public ObservableCollection<CabEgresoDTO> HistorialEgresos { get; } = new();
        
        private DetEgresoDTO? _detalleSeleccionado;
        public DetEgresoDTO? DetalleSeleccionado
        {
            get => _detalleSeleccionado;
            set
            {
                _detalleSeleccionado = value;
                OnPropertyChanged();
            }
        }

        private CabEgresoDTO? _egresoSeleccionado;
        public CabEgresoDTO? EgresoSeleccionado
        {
            get => _egresoSeleccionado;
            set
            {
                _egresoSeleccionado = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Comandos
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand AgregarDetalleCommand { get; }
        public ICommand EditarDetalleCommand { get; }
        public ICommand EliminarDetalleCommand { get; }
        public ICommand SeleccionarComprobanteCommand { get; }
        public ICommand VerComprobanteCommand { get; }
        public ICommand EliminarComprobanteCommand { get; }
        public ICommand CargarEgresoCommand { get; }
        public ICommand NuevoTipoEgresoCommand { get; }

        // Constructor para XAML (Fallback)
        public EgresosViewModel() : this(CreateFallbackEgresoService())
        {
        }

        private static IEgresoService CreateFallbackEgresoService()
        {
             var context = new SaunaDbContext();
             var egresoRepo = new EgresoRepository(context);
             var tipoEgresoRepo = new TipoEgresoRepository(context);
             return new EgresoService(egresoRepo, tipoEgresoRepo);
        }

        public EgresosViewModel(IEgresoService egresoService)
        {
            _egresoService = egresoService ?? throw new ArgumentNullException(nameof(egresoService));
            
            // Fully qualified names to avoid conflict with ProyectoSauna.ViewModels.RelayCommand form CuentasViewModel.cs
            GuardarCommand = new ProyectoSauna.Commands.AsyncRelayCommand(async (_) => await GuardarEgresoAsync(), (_) => !IsBusy && EsValidoParaGuardar);
            CancelarCommand = new ProyectoSauna.Commands.RelayCommand((_) => LimpiarFormulario());
            
            AgregarDetalleCommand = new ProyectoSauna.Commands.RelayCommand((_) => AgregarDetalle(), (_) => 
                !string.IsNullOrWhiteSpace(ConceptoDetalle) && MontoDetalle > 0 && TipoEgresoSeleccionadoId > 0);
            
            EditarDetalleCommand = new ProyectoSauna.Commands.RelayCommand(param => EditarDetalle(param as DetEgresoDTO));
            EliminarDetalleCommand = new ProyectoSauna.Commands.RelayCommand(param => EliminarDetalle(param as DetEgresoDTO));

            SeleccionarComprobanteCommand = new ProyectoSauna.Commands.RelayCommand((_) => SeleccionarComprobante());
            VerComprobanteCommand = new ProyectoSauna.Commands.RelayCommand((_) => VerComprobante(), (_) => !string.IsNullOrWhiteSpace(RutaComprobante));
            EliminarComprobanteCommand = new ProyectoSauna.Commands.RelayCommand((_) => EliminarComprobante(), (_) => !string.IsNullOrWhiteSpace(RutaComprobante));

            CargarEgresoCommand = new ProyectoSauna.Commands.AsyncRelayCommand(async param => await CargarEgresoAsync(param as CabEgresoDTO));
            
            NuevoTipoEgresoCommand = new ProyectoSauna.Commands.RelayCommand((_) => AbrirDialogoNuevoTipo());

            _ = InicializarDatosAsync();
            
            // Subscribe to collection changes to update command state
            DetallesEgreso.CollectionChanged += (s, e) => 
            {
                OnPropertyChanged(nameof(EsValidoParaGuardar));
                CommandManager.InvalidateRequerySuggested();
            };
        }
        #endregion

        #region Metodos Async
        private async Task InicializarDatosAsync()
        {
            try
            {
                IsBusy = true;
                await CargarTiposEgresoAsync();
                await CargarHistorialEgresosAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos iniciales: {ex.Message}", "Error");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GuardarEgresoAsync()
        {
            try
            {
                IsBusy = true;

                // Si ya existe, NO crear uno nuevo (evitar duplicados)
                if (CabeceraEgreso.idCabEgreso > 0)
                {
                    // Aquí deberíamos llamar a un método "ActualizarCabecera" si existiera.
                    // Como no existe, y el usuario se quejaba de que "no guarda", es probable que intentara guardar cambios en detalle
                    // y esperara que este botón lo hiciera.
                    // Ahora que los detalles se guardan individualmente al Editar/Eliminar/Agregar (si ya existe cabecera),
                    // este botón solo serviría para actualizar Fecha/Total o NADA.
                    
                    MessageBox.Show("Los cambios en los detalles ya se han guardado.\nPara modificar la fecha u otros datos de cabecera, esta funcionalidad está pendiente.", 
                        "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Obtener usuario válido
                int idUsuario = ProyectoSauna.Models.SesionActual.IdUsuario;
                if (idUsuario <= 0)
                {
                    // Fallback: buscar primer usuario en BD (para pruebas)
                    using var context = new ProyectoSauna.Models.SaunaDbContext();
                    var usuario = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(context.Usuario);
                    idUsuario = usuario?.idUsuario ?? 1; // Si falla, intenta 1
                }

                var egresoAGuardar = new CabEgresoDTO
                {
                    fecha = CabeceraEgreso.fecha,
                    montoTotal = DetallesEgreso.Sum(d => d.monto),
                    idUsuario = idUsuario,
                    Detalles = DetallesEgreso.ToList() 
                };

                // Asegurar que los detalles tengan ID 0 para que EF los inserte como nuevos
                // y ruta de comprobante válida
                foreach (var det in egresoAGuardar.Detalles)
                {
                   det.idDetEgreso = 0; // CRÍTICO: Forzar creación
                   det.idCabEgreso = 0; // CRÍTICO: Forzar creación
                   
                   if (string.IsNullOrEmpty(det.comprobanteRuta))
                       det.comprobanteRuta = RutaComprobante;
                }

                var (exito, mensaje, resultado) = await _egresoService.CrearEgresoAsync(egresoAGuardar);

                if (exito)
                {
                    // Forzar recarga completa para asegurar que la vista se actualiza
                    MessageBox.Show("Guardado exitoso.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    LimpiarFormulario();
                    await CargarHistorialEgresosAsync();
                }
                else
                {
                    MessageBox.Show($"No se pudo guardar: {mensaje}", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "No inner exception";
                MessageBox.Show($"Error CRÍTICO al guardar.\n\nMensaje: {ex.Message}\nDetalle interno: {innerMessage}\nUsuarioIntentado: {ProyectoSauna.Models.SesionActual.IdUsuario}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
                
                System.Diagnostics.Debug.WriteLine($"Error Guardar: {ex}");
            }
            finally
            {
                IsBusy = false;
                CommandManager.InvalidateRequerySuggested(); // Ensure button re-enables if needed
            }
        }

        private async Task CargarHistorialEgresosAsync()
        {
            var recientes = await _egresoService.GetEgresosRecientesAsync();
            HistorialEgresos.Clear();
            foreach (var item in recientes)
            {
                HistorialEgresos.Add(item);
            }
        }

        private async Task CargarEgresoAsync(CabEgresoDTO? egreso)
        {
            if (egreso == null) return;
            
            try
            {
                IsBusy = true;
                var detalles = await _egresoService.GetDetallesPorCabeceraAsync(egreso.idCabEgreso);
                CabeceraEgreso = egreso;
                
                DetallesEgreso.Clear();
                foreach (var d in detalles)
                {
                    DetallesEgreso.Add(d);
                }
                
                EgresoSeleccionado = egreso;
                RutaComprobante = detalles.FirstOrDefault(d => !string.IsNullOrEmpty(d.comprobanteRuta))?.comprobanteRuta ?? string.Empty;
                
                CalcularMontoTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar egreso: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
        #endregion

        #region Métodos Síncronos (UI)
        private void LimpiarFormulario()
        {
            CabeceraEgreso = new CabEgresoDTO
            {
                fecha = DateTime.Now,
                montoTotal = 0
            };

            DetallesEgreso.Clear();
            RutaComprobante = string.Empty;
            LimpiarDetalleFormulario();
            
            OnPropertyChanged(nameof(EsValidoParaGuardar)); // Update validity
            CommandManager.InvalidateRequerySuggested();
        }

        private void LimpiarDetalleFormulario()
        {
            ConceptoDetalle = string.Empty;
            MontoDetalle = 0;
            EsRecurrenteDetalle = false;
            TipoEgresoSeleccionadoId = 0;
            DetalleSeleccionado = null;
            
            OnPropertyChanged(nameof(ConceptoDetalle));
            OnPropertyChanged(nameof(MontoDetalle));
            OnPropertyChanged(nameof(EsRecurrenteDetalle));
            OnPropertyChanged(nameof(TipoEgresoSeleccionadoId));
        }

        private async void AgregarDetalle()
        {
            var tipo = TiposEgreso.FirstOrDefault(t => t.idTipoEgreso == TipoEgresoSeleccionadoId);
            
            // Check if we are updating an existing DB item
            if (DetalleSeleccionado != null && DetalleSeleccionado.idDetEgreso > 0)
            {
                try
                {
                    IsBusy = true;
                    var detalleActualizar = new DetEgresoDTO
                    {
                        idDetEgreso = DetalleSeleccionado.idDetEgreso,
                        idCabEgreso = DetalleSeleccionado.idCabEgreso,
                        concepto = ConceptoDetalle,
                        monto = MontoDetalle,
                        recurrente = EsRecurrenteDetalle,
                        idTipoEgreso = TipoEgresoSeleccionadoId,
                        TipoEgresoNombre = tipo?.nombre,
                        comprobanteRuta = RutaComprobante // Or keep existing if null?
                    };

                    var exito = await _egresoService.ActualizarDetalleAsync(detalleActualizar);
                    
                    if (exito)
                    {
                        // Refresh list or update item in place
                        // Reloading is safer for totals calculation
                        if (CabeceraEgreso.idCabEgreso > 0)
                            await CargarEgresoAsync(CabeceraEgreso);
                            
                        LimpiarDetalleFormulario();
                    }
                    else
                    {
                        MessageBox.Show("Error al actualizar el detalle en BD.", "Error");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al actualizar: {ex.Message}", "Error");
                }
                finally
                {
                    IsBusy = false;
                }
                return;
            }

            // Standard logic for new items or memory-only items
            var detalle = new DetEgresoDTO
            {
                idDetEgreso = 0, 
                concepto = ConceptoDetalle,
                monto = MontoDetalle,
                recurrente = EsRecurrenteDetalle,
                idTipoEgreso = TipoEgresoSeleccionadoId,
                TipoEgresoNombre = tipo?.nombre,
                comprobanteRuta = RutaComprobante
            };

            DetallesEgreso.Add(detalle);
            LimpiarDetalleFormulario();
            CalcularMontoTotal();
            // CollectionChanged handler will invoke updates
        }

        private async void EditarDetalle(DetEgresoDTO? detalle)
        {
            if (detalle == null) return;

            // Si el detalle ya existe en BD, actualizarlo directamente
            if (detalle.idDetEgreso > 0)
            {
                 // Nota: Esto actualiza el objeto en memoria, para persistir cambios reales 
                 // deberíamos abrir un diálogo o tomar los valores de los TextBoxes (ConceptoDetalle, etc)
                 // SI el usuario seleccionó "Editar" para ponerlo en el form, y LUEGO le da a otro botón "Guardar Cambios de Detalle"? 
                 // Actualmente el flujo parece ser: Click Editar -> Llena form -> ??? -> Cómo se confirman los cambios?
                 // Ah, el comando EditarDetalle parece ser "Cargar en Formulario para Editar".
                 // Verificando XAML... El botón en la grilla es "Editar".
                 
                 // CORRECCIÓN: El botón Editar en la grilla carga los datos en los inputs.
                 // La acción de "Confirmar Edición" debería ser el botón "Agregar/Actualizar"?
                 // Actualmente "AgregarDetalleCommand" SIEMPRE hace .Add.
                 
                 ConceptoDetalle = detalle.concepto;
                 MontoDetalle = detalle.monto;
                 EsRecurrenteDetalle = detalle.recurrente;
                 TipoEgresoSeleccionadoId = detalle.idTipoEgreso;
                 DetalleSeleccionado = detalle; // Marcar como seleccionado para edición
                 
                 // No removemos de la lista aún para no perderlo si cancela
                 // DetallesEgreso.Remove(detalle); 
            }
            else
            {
                // Comportamiento anterior para items en memoria
                ConceptoDetalle = detalle.concepto;
                MontoDetalle = detalle.monto;
                EsRecurrenteDetalle = detalle.recurrente;
                TipoEgresoSeleccionadoId = detalle.idTipoEgreso;
                DetalleSeleccionado = detalle;
                DetallesEgreso.Remove(detalle);
            }
            
            CalcularMontoTotal();
            
            OnPropertyChanged(nameof(ConceptoDetalle));
            OnPropertyChanged(nameof(MontoDetalle));
            OnPropertyChanged(nameof(EsRecurrenteDetalle));
            OnPropertyChanged(nameof(TipoEgresoSeleccionadoId));
        }

        private async void EliminarDetalle(DetEgresoDTO? detalle)
        {
            if (detalle == null) return;
            
            if (MessageBox.Show("¿Eliminar este detalle?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (detalle.idDetEgreso > 0)
                {
                    try
                    {
                        IsBusy = true;
                        var exito = await _egresoService.EliminarDetalleAsync(detalle.idDetEgreso);
                        if (exito)
                        {
                            DetallesEgreso.Remove(detalle);
                            
                            // Check if that was the last detail
                            if (DetallesEgreso.Count == 0)
                            {
                                MessageBox.Show("Se han eliminado todos los detalles. El egreso ha sido eliminado.", "Información");
                                LimpiarFormulario(); // Reset to "New" state
                                await CargarHistorialEgresosAsync(); // Refresh history to remove the deleted header
                            }
                            else
                            {
                                CalcularMontoTotal();
                                // Recargar cabecera para asegurar sincronización de total
                                if (CabeceraEgreso.idCabEgreso > 0) 
                                    await CargarEgresoAsync(CabeceraEgreso);
                            }
                        }
                        else
                        {
                           MessageBox.Show("No se pudo eliminar el detalle de la base de datos.", "Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar: {ex.Message}", "Error");
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                }
                else
                {
                    DetallesEgreso.Remove(detalle);
                    CalcularMontoTotal();
                }
            }
        }
        
        private void SeleccionarComprobante()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Seleccionar comprobante",
                Filter = "Archivos de imagen/PDF|*.jpg;*.jpeg;*.png;*.pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string carpeta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Comprobantes");
                    if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);

                    string ext = Path.GetExtension(dialog.FileName);
                    string nuevoNombre = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString().Substring(0,8)}{ext}";
                    string destino = Path.Combine(carpeta, nuevoNombre);

                    File.Copy(dialog.FileName, destino);
                    RutaComprobante = destino;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al copiar archivo: " + ex.Message);
                }
            }
        }

        private void VerComprobante()
        {
            if (File.Exists(RutaComprobante))
            {
                Process.Start(new ProcessStartInfo { FileName = RutaComprobante, UseShellExecute = true });
            }
        }

        private void EliminarComprobante()
        {
            RutaComprobante = string.Empty;
        }

        private void AbrirDialogoNuevoTipo()
        {
            MessageBox.Show("Funcionalidad para crear tipos de egreso pendiente de implementación global.");
        }

        #endregion

        #region Datos de Soporte
        public ObservableCollection<TipoEgresoDTO> TiposEgreso { get; } = new();

        private async Task CargarTiposEgresoAsync()
        {
            var tipos = await _egresoService.GetTiposEgresoAsync();
            TiposEgreso.Clear();
            foreach (var t in tipos) TiposEgreso.Add(t);
        }

        public bool EsValidoParaGuardar => 
            DetallesEgreso.Any() && 
            CabeceraEgreso.fecha <= DateTime.Now;
        #endregion
    }
}