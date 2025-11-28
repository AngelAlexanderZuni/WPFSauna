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
            return "Server=LAPTOP-2BE5D2EQ\\SQL2019;Database=ProyectoSauna1;Trusted_Connection=true;TrustServerCertificate=true;";
        }
    }
}