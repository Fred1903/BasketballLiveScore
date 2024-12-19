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

        public DbSet<Basketball_LiveScore.Server.Models.Player> Player { get; set; } = default!;
        public DbSet<Basketball_LiveScore.Server.Models.Team> Team { get; set; } = default!;
        public DbSet<Basketball_LiveScore.Server.Models.Match> Match { get; set; } = default!;
    }
}
