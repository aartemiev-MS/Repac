using Microsoft.EntityFrameworkCore;
using Repac.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repac.Data
{
    class DatabaseContext : DbContext
    {
        public static DatabaseContext DBInstance;
        private string _databasePath;

        public DbSet<CashRegisterScan> CashRegisterScans { get; set; }
        public DbSet<User> Users { get; set; }

        public DatabaseContext(string databasePath)
        {
            _databasePath = databasePath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={_databasePath}");
        }
    }
}
