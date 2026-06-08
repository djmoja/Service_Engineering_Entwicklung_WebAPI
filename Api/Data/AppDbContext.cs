using Microsoft.EntityFrameworkCore;
using Api.Model;   
namespace Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Car> Cars { get; set; }
}