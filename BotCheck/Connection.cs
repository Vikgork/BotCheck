using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace BotCheck
{
    public class Connection
    {
        [Key]
        public int ConnectionId { get; set; }
        public string Pass { get; private set; }
        public string Name { get; private set; }
        public int Tel { get; set; }

        [NotMapped]
        public User User { get
            {
                using (var db = new UserContext())
                {
                    return db.Users.ToList().Find(x => x.TelId == Tel);
                }
            }
        }
        
        public Connection(int tel ,string pass,string name)
        {
            Tel = tel;
            Pass = pass;
            Name = name;
        }
    }
}
