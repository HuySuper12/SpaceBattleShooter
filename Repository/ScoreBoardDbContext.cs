using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository.Entities;

namespace Repository;

public partial class ScoreBoardDbContext : DbContext
{
    public ScoreBoardDbContext()
    {
    }

    public ScoreBoardDbContext(DbContextOptions<ScoreBoardDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Score> Scores { get; set; }

    public virtual DbSet<ScoreSorted> ScoreSorteds { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(GetConnectionString());
    private string? GetConnectionString()
    {
        IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true).Build();
        return configuration["ConnectionStrings:DBDefault"];
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasKey(e => e.Number).HasName("PK__Score__3214EC07C63EC78C");

            entity.ToTable("Score");

            entity.Property(e => e.EntryDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Score1).HasColumnName("Score");
        });

        modelBuilder.Entity<ScoreSorted>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ScoreSorted");

            entity.Property(e => e.EntryDate).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
