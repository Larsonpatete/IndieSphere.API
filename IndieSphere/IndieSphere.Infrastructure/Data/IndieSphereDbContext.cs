using IndieSphere.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace IndieSphere.Infrastructure.Data;

public class IndieSphereDbContext : DbContext
{
    public IndieSphereDbContext(DbContextOptions<IndieSphereDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
}