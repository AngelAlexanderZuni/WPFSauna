// Services/CuentaEnEdicionService.cs
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
    /// 🔒 Servicio para controlar la edición simultánea de cuentas
    /// Previene que múltiples usuarios trabajen en la misma cuenta al mismo tiempo
    /// Utiliza MemoryMappedFiles para comunicación entre procesos
    /// </summary>
    public class CuentaEnEdicionService : IDisposable
    {
        private readonly string _memoryMapName = "SaunaCuentasEnEdicion";
        private readonly MemoryMappedFile _memoryMappedFile;
        private readonly MemoryMappedViewAccessor _accessor;
        private readonly Mutex _mutex;
        private readonly ConcurrentDictionary<int, string> _cuentasEnEdicion = new();
        private readonly Timer _limpiezaTimer;
        private bool _disposed = false;

        public CuentaEnEdicionService()
        {
            try
            {
                // Crear memoria compartida para almacenar cuentas en edición
                _memoryMappedFile = MemoryMappedFile.CreateOrOpen(_memoryMapName, 4096);
                _accessor = _memoryMappedFile.CreateViewAccessor(0, 4096);
                _mutex = new Mutex(false, "SaunaCuentasEnEdicionMutex");

                // Timer para limpiar entradas antiguas cada 30 segundos
                _limpiezaTimer = new Timer(LimpiarEntradasAntiguas, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error inicializando CuentaEnEdicionService: {ex.Message}");
            }
        }

        /// <summary>
        /// 🔒 Intenta bloquear una cuenta para edición
        /// </summary>
        /// <param name="idCuenta">ID de la cuenta</param>
        /// <param name="usuarioEditor">Nombre del usuario que está editando</param>
        /// <returns>True si se pudo bloquear, False si ya está siendo editada</returns>
        public (bool exito, string mensaje, string usuarioEditor) IntentarBloquearCuenta(int idCuenta, string usuarioEditor)
        {
            try
            {
                _mutex?.WaitOne();

                // Verificar en memoria local
                if (_cuentasEnEdicion.ContainsKey(idCuenta))
                {
                    var editor = _cuentasEnEdicion[idCuenta];
                    return (false, $"La cuenta ya está siendo utilizada por {editor}", editor);
                }

                // Verificar en memoria compartida
                var cuentaEnEdicion = LeerCuentaEnMemoria(idCuenta);
                if (cuentaEnEdicion.HasValue)
                {
                    return (false, $"La cuenta ya está siendo utilizada por {cuentaEnEdicion.Value.usuario}", cuentaEnEdicion.Value.usuario);
                }

                // Bloquear cuenta
                var timestamp = DateTime.Now.Ticks;
                _cuentasEnEdicion[idCuenta] = usuarioEditor;
                EscribirCuentaEnMemoria(idCuenta, usuarioEditor, timestamp);

                return (true, "Cuenta bloqueada para edición", usuarioEditor);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error bloqueando cuenta {idCuenta}: {ex.Message}");
                return (false, "Error interno al bloquear cuenta", "");
            }
            finally
            {
                try { _mutex?.ReleaseMutex(); } catch { }
            }
        }

        /// <summary>
        /// 🔓 Libera el bloqueo de una cuenta
        /// </summary>
        /// <param name="idCuenta">ID de la cuenta</param>
        /// <param name="usuarioEditor">Usuario que está liberando el bloqueo</param>
        public void LiberarBloqueCuenta(int idCuenta, string usuarioEditor)
        {
            try
            {
                _mutex?.WaitOne();

                // Remover de memoria local
                if (_cuentasEnEdicion.TryGetValue(idCuenta, out var editorActual) && editorActual == usuarioEditor)
                {
                    _cuentasEnEdicion.TryRemove(idCuenta, out _);
                }

                // Remover de memoria compartida
                RemoverCuentaDeMemoria(idCuenta, usuarioEditor);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error liberando cuenta {idCuenta}: {ex.Message}");
            }
            finally
            {
                try { _mutex?.ReleaseMutex(); } catch { }
            }
        }

        /// <summary>
        /// 🔍 Verifica si una cuenta está siendo editada
        /// </summary>
        public (bool enEdicion, string usuarioEditor) VerificarCuentaEnEdicion(int idCuenta)
        {
            try
            {
                _mutex?.WaitOne();

                // Verificar en memoria local
                if (_cuentasEnEdicion.TryGetValue(idCuenta, out var editor))
                {
                    return (true, editor);
                }

                // Verificar en memoria compartida
                var cuentaEnEdicion = LeerCuentaEnMemoria(idCuenta);
                if (cuentaEnEdicion.HasValue)
                {
                    return (true, cuentaEnEdicion.Value.usuario);
                }

                return (false, "");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error verificando cuenta {idCuenta}: {ex.Message}");
                return (false, "");
            }
            finally
            {
                try { _mutex?.ReleaseMutex(); } catch { }
            }
        }

        #region Gestión de Memoria Compartida

        private (string usuario, long timestamp)? LeerCuentaEnMemoria(int idCuenta)
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
                        id == idCuenta &&
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

        private void EscribirCuentaEnMemoria(int idCuenta, string usuario, long timestamp)
        {
            try
            {
                if (_accessor == null) return;

                var data = new byte[4096];
                _accessor.ReadArray(0, data, 0, 4096);
                var content = Encoding.UTF8.GetString(data).TrimEnd('\0');
                
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
                
                // Remover entrada anterior de la misma cuenta
                lines.RemoveAll(line => line.StartsWith($"{idCuenta}|"));
                
                // Agregar nueva entrada
                lines.Add($"{idCuenta}|{usuario}|{timestamp}");
                
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
                System.Diagnostics.Debug.WriteLine($"❌ Error escribiendo cuenta en memoria: {ex.Message}");
            }
        }

        private void RemoverCuentaDeMemoria(int idCuenta, string usuarioValidar)
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
                           id == idCuenta && 
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
                System.Diagnostics.Debug.WriteLine($"❌ Error removiendo cuenta de memoria: {ex.Message}");
            }
        }

        private void LimpiarEntradasAntiguas(object state)
        {
            try
            {
                _mutex?.WaitOne();

                if (_accessor == null) return;

                var ahora = DateTime.Now.Ticks;
                var tiempoLimite = TimeSpan.FromMinutes(15).Ticks; // 15 minutos para cuentas

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
                var cuentasARemover = _cuentasEnEdicion.Keys.ToList();
                foreach (var idCuenta in cuentasARemover)
                {
                    if (!lineasValidas.Any(l => l.StartsWith($"{idCuenta}|")))
                    {
                        _cuentasEnEdicion.TryRemove(idCuenta, out _);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"🧹 Limpieza cuentas completada. Entradas activas: {lineasValidas.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en limpieza automática cuentas: {ex.Message}");
            }
            finally
            {
                try { _mutex?.ReleaseMutex(); } catch { }
            }
        }

        /// <summary>
        /// 🧹 Libera todos los bloqueos de un usuario específico
        /// </summary>
        /// <param name="usuario">Usuario del cual liberar todos los bloqueos</param>
        public void LiberarTodosBloqueosUsuario(string usuario)
        {
            try
            {
                _mutex?.WaitOne();

                // Liberar de memoria local
                var cuentasALiberar = _cuentasEnEdicion
                    .Where(kvp => string.Equals(kvp.Value, usuario, StringComparison.OrdinalIgnoreCase))
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var idCuenta in cuentasALiberar)
                {
                    _cuentasEnEdicion.TryRemove(idCuenta, out _);
                }

                // Liberar de memoria compartida
                if (_accessor != null)
                {
                    var data = new byte[4096];
                    _accessor.ReadArray(0, data, 0, 4096);
                    var content = Encoding.UTF8.GetString(data).TrimEnd('\0');
                    
                    var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .Where(line => !string.IsNullOrEmpty(line))
                        .ToList();

                    // Filtrar líneas que NO pertenecen al usuario
                    var lineasFiltradas = lines.Where(line =>
                    {
                        var parts = line.Split('|');
                        return parts.Length < 2 || !string.Equals(parts[1], usuario, StringComparison.OrdinalIgnoreCase);
                    }).ToList();

                    // Escribir de vuelta
                    var newContent = string.Join("\n", lineasFiltradas);
                    var newData = new byte[4096];
                    var bytes = Encoding.UTF8.GetBytes(newContent);
                    Array.Copy(bytes, newData, Math.Min(bytes.Length, 4096));
                    _accessor.WriteArray(0, newData, 0, 4096);
                }

                System.Diagnostics.Debug.WriteLine($"🧹 Liberados todos los bloqueos del usuario: {usuario}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error liberando bloqueos del usuario {usuario}: {ex.Message}");
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