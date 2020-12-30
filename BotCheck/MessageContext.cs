using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotCheck
{
    class MessageContext:DbContext
    {


        public DbSet<Message> Messages { get; set; }
        public MessageContext()
        {
            Database.EnsureCreated();

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=tcp:vikos-server.database.windows.net,1433;Initial Catalog=messagedb;Persist Security Info=False;User ID=vikgork;Password=Dfktynbyf18;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        }
    }
}
