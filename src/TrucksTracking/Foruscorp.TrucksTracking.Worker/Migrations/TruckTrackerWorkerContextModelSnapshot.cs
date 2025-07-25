﻿// <auto-generated />
using System;
using Foruscorp.TrucksTracking.Worker.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Foruscorp.TrucksTracking.Worker.Migrations
{
    [DbContext(typeof(TruckTrackerWorkerContext))]
    partial class TruckTrackerWorkerContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("TruckTrackerWorker")
                .HasAnnotation("ProductVersion", "9.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Foruscorp.TrucksTracking.Worker.Domain.TruckTracker", b =>
                {
                    b.Property<Guid>("TruckId")
                        .HasColumnType("uuid");

                    b.Property<string>("ProviderTruckId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("TruckId");

                    b.ToTable("TruckTrackers", "TruckTrackerWorker");
                });
#pragma warning restore 612, 618
        }
    }
}
