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
            optionsBuilder.UseSqlServer("Server=tcp:vikos-server.database.windows.net,1433;Initial Catalog=ConnectionDb;Persist Security Info=False;User ID=vikgork;Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        }
    }
    
}
