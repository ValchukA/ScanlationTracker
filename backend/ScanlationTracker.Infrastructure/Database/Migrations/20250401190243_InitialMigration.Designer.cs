﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ScanlationTracker.Infrastructure.Database;

#nullable disable

namespace ScanlationTracker.Infrastructure.Database.Migrations
{
    [DbContext(typeof(ScanlationDbContext))]
    [Migration("20250401190243_InitialMigration")]
    partial class InitialMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ScanlationTracker.Infrastructure.Database.Entities.Chapter", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("AddedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ExternalId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Number")
                        .HasColumnType("integer");

                    b.Property<Guid>("SeriesId")
                        .HasColumnType("uuid");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("SeriesId", "ExternalId")
                        .IsUnique();

                    b.ToTable("Chapters");
                });

            modelBuilder.Entity("ScanlationTracker.Infrastructure.Database.Entities.ScanlationGroup", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("BaseCoverUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("BaseWebsiteUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PublicName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("ScanlationGroups");
                });

            modelBuilder.Entity("ScanlationTracker.Infrastructure.Database.Entities.Series", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ExternalId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("RelativeCoverUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("ScanlationGroupId")
                        .HasColumnType("uuid");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ScanlationGroupId", "ExternalId")
                        .IsUnique();

                    b.ToTable("Series");
                });

            modelBuilder.Entity("ScanlationTracker.Infrastructure.Database.Entities.SeriesTracking", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("SeriesId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.HasIndex("SeriesId", "UserId")
                        .IsUnique();

                    b.ToTable("SeriesTrackings");
                });

            modelBuilder.Entity("ScanlationTracker.Infrastructure.Database.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ScanlationTracker.Infrastructure.Database.Entities.Chapter", b =>
                {
                    b.HasOne("ScanlationTracker.Infrastructure.Database.Entities.Series", "Series")
                        .WithMany()
                        .HasForeignKey("SeriesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Series");
                });

            modelBuilder.Entity("ScanlationTracker.Infrastructure.Database.Entities.Series", b =>
                {
                    b.HasOne("ScanlationTracker.Infrastructure.Database.Entities.ScanlationGroup", "ScanlationGroup")
                        .WithMany()
                        .HasForeignKey("ScanlationGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ScanlationGroup");
                });

            modelBuilder.Entity("ScanlationTracker.Infrastructure.Database.Entities.SeriesTracking", b =>
                {
                    b.HasOne("ScanlationTracker.Infrastructure.Database.Entities.Series", "Series")
                        .WithMany()
                        .HasForeignKey("SeriesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ScanlationTracker.Infrastructure.Database.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Series");

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}
