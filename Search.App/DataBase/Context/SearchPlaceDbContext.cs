using Search.DataBase.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Data;
using System;

namespace Search.App.DataBase.Context
{
    public class SearchDbContext : DbContext
    {
        public SearchDbContext(DbContextOptions opt) :base(opt)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<Source> Sources { get; set; }
    }
}