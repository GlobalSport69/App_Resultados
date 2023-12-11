using System;
using System.Collections.Generic;
using LotteryResult.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LotteryResult.Data.Context;

public partial class PostgresDbContext : DbContext
{
    public PostgresDbContext()
    {
    }

    public PostgresDbContext(DbContextOptions<PostgresDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Provider> Providers { get; set; }

    public virtual DbSet<ProviderProduct> ProviderProducts { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_pk");

            entity.ToTable("products");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('product_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Enable).HasColumnName("enable");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Provider>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("providers_pk");

            entity.ToTable("providers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Enable).HasColumnName("enable");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.Url)
                .HasColumnType("character varying")
                .HasColumnName("url");
        });

        modelBuilder.Entity<ProviderProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("providers_products_pk");

            entity.ToTable("provider_product");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('providers_products_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.CronExpression)
                .HasColumnType("character varying")
                .HasColumnName("cron_expression");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");

            entity.HasOne(d => d.Product).WithMany(p => p.ProviderProducts)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("provider_product_fk_1");

            entity.HasOne(d => d.Provider).WithMany(p => p.ProviderProducts)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("provider_product_fk");
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("results_pk");

            entity.ToTable("results");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Date)
                .HasColumnType("character varying")
                .HasColumnName("date");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProviderId).HasColumnName("provider_id");
            entity.Property(e => e.Result1)
                .HasColumnType("character varying")
                .HasColumnName("result");
            entity.Property(e => e.Time)
                .HasColumnType("character varying")
                .HasColumnName("time");

            entity.HasOne(d => d.Product).WithMany(p => p.Results)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("results_fk");

            entity.HasOne(d => d.Provider).WithMany(p => p.Results)
                .HasForeignKey(d => d.ProviderId)
                .HasConstraintName("results_fk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
