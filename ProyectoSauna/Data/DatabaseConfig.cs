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
            return "Server=.;Database=ProyectoSauna;Trusted_Connection=true;TrustServerCertificate=true;";
        }
    }
}