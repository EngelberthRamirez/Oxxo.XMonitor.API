using ApplicationCore.Common.Models;
using ApplicationCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApplicationCore.Infrastructure.Persistence.Context
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<RequestConfig> RequestConfig => Set<RequestConfig>();
        public IQueryable<GetStoreDataClass> StoreDataByUser(int userId, bool active) => FromExpression(() => StoreDataByUser(userId, active));

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasDbFunction(typeof(ApplicationDbContext)
                .GetMethod(nameof(StoreDataByUser),
                   [typeof(int), typeof(bool)])!)
                .HasName("GetStoreData");

            modelBuilder.Entity<GetStoreDataClass>().HasNoKey();
            base.OnModelCreating(modelBuilder);
        }
    }


}
