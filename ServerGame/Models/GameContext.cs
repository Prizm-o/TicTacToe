using Microsoft.EntityFrameworkCore;

namespace ServerGame.Models
{
    public class GameContext : DbContext
    {
        public DbSet<GameResult> GameResults { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=TicTacToe;Username=postgres;Password=postgres");
        }
    }
}
