using System;
using System.Collections.Generic;
using System.Text;

namespace InventarioV1.Models
{
    public class DetalleVenta
    {
        public int Id { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }

        public int VentaId { get; set; }
        public Venta Venta { get; set; } = null!;

        public int ProductoId { get; set; }
        public Producto Producto { get; set; } = null!;
    }
}
