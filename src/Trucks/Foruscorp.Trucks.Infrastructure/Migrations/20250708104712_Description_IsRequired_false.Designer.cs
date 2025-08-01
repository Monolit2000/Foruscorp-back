﻿// <auto-generated />
using System;
using Foruscorp.Trucks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Foruscorp.Trucks.Infrastructure.Migrations
{
    [DbContext(typeof(TruckContext))]
    [Migration("20250708104712_Description_IsRequired_false")]
    partial class Description_IsRequired_false
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("Tuck")
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Foruscorp.Trucks.Domain.DriverFuelHistorys.DriverFuelHistory", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("DriverId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("FuelAmount")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("RecordedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("DriverId");

                    b.ToTable("DriverFuelHistories", "Tuck");
                });

            modelBuilder.Entity("Foruscorp.Trucks.Domain.Drivers.Driver", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("DriverId");

                    b.Property<decimal>("Bonus")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("ExperienceYears")
                        .HasColumnType("integer");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTime>("HireDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LicenseNumber")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<Guid?>("TruckId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("LicenseNumber")
                        .IsUnique();

                    b.HasIndex("TruckId")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("Drivers", "Tuck");
                });

            modelBuilder.Entity("Foruscorp.Trucks.Domain.Drivers.DriverBonus", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("BonusId");

                    b.Property<int>("Amount")
                        .HasColumnType("integer");

                    b.Property<DateTime>("AwardedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("DriverId")
                        .HasColumnType("uuid");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("DriverId");

                    b.ToTable("DriverBonuses", "Tuck");
                });

            modelBuilder.Entity("Foruscorp.Trucks.Domain.RouteOffers.RouteOffer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("RouteOfferId");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<Guid>("DriverId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RouteId")
                        .HasColumnType("uuid");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<Guid?>("TruckId")
                        .HasColumnType("uuid");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("DriverId");

                    b.ToTable("RouteOffers", "Tuck");
                });

            modelBuilder.Entity("Foruscorp.Trucks.Domain.Trucks.Truck", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAtTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("DriverId")
                        .HasColumnType("uuid");

                    b.Property<string>("HarshAccelerationSettingType")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("LicensePlate")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<string>("Make")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Model")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("ProviderTruckId")
                        .HasColumnType("text");

                    b.Property<string>("Serial")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAtTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Vin")
                        .HasColumnType("text");

                    b.Property<string>("Year")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("DriverId");

                    b.HasIndex("LicensePlate");

                    b.HasIndex("Serial");

                    b.HasIndex("Vin");

                    b.ToTable("Trucks", "Tuck");
                });

            modelBuilder.Entity("Foruscorp.Trucks.Domain.Users.User", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("UserId");

                    b.ToTable("Users", "Tuck");
                });

            modelBuilder.Entity("Foruscorp.Trucks.Domain.DriverFuelHistorys.DriverFuelHistory", b =>
                {
                    b.HasOne("Foruscorp.Trucks.Domain.Drivers.Driver", null)
                        .WithMany("FuelHistories")
                        .HasForeignKey("DriverId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Foruscorp.Trucks.Domain.Drivers.Driver", b =>
                {
                    b.HasOne("Foruscorp.Trucks.Domain.Trucks.Truck", "Truck")
                        .WithOne("Driver")
                        .HasForeignKey("Foruscorp.Trucks.Domain.Drivers.Driver", "TruckId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.OwnsOne("Foruscorp.Trucks.Domain.Drivers.Contact", "Contact", b1 =>
                        {
                            b1.Property<Guid>("DriverId")
                                .HasColumnType("uuid");

                            b1.Property<string>("Email")
                                .HasMaxLength(100)
                                .HasColumnType("character varying(100)");

                            b1.Property<string>("Phone")
                                .HasMaxLength(50)
                                .HasColumnType("character varying(50)")
                                .HasColumnName("PhoneNumber");

                            b1.Property<string>("TelegramLink")
                                .HasMaxLength(100)
                                .HasColumnType("character varying(100)");

                            b1.HasKey("DriverId");

                            b1.ToTable("Drivers", "Tuck");

                            b1.WithOwner()
                                .HasForeignKey("DriverId");
                        });

                    b.Navigation("Contact");

                    b.Navigation("Truck");
                });

            modelBuilder.Entity("Foruscorp.Trucks.Domain.Drivers.DriverBonus", b =>
                {
                    b.HasOne("Foruscorp.Trucks.Domain.Drivers.Driver", null)
                        .WithMany("Bonuses")
                        .HasForeignKey("DriverId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Foruscorp.Trucks.Domain.RouteOffers.RouteOffer", b =>
                {
                    b.HasOne("Foruscorp.Trucks.Domain.Drivers.Driver", "Driver")
                        .WithMany()
                        .HasForeignKey("DriverId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Driver");
                });

            modelBuilder.Entity("Foruscorp.Trucks.Domain.Drivers.Driver", b =>
                {
                    b.Navigation("Bonuses");

                    b.Navigation("FuelHistories");
                });

            modelBuilder.Entity("Foruscorp.Trucks.Domain.Trucks.Truck", b =>
                {
                    b.Navigation("Driver");
                });
#pragma warning restore 612, 618
        }
    }
}
