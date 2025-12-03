namespace test_api.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activa { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }

        // ?? Relación con Productos (1 a Muchos)
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
