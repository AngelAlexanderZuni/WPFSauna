using System;
using System.IO;
using ProyectoSauna.Models.Entities;

namespace ProyectoSauna.Services
{
    public static class AuditLogger
    {
        private static readonly string Dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProyectoSauna");
        private static readonly string FilePath = Path.Combine(Dir, "audit.log");

        private static void Ensure()
        {
            if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);
            if (!File.Exists(FilePath)) File.WriteAllText(FilePath, "# Auditoría de Inventario\n");
        }

        public static void LogInventario(string operacion, Producto producto, int stockAntes, int stockDespues, int idUsuario, string observaciones)
        {
            Ensure();
            var line = string.Join(" | ", new[]
            {
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                $"Usuario:{idUsuario}",
                $"Op:{operacion}",
                $"Producto:{producto.codigo}",
                $"Antes:{stockAntes}",
                $"Despues:{stockDespues}",
                $"Obs:{observaciones ?? string.Empty}"
            });
            File.AppendAllLines(FilePath, new[] { line });
        }
    }
}