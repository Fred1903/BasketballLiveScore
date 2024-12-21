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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Configure les entités Identity (Users, Roles, etc.)

            //On précise les tables car sinon va créer des tables Matches, Teams et Players
            modelBuilder.Entity<Team>().ToTable("Team");
            modelBuilder.Entity<Match>().ToTable("Match");
            modelBuilder.Entity<Player>().ToTable("Player");
            modelBuilder.Entity<User>().ToTable("User");

            modelBuilder.Entity<Player>()
                .HasOne(p => p.Team)         // Un joueur a une équipe
                .WithMany(t => t.Players)   // Une équipe a plusieurs joueurs
                .HasForeignKey(p => p.TeamId) // Clé étrangère
                .OnDelete(DeleteBehavior.Cascade); // Supprime les joueurs si l'équipe est supprimée
        }
    }
}
