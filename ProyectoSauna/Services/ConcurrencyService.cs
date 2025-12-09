// Services/ConcurrencyService.cs - Control de concurrencia seguro y opcional
using Microsoft.EntityFrameworkCore;
using ProyectoSauna.Models;
using System;
using System.Threading.Tasks;

namespace ProyectoSauna.Services
{
    public class ConcurrencyService
    {
        private readonly SaunaDbContext _context;
        private bool _concurrencyEnabled = true; // Flag para habilitar/deshabilitar

        public ConcurrencyService(SaunaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Intenta guardar cambios con manejo de concurrencia
        /// Si falla, devuelve false y no rompe el flujo normal
        /// </summary>
        public async Task<ConcurrencyResult> SafeSaveChangesAsync()
        {
            if (!_concurrencyEnabled)
            {
                // Si está deshabilitado, funciona como siempre
                await _context.SaveChangesAsync();
                return new ConcurrencyResult { Success = true, Message = "Guardado exitoso" };
            }

            try
            {
                await _context.SaveChangesAsync();
                return new ConcurrencyResult { Success = true, Message = "Guardado exitoso" };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Log del error pero NO rompe la aplicación
                Console.WriteLine($"Conflicto de concurrencia: {ex.Message}");
                
                return new ConcurrencyResult 
                { 
                    Success = false, 
                    Message = "Los datos fueron modificados por otro usuario. Por favor, recarga la información.",
                    ConflictEntries = ex.Entries
                };
            }
            catch (Exception ex)
            {
                // Cualquier otro error se maneja normalmente
                throw ex;
            }
        }

        /// <summary>
        /// Habilita o deshabilita el control de concurrencia
        /// </summary>
        public void SetConcurrencyEnabled(bool enabled)
        {
            _concurrencyEnabled = enabled;
        }

        /// <summary>
        /// Refresca una entidad con los datos más actuales de la BD
        /// </summary>
        public async Task<T> RefreshEntityAsync<T>(T entity) where T : class
        {
            var entry = _context.Entry(entity);
            await entry.ReloadAsync();
            return entity;
        }

        /// <summary>
        /// Versión genérica que permite retornar un resultado de la operación
        /// </summary>
        public async Task<ConcurrencyResult<T>> SafeSaveChangesAsync<T>(Func<Task<T>> operation)
        {
            if (!_concurrencyEnabled)
            {
                try
                {
                    var result = await operation();
                    await _context.SaveChangesAsync();
                    return new ConcurrencyResult<T> { Success = true, Result = result };
                }
                catch (Exception ex)
                {
                    return new ConcurrencyResult<T> 
                    { 
                        Success = false, 
                        ErrorMessage = ex.Message,
                        Result = default!
                    };
                }
            }

            try
            {
                var result = await operation();
                await _context.SaveChangesAsync();
                return new ConcurrencyResult<T> { Success = true, Result = result };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"Conflicto de concurrencia: {ex.Message}");
                
                return new ConcurrencyResult<T> 
                { 
                    Success = false, 
                    ErrorMessage = "Los datos fueron modificados por otro usuario. Por favor, recarga la información.",
                    ConflictEntries = ex.Entries,
                    Result = default!
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class ConcurrencyResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public IReadOnlyList<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry>? ConflictEntries { get; set; }
    }

    public class ConcurrencyResult<T> : ConcurrencyResult
    {
        public T Result { get; set; } = default!;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}