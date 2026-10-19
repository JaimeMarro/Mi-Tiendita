using InventarioV1.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace InventarioV1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Venta>()
                .Property(v => v.Total)
                .HasColumnType("TEXT");

            modelBuilder.Entity<Venta>()
                .Property(v => v.PagaCon)
                .HasColumnType("TEXT");

            modelBuilder.Entity<Venta>()
                .Property(v => v.Cambio)
                .HasColumnType("TEXT");

            modelBuilder.Entity<Producto>()
                .Property(p => p.Precio)
                .HasColumnType("TEXT");

            modelBuilder.Entity<DetalleVenta>()
                .Property(d => d.PrecioUnitario)
                .HasColumnType("TEXT");
        }
    }
}
