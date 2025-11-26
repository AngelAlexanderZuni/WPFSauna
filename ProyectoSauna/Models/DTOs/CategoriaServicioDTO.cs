using ProyectoSauna.Models.Entities;

namespace ProyectoSauna.Models.DTOs
{
    public class CategoriaServicioDTO
    {
        public int idCategoriaServicio { get; set; }
        public string nombre { get; set; } = null!;
        public bool activo { get; set; }
        public int? cantidadServicios { get; set; }

        public static CategoriaServicioDTO FromEntity(CategoriaServicio categoria)
        {
            return new CategoriaServicioDTO
            {
                idCategoriaServicio = categoria.idCategoriaServicio,
                nombre = categoria.nombre,
                activo = categoria.activo,
                cantidadServicios = categoria.Servicio?.Count
            };
        }

        public CategoriaServicio ToEntity()
        {
            return new CategoriaServicio
            {
                idCategoriaServicio = this.idCategoriaServicio,
                nombre = this.nombre,
                activo = this.activo
            };
        }
    }
}