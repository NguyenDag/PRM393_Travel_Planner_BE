using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PRM393_Travel_Planner_BE.Models;

public partial class Prm393TravelPlannerContext : DbContext
{
    public Prm393TravelPlannerContext()
    {
    }

    public Prm393TravelPlannerContext(DbContextOptions<Prm393TravelPlannerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AiSuggestion> AiSuggestions { get; set; }

    public virtual DbSet<Checklist> Checklists { get; set; }

    public virtual DbSet<ChecklistItem> ChecklistItems { get; set; }

    public virtual DbSet<Destination> Destinations { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Trip> Trips { get; set; }

    public virtual DbSet<TripActivity> TripActivities { get; set; }

    public virtual DbSet<TripDay> TripDays { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=gondola.proxy.rlwy.net;Port=12844;Database=PRM393_Travel_Planner;Username=postgres;Password=VvpnZbZfvgngGUslpUFwNiQrlapJjUCj");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<AiSuggestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("AiSuggestions_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.HighlightsJson).HasColumnType("jsonb");
            entity.Property(e => e.ItineraryJson).HasColumnType("jsonb");
            entity.Property(e => e.Label).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Destination).WithMany(p => p.AiSuggestions)
                .HasForeignKey(d => d.DestinationId)
                .HasConstraintName("AiSuggestions_DestinationId_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.AiSuggestions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("AiSuggestions_UserId_fkey");
        });

        modelBuilder.Entity<Checklist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Checklists_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.Trip).WithMany(p => p.Checklists)
                .HasForeignKey(d => d.TripId)
                .HasConstraintName("Checklists_TripId_fkey");
        });

        modelBuilder.Entity<ChecklistItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ChecklistItems_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsChecked).HasDefaultValue(false);
            entity.Property(e => e.Label).HasMaxLength(200);

            entity.HasOne(d => d.Checklist).WithMany(p => p.ChecklistItems)
                .HasForeignKey(d => d.ChecklistId)
                .HasConstraintName("ChecklistItems_ChecklistId_fkey");
        });

        modelBuilder.Entity<Destination>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Destinations_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsTrending).HasDefaultValue(false);
            entity.Property(e => e.Latitude).HasPrecision(10, 7);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Longitude).HasPrecision(10, 7);
            entity.Property(e => e.ShortDesc).HasMaxLength(300);
            entity.Property(e => e.Tag).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.ViewCount).HasDefaultValue(0);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("RefreshTokens_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ExpiresAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("RefreshTokens_UserId_fkey");
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Trips_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Country).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.TotalDays).HasComputedColumnSql("((\"DateTo\" - \"DateFrom\") + 1)", true);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.SourceAiSuggestion).WithMany(p => p.Trips)
                .HasForeignKey(d => d.SourceAiSuggestionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("Trips_SourceAiSuggestionId_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Trips)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("Trips_UserId_fkey");
        });

        modelBuilder.Entity<TripActivity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TripActivities_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Latitude).HasPrecision(10, 7);
            entity.Property(e => e.LocationName).HasMaxLength(200);
            entity.Property(e => e.Longitude).HasPrecision(10, 7);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.TripDay).WithMany(p => p.TripActivities)
                .HasForeignKey(d => d.TripDayId)
                .HasConstraintName("TripActivities_TripDayId_fkey");
        });

        modelBuilder.Entity<TripDay>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("TripDays_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Trip).WithMany(p => p.TripDays)
                .HasForeignKey(d => d.TripId)
                .HasConstraintName("TripDays_TripId_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Users_pkey");

            entity.HasIndex(e => e.Email, "Users_Email_key").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
