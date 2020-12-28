using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BotCheck
{
    public enum Status
    {
        Add=1,
        Choose=2,
        Free=3,
    }
    public class User:ICloneable
    {
        [Key]
        public int Id { get; private set; }
        public int TelId { get; private set; }
        public int Stat { get;private set; }
        [NotMapped]
        public Status Status { get
            {
                return (Status)Stat;
            }
            private set
            {
                Stat = (int)value;
            }
        }

        
        public string UserName { get; private set; }

        [NotMapped]
        public List<Connection>? Connections
        {
            get
            {
                using (var db = new ConnectContext())
                {
                    return db.Conn.ToList().Where(x => x.Tel == this.TelId).ToList();
                }
            }
        }
        public void ChangeStatus(Status newStatus)
        {
            Stat = (int)newStatus;
        }
        private User()
        {

        }
        public User(int id, string name)
        {
            
            TelId = id;
            UserName = name;

        }
        public override bool Equals(object obj)
        {
            return TelId == ((User)obj).TelId;
        }

        public object Clone()
        {
            var result = new User(TelId, UserName);
            return result;
        }
    }
}
