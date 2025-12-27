namespace ProyectoSauna.Data
{
    /// <summary>
    /// Configuración centralizada de base de datos
    /// </summary>
    public static class DatabaseConfig
    {
        /// <summary>
        /// Obtiene la cadena de conexión a SQL Server
        /// </summary>
        public static string GetConnectionString()
        {
            // ⚠️ CONFIGURACIÓN ACTUAL:
            // - Servidor: DESKTOP-HG4U4IK\Luis
            // - Base de Datos: ProyectoSauna
            // - Autenticación: Windows Authentication (Trusted_Connection=true)
            // - Certificado: Se confía automáticamente (TrustServerCertificate=true)
            
            var connectionString = "Server=DESKTOP-HG4U4IK;Database=ProyectoSauna;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true;";
            
            #if DEBUG
            System.Diagnostics.Debug.WriteLine($"🔗 Connection String: {connectionString}");
            #endif
            
            return connectionString;
        }

        /// <summary>
        /// Obtiene la cadena de conexión con timeout personalizado
        /// </summary>
        public static string GetConnectionStringWithTimeout(int timeoutSeconds = 30)
        {
            return $"{GetConnectionString()}Connect Timeout={timeoutSeconds};";
        }
    }
}