using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotCheck
{
    public class ConnectContext:DbContext
    {
        public DbSet<Connection> Conn { get; set; }
        public ConnectContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=db;Trusted_Connection=True;");
        }
    }
    
}
