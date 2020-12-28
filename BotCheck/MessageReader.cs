    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot;

namespace BotCheck
{
    internal class MessageReader
    {

        TelegramBotClient client;
        MessageContext cn;
        public MessageReader(TelegramBotClient client,MessageContext context)
        {
            cn = context;
            this.client = client;
            client.OnMessage += Client_OnMessage;
            client.OnCallbackQuery += Client_OnCallbackQuery;
        }

        private void Client_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            var data = e.CallbackQuery.Data;
            User user;
            using (var db = new UserContext())
            {
                user = db.Users.ToList().Find(x => x.TelId == e.CallbackQuery.From.Id);
            }
            switch (data)
            {
                case "AddTo":
                    SaveMessage(MessageType.AddTo, user, "Add");
                    break;
                case "Show":
                    SaveMessage(MessageType.Choose, user,"Show");
                    break;
            }
        }

        private void Client_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var tgmmess = e.Message;
            User user;
            using(var db = new UserContext())
            {
                user = db.Users.ToList().Find(x => x.TelId == tgmmess.From.Id);
            }
            if (user == null)
            {
                user = new User(tgmmess.From.Id, tgmmess.From.Username);
                SaveMessage(MessageType.Start, user, tgmmess.Text);
            }
            else
            {
                switch (user.Status)
                {
                    case Status.Add:
                        SaveMessage(MessageType.PassFromUser, user, tgmmess.Text);
                        break;
                    case Status.Choose:
                        SaveMessage(MessageType.GetStatusFor, user, tgmmess.Text);
                        break;
                    case Status.Free:
                        if (tgmmess.Text == @"/start")
                            SaveMessage(MessageType.Start, user, tgmmess.Text);
                        SaveMessage(MessageType.Other, user, tgmmess.Text);
                        break;

                }
            }
        }
        private void SaveMessage(MessageType type,User user,string txt)
        {
            
            Message message = new Message(type,user, txt);

            using(var db = new MessageContext())
            {
                db.Messages.Add(message);
                db.SaveChanges();
                cn.SaveChanges();
            }
            
            

        }
        public void Start()
        {
            Console.WriteLine("StartReceiving");
            client.StartReceiving();
            Console.WriteLine("StartReceiving2");
        }
    }
}
