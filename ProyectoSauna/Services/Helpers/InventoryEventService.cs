using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace ProyectoSauna.Services
{
    public class InventoryEventService
    {
        private static InventoryEventService _instance;
        private static readonly string MEMORY_MAP_NAME = "SaunaApp_Events";
        private static readonly string MUTEX_NAME = "SaunaApp_EventMutex";
        private MemoryMappedFile _mmf;
        private MemoryMappedViewAccessor _accessor;
        private Timer _checkTimer;
        private string _lastEventData = "";
        
        public static InventoryEventService Instance 
        { 
            get 
            {
                if (_instance == null) 
                    _instance = new InventoryEventService();
                return _instance;
            }
        }

        private InventoryEventService() 
        {
            InicializarComunicacionEntreProcesos();
        }
        
        private void InicializarComunicacionEntreProcesos()
        {
            try
            {
                // Crear o abrir memoria compartida
                try
                {
                    _mmf = MemoryMappedFile.CreateNew(MEMORY_MAP_NAME, 1024);
                }
                catch
                {
                    _mmf = MemoryMappedFile.OpenExisting(MEMORY_MAP_NAME);
                }
                
                _accessor = _mmf.CreateViewAccessor(0, 1024);
                
                // Timer para verificar cambios cada segundo
                _checkTimer = new Timer(VerificarEventos, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                
                System.Diagnostics.Debug.WriteLine("🔗 InventoryEventService inicializado para comunicación entre procesos");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error inicializando comunicación: {ex.Message}");
            }
        }

        public event EventHandler<StockChangedEventArgs> StockChanged;
        public static event EventHandler StockChangedLegacy; // Mantener compatibilidad

        public void OnStockChanged(StockChangedEventArgs args)
        {
            try
            {
                // Disparar evento local
                StockChanged?.Invoke(this, args);
                
                // Escribir a memoria compartida para otros procesos
                var eventData = $"{args.TipoMovimiento}|{args.IdCuenta}|{DateTime.Now.Ticks}";
                EscribirEventoAMemoriaCompartida(eventData);
                
                System.Diagnostics.Debug.WriteLine($"🗼 Evento enviado: {eventData}");
                
                // También disparar el evento legacy para compatibilidad
                StockChangedLegacy?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error enviando evento: {ex.Message}");
            }
        }

        // Alias para compatibilidad con diferentes nombres de método
        public void NotificarCambioStock(string tipoMovimiento, int idCuenta, string descripcion)
        {
            var args = new StockChangedEventArgs
            {
                TipoMovimiento = tipoMovimiento,
                IdCuenta = idCuenta,
                Descripcion = descripcion
            };
            OnStockChanged(args);
        }
        
        private void EscribirEventoAMemoriaCompartida(string eventData)
        {
            try
            {
                using (var mutex = new Mutex(false, MUTEX_NAME))
                {
                    if (mutex.WaitOne(1000))
                    {
                        var bytes = Encoding.UTF8.GetBytes(eventData);
                        _accessor?.WriteArray(0, bytes, 0, Math.Min(bytes.Length, 1000));
                        _accessor?.Write(1000, bytes.Length); // Escribir la longitud al final
                        mutex.ReleaseMutex();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error escribiendo a memoria: {ex.Message}");
            }
        }
        
        private void VerificarEventos(object state)
        {
            try
            {
                using (var mutex = new Mutex(false, MUTEX_NAME))
                {
                    if (mutex.WaitOne(100))
                    {
                        var length = _accessor?.ReadInt32(1000) ?? 0;
                        if (length > 0 && length < 1000)
                        {
                            var bytes = new byte[length];
                            _accessor?.ReadArray(0, bytes, 0, length);
                            var eventData = Encoding.UTF8.GetString(bytes);
                            
                            if (eventData != _lastEventData && !string.IsNullOrEmpty(eventData))
                            {
                                _lastEventData = eventData;
                                ProcesarEventoRecibido(eventData);
                            }
                        }
                        mutex.ReleaseMutex();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error verificando eventos: {ex.Message}");
            }
        }
        
        private void ProcesarEventoRecibido(string eventData)
        {
            try
            {
                var parts = eventData.Split('|');
                if (parts.Length >= 3)
                {
                    var tipoMovimiento = parts[0];
                    var idCuenta = string.IsNullOrEmpty(parts[1]) ? (int?)null : int.Parse(parts[1]);
                    
                    System.Diagnostics.Debug.WriteLine($"📨 Evento recibido: {tipoMovimiento}, Cuenta: {idCuenta}");
                    
                    var args = new StockChangedEventArgs
                    {
                        TipoMovimiento = tipoMovimiento,
                        IdCuenta = idCuenta,
                        ProductoId = 0,
                        NuevoStock = 0
                    };
                    
                    // Disparar evento local
                    StockChanged?.Invoke(this, args);
                    StockChangedLegacy?.Invoke(null, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error procesando evento: {ex.Message}");
            }
        }

        public static void NotifyStockChanged()
        {
            StockChangedLegacy?.Invoke(null, EventArgs.Empty);
        }
        
        public void Dispose()
        {
            try
            {
                _checkTimer?.Dispose();
                _accessor?.Dispose();
                _mmf?.Dispose();
            }
            catch { }
        }
    }

    public class StockChangedEventArgs : EventArgs
    {
        public int ProductoId { get; set; }
        public int NuevoStock { get; set; }
        public string TipoMovimiento { get; set; } = string.Empty;
        public int? IdCuenta { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}