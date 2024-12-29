using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Basketball_LiveScore.Server.Models;

namespace Basketball_LiveScore.Server.Data
{
    public class BasketballDBContext : DbContext
    {
        public BasketballDBContext (DbContextOptions<BasketballDBContext> options)
            : base(options)
        {
        }

        public DbSet<Player> Players { get; set; } = default!;
        public DbSet<Team> Teams { get; set; } = default!;
        public DbSet<Match> Matches { get; set; } = default!;

        public DbSet<User> Users { get; set; } = default!;

        public DbSet<MatchEvent> MatchEvents { get; set; }

        public DbSet<MatchPlayer> MatchPlayers { get; set; }
        public DbSet<Quarter> Quarters { get; set; } = default!;
        public DbSet<Models.Timeout> Timeouts { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Configure les entités Identity (Users, Roles, etc.)

            //On précise les tables car sinon va créer des tables Matches, Teams et Players
            modelBuilder.Entity<Team>().ToTable("Team");
            modelBuilder.Entity<Match>().ToTable("Match");
            modelBuilder.Entity<Player>().ToTable("Player");
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<MatchEvent>().ToTable("MatchEvent");
            modelBuilder.Entity<MatchPlayer>().ToTable("MatchPlayer");
            modelBuilder.Entity<Quarter>().ToTable("Quarter");
            modelBuilder.Entity<Models.Timeout>().ToTable("Timeout");
            modelBuilder.Entity<BasketEvent>().Property(e => e.PlayerId).HasColumnName("BasketEvent_PlayerId");
            modelBuilder.Entity<BasketEvent>().Property(e => e.Points).HasColumnName("BasketEvent_Points");
            modelBuilder.Entity<FoulEvent>().Property(e => e.PlayerId).HasColumnName("FoulEvent_PlayerId");
            modelBuilder.Entity<FoulEvent>().Property(e => e.FoulType).HasColumnName("FoulEvent_FoulType");

            modelBuilder.Entity<Player>()
                .HasOne(p => p.Team)         // Un joueur a une équipe
                .WithMany(t => t.Players)   // Une équipe a plusieurs joueurs
                .HasForeignKey(p => p.TeamId) // Clé étrangère
                .OnDelete(DeleteBehavior.Cascade); // Supprime les joueurs si l'équipe est supprimée

            modelBuilder.Entity<MatchEvent>()
                .ToTable("MatchEvent") //Tous les events dans une seule table !
                .HasDiscriminator<string>("EventType")
                .HasValue<BasketEvent>("Basket") 
                .HasValue<FoulEvent>("Foul")
                .HasValue<SubstitutionEvent>("Substitution")
                .HasValue<TimeoutEvent>("Timeout")
                .HasValue<ChronoEvent>("Chrono")
                .HasValue<QuarterChangeEvent>("QuarterChange");

            modelBuilder.Entity<MatchPlayer>()
                .HasKey(mp => mp.Id);

            modelBuilder.Entity<MatchPlayer>()
                .HasOne(mp => mp.Match)
                .WithMany(m => m.MatchPlayers)
                .HasForeignKey(mp => mp.MatchId);

            modelBuilder.Entity<MatchPlayer>()
                .HasOne(mp => mp.Player)
                .WithMany()
                .HasForeignKey(mp => mp.PlayerId);

            modelBuilder.Entity<Quarter>()
                .ToTable("Quarter")
                .HasKey(q => q.Id);

            modelBuilder.Entity<Quarter>()
                .HasOne(q => q.Match)
                .WithMany(m => m.Quarters)
                .HasForeignKey(q => q.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Models.Timeout>()
                .ToTable("Timeout")
                .HasKey(t => t.Id);

            modelBuilder.Entity<Models.Timeout>()
                .HasOne(t => t.Match)
                .WithMany(m => m.Timeouts)
                .HasForeignKey(t => t.MatchId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
