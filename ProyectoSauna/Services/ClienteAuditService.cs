// Services/ClienteAuditService.cs - Auditoría de operaciones de clientes para control de concurrencia
using ProyectoSauna.Models;
using ProyectoSauna.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ProyectoSauna.Services
{
    /// <summary>
    /// Servicio de auditoría para rastrear operaciones de clientes
    /// Útil para detectar problemas de concurrencia y debugging
    /// </summary>
    public class ClienteAuditService
    {
        private readonly SaunaDbContext _context;

        public ClienteAuditService(SaunaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Registra una operación de cliente para auditoría
        /// </summary>
        public Task LogOperationAsync(ClienteOperation operation)
        {
            try
            {
                // En un entorno real, esto iría a una tabla de auditoría
                // Por ahora, solo log en consola para no modificar el esquema de BD
                
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] " +
                              $"Usuario: {operation.Usuario} | " +
                              $"Operación: {operation.TipoOperacion} | " +
                              $"ClienteID: {operation.ClienteId} | " +
                              $"DNI: {operation.DNI} | " +
                              $"Thread: {System.Threading.Thread.CurrentThread.ManagedThreadId} | " +
                              $"Resultado: {operation.Resultado} | " +
                              $"Duración: {operation.DuracionMs}ms";

                Console.WriteLine(logEntry);

                // También guardar en memoria para consultas rápidas durante testing
                _recentOperations.Enqueue(operation);
                
                // Mantener solo las últimas 100 operaciones en memoria
                while (_recentOperations.Count > 100)
                {
                    _recentOperations.TryDequeue(out _);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                // No fallar la operación principal por problemas de auditoría
                Console.WriteLine($"Error en auditoría: {ex.Message}");
                return Task.CompletedTask;
            }
        }

        // Cola thread-safe para operaciones recientes
        private static readonly System.Collections.Concurrent.ConcurrentQueue<ClienteOperation> _recentOperations = new();

        /// <summary>
        /// Obtiene las operaciones recientes para análisis
        /// </summary>
        public List<ClienteOperation> GetRecentOperations()
        {
            return _recentOperations.ToList();
        }

        /// <summary>
        /// Detecta posibles conflictos de concurrencia basado en operaciones recientes
        /// </summary>
        public List<string> DetectPotentialConcurrencyIssues()
        {
            var issues = new List<string>();
            var operations = GetRecentOperations();
            var now = DateTime.Now;

            // Buscar operaciones en el mismo DNI en los últimos 30 segundos
            var dniConflicts = operations
                .Where(op => (now - op.Timestamp).TotalSeconds <= 30)
                .GroupBy(op => op.DNI)
                .Where(g => g.Count() > 1)
                .Where(g => g.Any(op => op.TipoOperacion == "Crear"))
                .ToList();

            foreach (var group in dniConflicts)
            {
                issues.Add($"Posible conflicto de DNI: {group.Key} - {group.Count()} operaciones en 30 segundos");
            }

            // Buscar actualizaciones simultáneas del mismo cliente
            var clienteConflicts = operations
                .Where(op => op.ClienteId.HasValue && (now - op.Timestamp).TotalSeconds <= 10)
                .GroupBy(op => op.ClienteId)
                .Where(g => g.Count() > 1 && g.Any(op => op.TipoOperacion == "Actualizar"))
                .ToList();

            foreach (var group in clienteConflicts)
            {
                issues.Add($"Posible conflicto de actualización: Cliente ID {group.Key} - {group.Count()} operaciones en 10 segundos");
            }

            return issues;
        }

        /// <summary>
        /// Obtiene estadísticas de operaciones
        /// </summary>
        public OperationStats GetOperationStats()
        {
            var operations = GetRecentOperations();
            var now = DateTime.Now;
            var last5Minutes = operations.Where(op => (now - op.Timestamp).TotalMinutes <= 5).ToList();

            return new OperationStats
            {
                TotalOperationsLast5Min = last5Minutes.Count,
                CreateOperations = last5Minutes.Count(op => op.TipoOperacion == "Crear"),
                UpdateOperations = last5Minutes.Count(op => op.TipoOperacion == "Actualizar"),
                FailedOperations = last5Minutes.Count(op => op.Resultado == "Error"),
                AverageDurationMs = last5Minutes.Any() ? last5Minutes.Average(op => op.DuracionMs) : 0,
                ConcurrentOperationsDetected = DetectPotentialConcurrencyIssues().Count
            };
        }
    }

    /// <summary>
    /// Información de una operación de cliente para auditoría
    /// </summary>
    public class ClienteOperation
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Usuario { get; set; } = "Sistema";
        public string TipoOperacion { get; set; } = ""; // Crear, Actualizar, Activar, Desactivar
        public int? ClienteId { get; set; }
        public string DNI { get; set; } = "";
        public string Resultado { get; set; } = ""; // Éxito, Error, Conflicto
        public long DuracionMs { get; set; }
        public string? DetallesError { get; set; }
        public int ThreadId { get; set; } = System.Threading.Thread.CurrentThread.ManagedThreadId;
    }

    /// <summary>
    /// Estadísticas de operaciones
    /// </summary>
    public class OperationStats
    {
        public int TotalOperationsLast5Min { get; set; }
        public int CreateOperations { get; set; }
        public int UpdateOperations { get; set; }
        public int FailedOperations { get; set; }
        public double AverageDurationMs { get; set; }
        public int ConcurrentOperationsDetected { get; set; }
    }
}