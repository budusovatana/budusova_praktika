using budusova_partners.Lib.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace budusova_partners.Lib.Data
{
    public class AppDbContext : DbContext
    {
        // Конструктор для обычной работы
        public AppDbContext() { }

        // Конструктор для тестов с переданными параметрами
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet-ы
        public DbSet<Partner> Partners { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PartnerSale> PartnerSales { get; set; }
        public DbSet<PartnerType> PartnerTypes { get; set; }

        public DbSet<ProductType> ProductTypes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Подключение к PostgreSQL для основной работы
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Database=budusova_db_partners;Username=app;Password=123456789");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("app");

            // Настройка Partner
            modelBuilder.Entity<Partner>(entity =>
            {
                entity.ToTable("partners");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PartnerTypeId).HasColumnName("partner_type_id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.LegalAddress).HasColumnName("legal_address");
                entity.Property(e => e.DirectorFullName).HasColumnName("director_full_name");
                entity.Property(e => e.Phone).HasColumnName("phone");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.Rating).HasColumnName("rating");
            });

            // Настройка PartnerType
            modelBuilder.Entity<PartnerType>(entity =>
            {
                entity.ToTable("partner_types");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
            });

            // Настройка Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ProductTypeId).HasColumnName("product_type_id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.MinPartnerPrice).HasColumnName("min_partner_price");
            });

            // Настройка PartnerSale
            modelBuilder.Entity<PartnerSale>(entity =>
            {
                entity.ToTable("partner_sales");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PartnerId).HasColumnName("partner_id");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.UnitPrice).HasColumnName("unit_price");
                entity.Property(e => e.SaleDate).HasColumnName("sale_date");
            });
        }
    }
}