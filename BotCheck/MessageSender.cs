using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
namespace BotCheck
{
    class MessageSender
    {
        Message message;
        MessageType messtype;
        User user;
        InlineKeyboardMarkup keyboard;
        TelegramBotClient client;
        public bool IsWork { get; private set; }

        public MessageSender(TelegramBotClient client)
        {
            this.client = client;
            IsWork = false;
            keyboard = new InlineKeyboardMarkup(new InlineKeyboardButton[][]{
                new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton()
                    {
                        Text="Добавить подключение",
                        CallbackData="AddTo"
                    }
                },
                new InlineKeyboardButton[]
                {
                    new InlineKeyboardButton()
                    {
                        Text="Просмотреть подключения",
                        CallbackData="Show"
                    }
                }
            });
        }

        public async Task<Message> Work(Message mess)
        {
            IsWork = true;
            message = mess;
            messtype = mess.Type;
            
            using(var db = new UserContext())
            {
                user = db.Users.ToList().Find(x=> x.TelId==mess.TelIdFrom);
            }
            try
            {
                switch (messtype)
                {
                    case MessageType.Start:
                        await Task.Run(RegisterUser);
                        break;
                    case MessageType.PassFromUser:
                        await Task.Run(AddPass);
                        break;
                    case MessageType.GetStatusFor:
                        await Task.Run(GetStatus);
                        break;
                    case MessageType.Other:
                        await Task.Run(Other);
                        break;
                    case MessageType.AddTo:
                        await Task.Run(AddTo);
                        break;
                    case MessageType.Choose:
                        await Task.Run(Choose);
                        break;

                }
                

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
                
            IsWork = false;
            return message;
        }
        private async void RegisterUser()
        {
            
            using(var db = new UserContext())
            {
                user = new User(message.TelIdFrom, message.NameFrom);
                if (!db.Users.ToList().Exists(x => x.TelId == user.TelId))
                {
                    
                    db.Users.Add(user);
                    db.SaveChanges();
                }
                else
                    await client.SendTextMessageAsync(user.TelId, "Пользователь существует", replyMarkup: keyboard);
            }
            await client.SendTextMessageAsync(user.TelId,"Выберите действие",replyMarkup: keyboard);
        }
        private async void AddPass()
        {
            try
            {
                string pattern = @"(\AP[\d]{10}\Z)";
                var passwords = message.Text.Split(' ').Where(x => Regex.IsMatch(x, pattern)).Distinct();
                using(var conc = new ConnectContext())
                {
                    var local = conc.Conn.ToList().Select(x=> { return x.Pass; });
                    passwords = passwords.Except(local);
                }
                Parallel.ForEach(passwords,async (x)=> { 
                    try{
                        string name;
                        using (var pars = new DtekParser())
                        {
                            pars.Authorize(x);
                            name=pars.GetName();
                        }
                        var connection = new Connection(user.TelId, x, name);
                        using(var conc = new ConnectContext())
                        {
                            conc.Conn.Add(connection);
                            conc.SaveChanges();
                        }
                        
                    }
                    catch(Exception ex)
                    {
                        Console.Beep();
                        Console.Beep();
                        await client.SendTextMessageAsync(user.TelId, ex.Message);
                    }
                });
                
                using (var db = new UserContext())
                {
                    db.Users.ToList().Find(x => x.TelId == user.TelId).ChangeStatus(Status.Free);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now}|{ex.Message}|{message.Text}|{user.TelId}");
                await client.SendTextMessageAsync(user.TelId, "Произошла ошибка, попробуйте ещё раз");
            }
            await client.SendTextMessageAsync(user.TelId, "Успешно добавлено");
        }
        private async void AddTo()
        {
            try
            {
                await client.SendTextMessageAsync(user.TelId, "Введите номер подключения \"P1111111111\"");
                using(var db = new UserContext())
                {
                    db.Users.ToList().Find(x=> x.TelId==user.TelId).ChangeStatus(Status.Add);
                    db.SaveChanges();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{DateTime.Now}|{ex.Message}");
            }
        }
        private async void GetStatus()
        {

            try
            {
                string pattern = @"([\d,-]{1,})";
                string txt = message.Text;
                while (txt.Contains("--") || txt.Contains(",,"))
                    txt = txt.Replace(",,", ",").Replace("--", "-");
                if (!Regex.IsMatch(txt, pattern))
                    return;
                List<int> pages = new List<int>();
                if (txt.Contains(","))
                {
                    txt.Split(',').ToList().ForEach(x =>
                    {
                        if (x.Contains('-'))
                        {
                            int first = int.Parse(x.Split('-')[0]);
                            int last = int.Parse(x.Split('-')[1]);
                            //3-5
                            pages.AddRange(Enumerable.Range(first, last - first + 1));
                        }
                        else
                            pages.Add(int.Parse(x));
                    });
                }
                else
                {
                    if (txt.Contains('-'))
                    {
                        int first = int.Parse(txt.Split('-')[0]);
                        int last = int.Parse(txt.Split('-')[1]);
                        //3-5
                        pages.AddRange(Enumerable.Range(first, last - first + 1));
                    }
                    else
                    {
                        pages.Add(int.Parse(txt));
                    }
                }
               

                var passwords = pages.Select(x =>
                {
                    return user.Connections[x-1];
                }).ToList();
                await client.SendTextMessageAsync(user.TelId, "Ожидайте");
                try
                {
                    Parallel.ForEach(passwords, (x) =>
                    {
                        using (var pars = new DtekParser())
                        {
                            pars.Authorize(x.Pass);
                            var result = pars.GetInfo();
                            client.SendTextMessageAsync(user.TelId, result);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.Beep();
                    Console.WriteLine(ex.Message);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            using (var db = new UserContext())
            {
                db.Users.ToList().Find(x => x.TelId == user.TelId).ChangeStatus(Status.Free);
                db.SaveChanges();
            }
        }

        private async void Choose()
        {
            var connections = user.Connections;
            var result = String.Join("\n", connections.Select(x => { return $"{connections.IndexOf(x) + 1}|{x.Name}"; } ));
            client.SendTextMessageAsync(user.TelId,result);
            await client.SendTextMessageAsync(user.TelId,"Выберите номера которые хотите просмотреть\"1-3,6,9-11 \"");
            using(var db = new UserContext())
            {
                db.Users.ToList().Find(x=> x.TelId==user.TelId).ChangeStatus(Status.Choose);
                db.SaveChanges();
            }
        }
        private async void Other()
        {
            await client.SendTextMessageAsync(user.TelId, "Выберите действие", replyMarkup: keyboard);
        }
    }
}
