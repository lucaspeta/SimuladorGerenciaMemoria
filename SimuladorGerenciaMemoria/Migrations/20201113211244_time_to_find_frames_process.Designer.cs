﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SimuladorGerenciaMemoria.Models;

namespace SimuladorGerenciaMemoria.Migrations
{
    [DbContext(typeof(SimuladorContext))]
    [Migration("20201113211244_time_to_find_frames_process")]
    partial class time_to_find_frames_process
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SimuladorGerenciaMemoria.Models.Frame", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CapacidadeUtilizada")
                        .HasColumnType("int");

                    b.Property<int>("FrameNumber")
                        .HasColumnType("int");

                    b.Property<int>("FrameSize")
                        .HasColumnType("int");

                    b.Property<bool>("IsInitial")
                        .HasColumnType("bit");

                    b.Property<int>("MemoryID")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ProcessID")
                        .HasColumnType("int");

                    b.Property<long>("RegB")
                        .HasColumnType("bigint");

                    b.Property<int?>("TipoAlg")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("MemoryID");

                    b.HasIndex("ProcessID");

                    b.ToTable("Frames");
                });

            modelBuilder.Entity("SimuladorGerenciaMemoria.Models.Memory", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<long>("FramesQTD")
                        .HasColumnType("bigint");

                    b.Property<long>("FramesSize")
                        .HasColumnType("bigint");

                    b.Property<int>("InitialState")
                        .HasColumnType("int");

                    b.Property<string>("InitialStateVal")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsGeneratedProcessList")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SimulationID")
                        .HasColumnType("int");

                    b.Property<long>("Size")
                        .HasColumnType("bigint");

                    b.Property<int?>("UserID")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("SimulationID");

                    b.HasIndex("UserID");

                    b.ToTable("Memories");
                });

            modelBuilder.Entity("SimuladorGerenciaMemoria.Models.Process", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("MemoryID")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("RegB")
                        .HasColumnType("bigint");

                    b.Property<long>("RegL")
                        .HasColumnType("bigint");

                    b.Property<long>("Size")
                        .HasColumnType("bigint");

                    b.Property<double>("TimeToFindFrame")
                        .HasColumnType("float");

                    b.Property<bool>("isInitial")
                        .HasColumnType("bit");

                    b.HasKey("ID");

                    b.HasIndex("MemoryID");

                    b.ToTable("Processes");
                });

            modelBuilder.Entity("SimuladorGerenciaMemoria.Models.Simulation", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UserID")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("UserID");

                    b.ToTable("Simulations");
                });

            modelBuilder.Entity("SimuladorGerenciaMemoria.Models.User", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(40)")
                        .HasMaxLength(40);

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ID");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("SimuladorGerenciaMemoria.Models.Frame", b =>
                {
                    b.HasOne("SimuladorGerenciaMemoria.Models.Memory", "Memory")
                        .WithMany("Frames")
                        .HasForeignKey("MemoryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SimuladorGerenciaMemoria.Models.Process", "Process")
                        .WithMany("Frames")
                        .HasForeignKey("ProcessID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SimuladorGerenciaMemoria.Models.Memory", b =>
                {
                    b.HasOne("SimuladorGerenciaMemoria.Models.Simulation", "Simulation")
                        .WithMany("Memories")
                        .HasForeignKey("SimulationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SimuladorGerenciaMemoria.Models.User", "User")
                        .WithMany("Memories")
                        .HasForeignKey("UserID");
                });

            modelBuilder.Entity("SimuladorGerenciaMemoria.Models.Process", b =>
                {
                    b.HasOne("SimuladorGerenciaMemoria.Models.Memory", "Memory")
                        .WithMany("Processes")
                        .HasForeignKey("MemoryID");
                });

            modelBuilder.Entity("SimuladorGerenciaMemoria.Models.Simulation", b =>
                {
                    b.HasOne("SimuladorGerenciaMemoria.Models.User", "User")
                        .WithMany("Simulations")
                        .HasForeignKey("UserID");
                });
#pragma warning restore 612, 618
        }
    }
}
