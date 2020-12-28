using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BotCheck
{
    public enum MessageType
    {
        Start=1,
        Choose=2,
        AddTo=3,
        PassFromUser=4,
        Other=5,
        GetStatusFor=6
    }
    class Message
    {
        [Key]
        public int Id { get; private set; }

        [NotMapped]
        public MessageType Type { get { return (MessageType)MessType; } }

        public int MessType { get; set; }
        public string Text { get; private set; }

        public bool IsTreatment { get; private set; }

        public string NameFrom { get; private set; }
        public int TelIdFrom {get; private set;}

        private Message(int type, string text)
        {
            MessType = type;
            Text = text;
            IsTreatment = false;
        }
        public Message()
        {

        }
        
        public Message(MessageType type,User from, string text):this((int)type,text)
        {
            NameFrom = from.UserName;
            MessType = (int)type;
            TelIdFrom = from.TelId;
            Text = text;
            IsTreatment = false;
        }
        public void Treatment()
        {
            IsTreatment = true;
        }
    }
}
