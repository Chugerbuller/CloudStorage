using CloudStore.BL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStore.BL
{
    public class CloudStoreDbContext : DbContext
    {
        private readonly string _connectionString;
        public DbSet<FileModel> Files { get; set; }
        public DbSet<User> Users { get; set; }

        public CloudStoreDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString);
        }
    }
}