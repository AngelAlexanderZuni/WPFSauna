using ProyectoSauna.Models.Entities;

namespace ProyectoSauna.Models.DTOs
{
    public class ServicioDTO
    {
        public int idServicio { get; set; }
        public string nombre { get; set; } = null!;
        public decimal precio { get; set; }
        public int? duracionEstimada { get; set; }
        public bool activo { get; set; }
        public int? idCategoriaServicio { get; set; }
        public string? nombreCategoria { get; set; }

        public static ServicioDTO FromEntity(Servicio s)
        {
            return new ServicioDTO
            {
                idServicio = s.idServicio,
                nombre = s.nombre,
                precio = s.precio,
                duracionEstimada = s.duracionEstimada,
                activo = s.activo,
                idCategoriaServicio = s.idCategoriaServicio,
                nombreCategoria = s.idCategoriaServicioNavigation?.nombre
            };
        }

        public Servicio ToEntity()
        {
            return new Servicio
            {
                idServicio = this.idServicio,
                nombre = this.nombre,
                precio = this.precio,
                duracionEstimada = this.duracionEstimada,
                activo = this.activo,
                idCategoriaServicio = this.idCategoriaServicio
            };
        }
    }
}