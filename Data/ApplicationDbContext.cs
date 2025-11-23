using Microsoft.EntityFrameworkCore;
using CelebrateMeProto.Models;

namespace CelebrateMeProto.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
}