using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Repositories;

namespace SGCAE.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Credential> Credentials => Set<Credential>();
    public DbSet<AccessPoint> AccessPoints => Set<AccessPoint>();
    public DbSet<AccessLog> AccessLogs => Set<AccessLog>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}

