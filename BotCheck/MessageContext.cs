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
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=messagedb;Trusted_Connection=True;");
        }
    }
}
