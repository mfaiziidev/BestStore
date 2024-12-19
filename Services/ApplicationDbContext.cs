using BestStore.Models;
using Microsoft.EntityFrameworkCore;

namespace BestStore.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        
        public DbSet<clsProduct> Products { get; set; }
    }
}
