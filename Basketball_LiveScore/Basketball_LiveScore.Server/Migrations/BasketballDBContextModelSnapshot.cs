﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Basketball_LiveScore.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Basketball_LiveScore.Server.Migrations
{
    [DbContext(typeof(BasketballDBContext))]
    partial class BasketballDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.Match", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AwayTeamId")
                        .HasColumnType("integer");

                    b.Property<List<int>>("AwayTeamStartingFiveIds")
                        .IsRequired()
                        .HasColumnType("integer[]");

                    b.Property<int>("EncoderRealTimeId")
                        .HasColumnType("integer");

                    b.Property<int>("EncoderSettingsId")
                        .HasColumnType("integer");

                    b.Property<int>("HomeTeamId")
                        .HasColumnType("integer");

                    b.Property<List<int>>("HomeTeamStartingFiveIds")
                        .IsRequired()
                        .HasColumnType("integer[]");

                    b.Property<int>("ScoreAway")
                        .HasColumnType("integer");

                    b.Property<int>("ScoreHome")
                        .HasColumnType("integer");

                    b.Property<DateTime>("matchDate")
                        .HasMaxLength(10)
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("AwayTeamId");

                    b.HasIndex("HomeTeamId");

                    b.ToTable("Match", (string)null);
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.MatchEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasMaxLength(13)
                        .HasColumnType("character varying(13)");

                    b.Property<int>("MatchId")
                        .HasColumnType("integer");

                    b.Property<int>("Quarter")
                        .HasColumnType("integer");

                    b.Property<TimeSpan>("Time")
                        .HasColumnType("interval");

                    b.HasKey("Id");

                    b.HasIndex("MatchId");

                    b.ToTable("MatchEvent", (string)null);

                    b.HasDiscriminator<string>("EventType").HasValue("MatchEvent");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.MatchPlayer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("IsHomeTeam")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsStarter")
                        .HasColumnType("boolean");

                    b.Property<int>("MatchId")
                        .HasColumnType("integer");

                    b.Property<int>("PlayerId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("MatchId");

                    b.HasIndex("PlayerId");

                    b.ToTable("MatchPlayer", (string)null);
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<double>("Height")
                        .HasColumnType("double precision");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<int>("Number")
                        .HasColumnType("integer");

                    b.Property<int>("Position")
                        .HasColumnType("integer");

                    b.Property<int>("TeamId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TeamId");

                    b.ToTable("Player", (string)null);
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.Quarter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Duration")
                        .HasColumnType("integer");

                    b.Property<int>("MatchId")
                        .HasColumnType("integer");

                    b.Property<int>("Number")
                        .HasColumnType("integer");

                    b.Property<TimeSpan>("RemainingTime")
                        .HasColumnType("interval");

                    b.HasKey("Id");

                    b.HasIndex("MatchId");

                    b.ToTable("Quarter");
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.Team", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Coach")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.ToTable("Team", (string)null);
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.Timeout", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Duration")
                        .HasColumnType("integer");

                    b.Property<int>("MatchId")
                        .HasColumnType("integer");

                    b.Property<int>("QuarterNumber")
                        .HasColumnType("integer");

                    b.Property<string>("Team")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<TimeSpan>("TimeStamp")
                        .HasColumnType("interval");

                    b.HasKey("Id");

                    b.HasIndex("MatchId");

                    b.ToTable("Timeout");
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("User", (string)null);
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.BasketEvent", b =>
                {
                    b.HasBaseType("Basketball_LiveScore.Server.Models.MatchEvent");

                    b.Property<int>("PlayerId")
                        .HasColumnType("integer")
                        .HasColumnName("BasketEvent_PlayerId");

                    b.Property<int>("Points")
                        .HasColumnType("integer")
                        .HasColumnName("BasketEvent_Points");

                    b.HasDiscriminator().HasValue("Basket");
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.ChronoEvent", b =>
                {
                    b.HasBaseType("Basketball_LiveScore.Server.Models.MatchEvent");

                    b.Property<bool>("IsRunning")
                        .HasColumnType("boolean");

                    b.HasDiscriminator().HasValue("Chrono");
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.FoulEvent", b =>
                {
                    b.HasBaseType("Basketball_LiveScore.Server.Models.MatchEvent");

                    b.Property<string>("FoulType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("FoulEvent_FoulType");

                    b.Property<int>("PlayerId")
                        .HasColumnType("integer")
                        .HasColumnName("FoulEvent_PlayerId");

                    b.HasDiscriminator().HasValue("Foul");
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.QuarterChangeEvent", b =>
                {
                    b.HasBaseType("Basketball_LiveScore.Server.Models.MatchEvent");

                    b.HasDiscriminator().HasValue("QuarterChange");
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.SubstitutionEvent", b =>
                {
                    b.HasBaseType("Basketball_LiveScore.Server.Models.MatchEvent");

                    b.Property<int>("PlayerInId")
                        .HasColumnType("integer");

                    b.Property<int>("PlayerOutId")
                        .HasColumnType("integer");

                    b.HasDiscriminator().HasValue("Substitution");
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.TimeoutEvent", b =>
                {
                    b.HasBaseType("Basketball_LiveScore.Server.Models.MatchEvent");

                    b.Property<string>("Team")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasDiscriminator().HasValue("Timeout");
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.Match", b =>
                {
                    b.HasOne("Basketball_LiveScore.Server.Models.Team", "AwayTeam")
                        .WithMany()
                        .HasForeignKey("AwayTeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Basketball_LiveScore.Server.Models.Team", "HomeTeam")
                        .WithMany()
                        .HasForeignKey("HomeTeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AwayTeam");

                    b.Navigation("HomeTeam");
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.MatchEvent", b =>
                {
                    b.HasOne("Basketball_LiveScore.Server.Models.Match", "Match")
                        .WithMany()
                        .HasForeignKey("MatchId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Match");
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.MatchPlayer", b =>
                {
                    b.HasOne("Basketball_LiveScore.Server.Models.Match", "Match")
                        .WithMany("MatchPlayers")
                        .HasForeignKey("MatchId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Basketball_LiveScore.Server.Models.Player", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Match");

                    b.Navigation("Player");
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.Player", b =>
                {
                    b.HasOne("Basketball_LiveScore.Server.Models.Team", "Team")
                        .WithMany("Players")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Team");
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.Quarter", b =>
                {
                    b.HasOne("Basketball_LiveScore.Server.Models.Match", "Match")
                        .WithMany("Quarters")
                        .HasForeignKey("MatchId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Match");
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.Timeout", b =>
                {
                    b.HasOne("Basketball_LiveScore.Server.Models.Match", "Match")
                        .WithMany("Timeouts")
                        .HasForeignKey("MatchId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Match");
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.Match", b =>
                {
                    b.Navigation("MatchPlayers");

                    b.Navigation("Quarters");

                    b.Navigation("Timeouts");
                });

            modelBuilder.Entity("Basketball_LiveScore.Server.Models.Team", b =>
                {
                    b.Navigation("Players");
                });
#pragma warning restore 612, 618
        }
    }
}
