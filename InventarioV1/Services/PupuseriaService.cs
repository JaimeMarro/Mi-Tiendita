using InventarioV1.Data;
using InventarioV1.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventarioV1.Services
{
    public class PupuseriaService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        // Caché simple
        private List<Categoria>? _categoriasCache;
        private List<Producto>? _productosCache;

        public PupuseriaService(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
        }

        // ─── CATEGORIAS ───
        public async Task<List<Categoria>> GetCategoriasAsync()
        {
            if (_categoriasCache != null) return _categoriasCache;
            using var db = _factory.CreateDbContext();
            _categoriasCache = await db.Categorias.ToListAsync();
            return _categoriasCache;
        }

        public async Task AgregarCategoriaAsync(Categoria categoria)
        {
            using var db = _factory.CreateDbContext();
            db.Categorias.Add(categoria);
            await db.SaveChangesAsync();
            _categoriasCache = null; // Invalidar caché
        }

        public async Task EliminarCategoriaAsync(Categoria categoria)
        {
            using var db = _factory.CreateDbContext();
            var existing = await db.Categorias.FindAsync(categoria.Id);
            if (existing != null)
            {
                db.Categorias.Remove(existing);
                await db.SaveChangesAsync();
            }
            _categoriasCache = null;
            _productosCache = null;
        }

        // ─── PRODUCTOS ───
        public async Task<List<Producto>> GetProductosAsync()
        {
            if (_productosCache != null) return _productosCache;
            using var db = _factory.CreateDbContext();
            _productosCache = await db.Productos.Include(p => p.Categoria).ToListAsync();
            return _productosCache;
        }

        public async Task<List<Producto>> GetProductosPorCategoriaAsync(int categoriaId)
        {
            var todos = await GetProductosAsync();
            return todos.Where(p => p.CategoriaId == categoriaId).ToList();
        }

        public async Task AgregarProductoAsync(Producto producto)
        {
            using var db = _factory.CreateDbContext();
            db.Productos.Add(producto);
            await db.SaveChangesAsync();
            _productosCache = null; // Invalidar caché
        }

        public async Task EditarProductoAsync(Producto producto)
        {
            using var db = _factory.CreateDbContext();
            var existing = await db.Productos.FindAsync(producto.Id);
            if (existing != null)
            {
                existing.Nombre = producto.Nombre;
                existing.Precio = producto.Precio;
                existing.ImagenBase64 = producto.ImagenBase64;
                existing.CategoriaId = producto.CategoriaId;
                await db.SaveChangesAsync();
            }
            _productosCache = null;
        }

        public async Task EliminarProductoAsync(Producto producto)
        {
            using var db = _factory.CreateDbContext();
            db.Productos.Remove(producto);
            await db.SaveChangesAsync();
            _productosCache = null;
        }

        // ─── VENTAS ───
        public async Task<Venta> RegistrarVentaAsync(List<(Producto producto, int cantidad)> items, decimal pagaCon)
        {
            using var db = _factory.CreateDbContext();
            var total = items.Sum(i => i.producto.Precio * i.cantidad);
            var venta = new Venta
            {
                FechaHora = DateTime.Now,
                Total = total,
                PagaCon = pagaCon,
                Cambio = pagaCon - total,
                Detalles = items.Select(i => new DetalleVenta
                {
                    ProductoId = i.producto.Id,
                    Cantidad = i.cantidad,
                    PrecioUnitario = i.producto.Precio
                }).ToList()
            };
            db.Ventas.Add(venta);
            await db.SaveChangesAsync();
            return venta;
        }

        // ─── HISTORIAL ───
        public async Task<List<Venta>> GetVentasPorFechaAsync(DateTime fecha)
        {
            using var db = _factory.CreateDbContext();
            return await db.Ventas
                .Where(v => v.FechaHora.Date == fecha.Date)
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .ThenInclude(p => p.Categoria)
                .OrderByDescending(v => v.FechaHora)
                .ToListAsync();
        }

        public async Task<List<Venta>> GetVentasPorSemanaAsync(DateTime fecha)
        {
            using var db = _factory.CreateDbContext();
            var inicioDeSemana = fecha.AddDays(-(int)fecha.DayOfWeek);
            var finDeSemana = inicioDeSemana.AddDays(7);
            return await db.Ventas
                .Where(v => v.FechaHora >= inicioDeSemana && v.FechaHora < finDeSemana)
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .ThenInclude(p => p.Categoria)
                .OrderByDescending(v => v.FechaHora)
                .ToListAsync();
        }

        public async Task<List<Venta>> GetVentasPorMesAsync(int anio, int mes)
        {
            using var db = _factory.CreateDbContext();
            return await db.Ventas
                .Where(v => v.FechaHora.Year == anio && v.FechaHora.Month == mes)
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .ThenInclude(p => p.Categoria)
                .OrderByDescending(v => v.FechaHora)
                .ToListAsync();
        }

        public async Task EliminarVentaAsync(Venta venta)
        {
            using var db = _factory.CreateDbContext();
            var existing = await db.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == venta.Id);
            if (existing != null)
            {
                db.DetallesVenta.RemoveRange(existing.Detalles);
                db.Ventas.Remove(existing);
                await db.SaveChangesAsync();
            }
        }

        // ─── RESPALDO ───
        public async Task<string> ExportarDatosAsync()
        {
            using var db = _factory.CreateDbContext();

            var categorias = await db.Categorias.ToListAsync();
            var productos = await db.Productos.Include(p => p.Categoria).ToListAsync();
            var ventas = await db.Ventas
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .ToListAsync();

            var respaldo = new RespaldoDatos
            {
                Categorias = categorias.Select(c => new CategoriaRespaldo { Nombre = c.Nombre }).ToList(),
                Productos = productos.Select(p => new ProductoRespaldo
                {
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    ImagenBase64 = p.ImagenBase64,
                    CategoriaNombre = p.Categoria.Nombre
                }).ToList(),
                Ventas = ventas.Select(v => new VentaRespaldo
                {
                    FechaHora = v.FechaHora,
                    Total = v.Total,
                    PagaCon = v.PagaCon,
                    Cambio = v.Cambio,
                    Detalles = v.Detalles.Select(d => new DetalleVentaRespaldo
                    {
                        ProductoNombre = d.Producto.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario
                    }).ToList()
                }).ToList()
            };

            return System.Text.Json.JsonSerializer.Serialize(respaldo, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        public async Task<(bool exito, string mensaje)> ImportarDatosAsync(string json)
        {
            try
            {
                var respaldo = System.Text.Json.JsonSerializer.Deserialize<RespaldoDatos>(json);
                if (respaldo == null)
                    return (false, "El archivo no tiene un formato válido.");

                using var db = _factory.CreateDbContext();

                var categoriasExistentes = await db.Categorias.Select(c => c.Nombre).ToListAsync();
                foreach (var cat in respaldo.Categorias)
                {
                    if (!categoriasExistentes.Contains(cat.Nombre))
                    {
                        db.Categorias.Add(new Categoria { Nombre = cat.Nombre });
                    }
                }
                await db.SaveChangesAsync();

                var categoriasMap = await db.Categorias.ToDictionaryAsync(c => c.Nombre, c => c.Id);

                var productosExistentes = await db.Productos.Select(p => p.Nombre).ToListAsync();
                foreach (var prod in respaldo.Productos)
                {
                    if (!productosExistentes.Contains(prod.Nombre) &&
                        categoriasMap.ContainsKey(prod.CategoriaNombre))
                    {
                        db.Productos.Add(new Producto
                        {
                            Nombre = prod.Nombre,
                            Precio = prod.Precio,
                            ImagenBase64 = prod.ImagenBase64,
                            CategoriaId = categoriasMap[prod.CategoriaNombre]
                        });
                    }
                }
                await db.SaveChangesAsync();

                var productosMap = await db.Productos.ToDictionaryAsync(p => p.Nombre, p => p.Id);

                foreach (var venta in respaldo.Ventas)
                {
                    var nuevaVenta = new Venta
                    {
                        FechaHora = venta.FechaHora,
                        Total = venta.Total,
                        PagaCon = venta.PagaCon,
                        Cambio = venta.Cambio,
                        Detalles = venta.Detalles
                            .Where(d => productosMap.ContainsKey(d.ProductoNombre))
                            .Select(d => new DetalleVenta
                            {
                                ProductoId = productosMap[d.ProductoNombre],
                                Cantidad = d.Cantidad,
                                PrecioUnitario = d.PrecioUnitario
                            }).ToList()
                    };
                    db.Ventas.Add(nuevaVenta);
                }
                await db.SaveChangesAsync();

                _categoriasCache = null;
                _productosCache = null;

                return (true, "Datos importados correctamente.");
            }
            catch (Exception ex)
            {
                return (false, $"Error al importar: {ex.Message}");
            }
        }

        public class RespaldoDatos
        {
            public List<CategoriaRespaldo> Categorias { get; set; } = new();
            public List<ProductoRespaldo> Productos { get; set; } = new();
            public List<VentaRespaldo> Ventas { get; set; } = new();
        }

        public class ProductoRespaldo
        {
            public string Nombre { get; set; } = string.Empty;
            public decimal Precio { get; set; }
            public string? ImagenBase64 { get; set; }
            public string CategoriaNombre { get; set; } = string.Empty;
        }

        public class VentaRespaldo
        {
            public DateTime FechaHora { get; set; }
            public decimal Total { get; set; }
            public decimal PagaCon { get; set; }
            public decimal Cambio { get; set; }
            public List<DetalleVentaRespaldo> Detalles { get; set; } = new();
        }

        public class DetalleVentaRespaldo
        {
            public string ProductoNombre { get; set; } = string.Empty;
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
        }

        public class CategoriaRespaldo
        {
            public string Nombre { get; set; } = string.Empty;
        }
    }

}
