using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public partial class RaceDayContext : DbContext
{
    public RaceDayContext(DbContextOptions<RaceDayContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Enrolment> Enrolments { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<EventRoute> Routes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Weather> Weathers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A2B40D27CE0");

            entity.ToTable("Category");

            entity.HasIndex(e => new { e.EventId, e.CategoryName }, "UQ_Category_Event_Name").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.Distance).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.EntryFee).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.MaximumParticipants).HasDefaultValue(100);

            entity.HasOne(d => d.Event).WithMany(p => p.Categories)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Category_Event");
        });

        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.HasKey(e => e.EnrolmentId).HasName("PK__Enrolmen__5C0E5FEFFFDE2314");

            entity.ToTable("Enrolment");

            entity.HasIndex(e => new { e.ParticipantId, e.CategoryId }, "UQ_Enrolment_Participant_Category").IsUnique();

            entity.Property(e => e.EnrolmentId).HasColumnName("EnrolmentID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.EnrolmentDate).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.ParticipantId).HasColumnName("ParticipantID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Confirmed");

            entity.HasOne(d => d.Category).WithMany(p => p.Enrolments)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrolment_Category");

            entity.HasOne(d => d.Participant).WithMany(p => p.Enrolments)
                .HasForeignKey(d => d.ParticipantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrolment_Participant");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Event__7944C87066F757A6");

            entity.ToTable("Event");

            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EventName).HasMaxLength(150);
            entity.Property(e => e.Location).HasMaxLength(150);
            entity.Property(e => e.OrganiserId).HasColumnName("OrganiserID");

            entity.HasOne(d => d.Organiser).WithMany(p => p.Events)
                .HasForeignKey(d => d.OrganiserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Event_User");
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__Result__976902281911E942");

            entity.ToTable("Result");

            entity.HasIndex(e => e.EnrolmentId, "UQ__Result__5C0E5FEE1978F779").IsUnique();

            entity.Property(e => e.ResultId).HasColumnName("ResultID");
            entity.Property(e => e.EnrolmentId).HasColumnName("EnrolmentID");
            entity.Property(e => e.FinishTime).HasPrecision(0);

            entity.HasOne(d => d.Enrolment).WithOne(p => p.Result)
                .HasForeignKey<Result>(d => d.EnrolmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Result_Enrolment");
        });


        modelBuilder.Entity<EventRoute>(entity =>
        {
            entity.HasKey(e => e.RouteId)
                .HasName("PK__Route__80979AAD1522C863");

            entity.ToTable("Route");

            entity.HasIndex(e => e.EventId, "UQ__Route__7944C871F772583A")
                .IsUnique();

            entity.Property(e => e.RouteId).HasColumnName("RouteID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Distance).HasColumnType("decimal(6, 2)");
            entity.Property(e => e.EventId).HasColumnName("EventID");

            entity.Property(e => e.MapUrl)
                .HasMaxLength(500)
                .HasColumnName("MapURL");

            entity.Property(e => e.RouteName).HasMaxLength(150);

            entity.HasOne(d => d.Event)
                .WithOne(p => p.Route)
                .HasForeignKey<EventRoute>(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Route_Event");
        });


        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCACE4E959A0");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__A9D10534CE6099DC").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(20);
        });

        modelBuilder.Entity<Weather>(entity =>
        {
            entity.HasKey(e => e.WeatherId).HasName("PK__Weather__0BF97BD53D2F567E");

            entity.ToTable("Weather");

            entity.Property(e => e.WeatherId).HasColumnName("WeatherID");
            entity.Property(e => e.Condition).HasMaxLength(100);
            entity.Property(e => e.EventId).HasColumnName("EventID");
            entity.Property(e => e.Temperature).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.WindSpeed).HasColumnType("decimal(6, 2)");

            entity.HasOne(d => d.Event).WithMany(p => p.Weathers)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Weather_Event");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
