using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading;

namespace BotCheck
{
   public class BotControler
    {
        TelegramBotClient client = new TelegramBotClient("1424055958:AAG3c_w2eXfYJSClZbeSQbT2oLJakFUnj2g");
        MessageReader reader;
        List<MessageSender> worker;
        bool IsWork;
        MessageContext context;
        public BotControler()
        {
            IsWork = false;
            context = new MessageContext();
            context.SavedChanges += MessageContext_SavedChanges;
            reader = new MessageReader(client,context);
            worker = new List<MessageSender>();
            
        }

        private void MessageContext_SavedChanges(object sender, Microsoft.EntityFrameworkCore.SavedChangesEventArgs e)
        {
            Task.Run(TreatmentMessage);

        }

        private void TreatmentMessage()
        {
            List<Message> newMess;
            using (var db = new MessageContext())
            {
                newMess = db.Messages.Where(x => !x.IsTreatment).ToList();
            }
            List<Message> result = new List<Message>(); ;
            Parallel.ForEach(newMess,async (mess) =>
            {
                var freeWorker = worker.Where(x => !x.IsWork);
                if (freeWorker.Count() == 0 || freeWorker == null)
                {
                    worker.Add(new MessageSender(client));
                    freeWorker = worker.Where(x => !x.IsWork);
                }
                var res = await  freeWorker.First().Work(mess);
                result.Add(res);
                lock (new object())
                {
                    using(var db = new MessageContext())
                    {
                        db.Messages.Find(res.Id).Treatment();
                        db.SaveChanges();
                    }
                }
                
            });
            using(var db = new MessageContext())
            {
                result.ForEach(x =>
                {
                    db.Messages.Find(x.Id).Treatment();
                });
                    db.SaveChanges();
            }
        }

        public void Start()
        {
            IsWork = true;
            
            reader.Start();
            Console.WriteLine("Start");
            while (IsWork) ;
        }

        public void Stop()
        {
            IsWork = false;
        }
    }
}
