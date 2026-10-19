using System;
using System.Collections.Generic;
using System.Text;

namespace InventarioV1.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal Total { get; set; }
        public decimal PagaCon { get; set; }
        public decimal Cambio { get; set; }

        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }

}
