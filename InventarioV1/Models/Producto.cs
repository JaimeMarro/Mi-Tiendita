using System;
using System.Collections.Generic;
using System.Text;

namespace InventarioV1.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string? ImagenBase64 { get; set; }
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = null!;
    }
}
