// Services/ClienteEnEdicionService.cs
using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace ProyectoSauna.Services
{
    /// <summary>
    /// 🔒 Servicio para controlar la edición simultánea de clientes
    /// Previene que múltiples usuarios editen el mismo cliente al mismo tiempo
    /// Utiliza MemoryMappedFiles para comunicación entre procesos
    /// </summary>
    public class ClienteEnEdicionService : IDisposable
    {
        private readonly string _memoryMapName = "SaunaClientesEnEdicion";
        private readonly MemoryMappedFile _memoryMappedFile;
        private readonly MemoryMappedViewAccessor _accessor;
        private readonly Mutex _mutex;
        private readonly ConcurrentDictionary<int, string> _clientesEnEdicion = new();
        private readonly Timer _limpiezaTimer;
        private bool _disposed = false;

        public ClienteEnEdicionService()
        {
            try
            {
                // Crear memoria compartida para almacenar clientes en edición
                _memoryMappedFile = MemoryMappedFile.CreateOrOpen(_memoryMapName, 4096);
                _accessor = _memoryMappedFile.CreateViewAccessor(0, 4096);
                _mutex = new Mutex(false, "SaunaClientesEnEdicionMutex");

                // Timer para limpiar entradas antiguas cada 30 segundos
                _limpiezaTimer = new Timer(LimpiarEntradasAntiguas, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error inicializando ClienteEnEdicionService: {ex.Message}");
            }
        }

        /// <summary>
        /// 🔒 Intenta bloquear un cliente para edición
        /// </summary>
        /// <param name="idCliente">ID del cliente</param>
        /// <param name="usuarioEditor">Nombre del usuario que está editando</param>
        /// <returns>True si se pudo bloquear, False si ya está siendo editado</returns>
        public (bool exito, string mensaje, string usuarioEditor) IntentarBloquearCliente(int idCliente, string usuarioEditor)
        {
            try
            {
                _mutex?.WaitOne();

                // Verificar en memoria local
                if (_clientesEnEdicion.ContainsKey(idCliente))
                {
                    var editor = _clientesEnEdicion[idCliente];
                    return (false, $"El cliente ya está siendo editado por {editor}", editor);
                }

                // Verificar en memoria compartida
                var clienteEnEdicion = LeerClienteEnMemoria(idCliente);
                if (clienteEnEdicion.HasValue)
                {
                    return (false, $"El cliente ya está siendo editado por {clienteEnEdicion.Value.usuario}", clienteEnEdicion.Value.usuario);
                }

                // Bloquear cliente
                var timestamp = DateTime.Now.Ticks;
                _clientesEnEdicion[idCliente] = usuarioEditor;
                EscribirClienteEnMemoria(idCliente, usuarioEditor, timestamp);

                return (true, "Cliente bloqueado para edición", usuarioEditor);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error bloqueando cliente {idCliente}: {ex.Message}");
                return (false, "Error interno al bloquear cliente", "");
            }
            finally
            {
                try { _mutex?.ReleaseMutex(); } catch { }
            }
        }

        /// <summary>
        /// 🔓 Libera el bloqueo de un cliente
        /// </summary>
        /// <param name="idCliente">ID del cliente</param>
        /// <param name="usuarioEditor">Usuario que está liberando el bloqueo</param>
        public void LiberarBloqueoCliente(int idCliente, string usuarioEditor)
        {
            try
            {
                _mutex?.WaitOne();

                // Remover de memoria local
                if (_clientesEnEdicion.TryGetValue(idCliente, out var editorActual) && editorActual == usuarioEditor)
                {
                    _clientesEnEdicion.TryRemove(idCliente, out _);
                }

                // Remover de memoria compartida
                RemoverClienteDeMemoria(idCliente, usuarioEditor);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error liberando cliente {idCliente}: {ex.Message}");
            }
            finally
            {
                try { _mutex?.ReleaseMutex(); } catch { }
            }
        }

        /// <summary>
        /// 🔍 Verifica si un cliente está siendo editado
        /// </summary>
        public (bool enEdicion, string usuarioEditor) VerificarClienteEnEdicion(int idCliente)
        {
            try
            {
                _mutex?.WaitOne();

                // Verificar en memoria local
                if (_clientesEnEdicion.TryGetValue(idCliente, out var editor))
                {
                    return (true, editor);
                }

                // Verificar en memoria compartida
                var clienteEnEdicion = LeerClienteEnMemoria(idCliente);
                if (clienteEnEdicion.HasValue)
                {
                    return (true, clienteEnEdicion.Value.usuario);
                }

                return (false, "");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error verificando cliente {idCliente}: {ex.Message}");
                return (false, "");
            }
            finally
            {
                try { _mutex?.ReleaseMutex(); } catch { }
            }
        }

        #region Gestión de Memoria Compartida

        private (string usuario, long timestamp)? LeerClienteEnMemoria(int idCliente)
        {
            try
            {
                if (_accessor == null) return null;

                var data = new byte[4096];
                _accessor.ReadArray(0, data, 0, 4096);
                var content = Encoding.UTF8.GetString(data).TrimEnd('\0');
                
                if (string.IsNullOrEmpty(content)) return null;

                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 3 && 
                        int.TryParse(parts[0], out var id) && 
                        id == idCliente &&
                        long.TryParse(parts[2], out var timestamp))
                    {
                        return (parts[1], timestamp);
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private void EscribirClienteEnMemoria(int idCliente, string usuario, long timestamp)
        {
            try
            {
                if (_accessor == null) return;

                var data = new byte[4096];
                _accessor.ReadArray(0, data, 0, 4096);
                var content = Encoding.UTF8.GetString(data).TrimEnd('\0');
                
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
                
                // Remover entrada anterior del mismo cliente
                lines.RemoveAll(line => line.StartsWith($"{idCliente}|"));
                
                // Agregar nueva entrada
                lines.Add($"{idCliente}|{usuario}|{timestamp}");
                
                // Escribir de vuelta a memoria
                var newContent = string.Join('\n', lines);
                var newData = Encoding.UTF8.GetBytes(newContent);
                
                if (newData.Length < 4096)
                {
                    Array.Clear(data, 0, data.Length);
                    Array.Copy(newData, data, newData.Length);
                    _accessor.WriteArray(0, data, 0, 4096);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error escribiendo cliente en memoria: {ex.Message}");
            }
        }

        private void RemoverClienteDeMemoria(int idCliente, string usuarioValidar)
        {
            try
            {
                if (_accessor == null) return;

                var data = new byte[4096];
                _accessor.ReadArray(0, data, 0, 4096);
                var content = Encoding.UTF8.GetString(data).TrimEnd('\0');
                
                if (string.IsNullOrEmpty(content)) return;

                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
                
                // Remover solo si el usuario coincide
                lines.RemoveAll(line => 
                {
                    var parts = line.Split('|');
                    return parts.Length >= 2 && 
                           int.TryParse(parts[0], out var id) && 
                           id == idCliente && 
                           parts[1] == usuarioValidar;
                });
                
                // Escribir de vuelta
                var newContent = string.Join('\n', lines);
                var newData = Encoding.UTF8.GetBytes(newContent);
                
                Array.Clear(data, 0, data.Length);
                if (newData.Length < 4096)
                {
                    Array.Copy(newData, data, newData.Length);
                }
                _accessor.WriteArray(0, data, 0, 4096);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error removiendo cliente de memoria: {ex.Message}");
            }
        }

        private void LimpiarEntradasAntiguas(object state)
        {
            try
            {
                _mutex?.WaitOne();

                if (_accessor == null) return;

                var ahora = DateTime.Now.Ticks;
                var tiempoLimite = TimeSpan.FromMinutes(10).Ticks; // 10 minutos

                var data = new byte[4096];
                _accessor.ReadArray(0, data, 0, 4096);
                var content = Encoding.UTF8.GetString(data).TrimEnd('\0');
                
                if (string.IsNullOrEmpty(content)) return;

                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
                var lineasValidas = new List<string>();
                
                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 3 && 
                        long.TryParse(parts[2], out var timestamp) &&
                        (ahora - timestamp) < tiempoLimite)
                    {
                        lineasValidas.Add(line);
                    }
                }
                
                // Escribir solo las líneas válidas
                var newContent = string.Join('\n', lineasValidas);
                var newData = Encoding.UTF8.GetBytes(newContent);
                
                Array.Clear(data, 0, data.Length);
                if (newData.Length < 4096)
                {
                    Array.Copy(newData, data, newData.Length);
                }
                _accessor.WriteArray(0, data, 0, 4096);

                // Limpiar también la memoria local
                var clientesARemover = _clientesEnEdicion.Keys.ToList();
                foreach (var idCliente in clientesARemover)
                {
                    if (!lineasValidas.Any(l => l.StartsWith($"{idCliente}|")))
                    {
                        _clientesEnEdicion.TryRemove(idCliente, out _);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"🧹 Limpieza completada. Entradas activas: {lineasValidas.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en limpieza automática: {ex.Message}");
            }
            finally
            {
                try { _mutex?.ReleaseMutex(); } catch { }
            }
        }

        #endregion

        public void Dispose()
        {
            if (!_disposed)
            {
                _limpiezaTimer?.Dispose();
                _accessor?.Dispose();
                _memoryMappedFile?.Dispose();
                _mutex?.Dispose();
                _disposed = true;
            }
        }
    }
}