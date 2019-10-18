using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Rodkulman.TestemunhaBot
{
    class Program
    {
        private static TelegramBotClient bot;
        private static User me;
        private static Timer tm;
        private static TimeZoneInfo brZone;

        public static void Main(string[] args)
        {
            DB.Load();
            
            brZone = TimeZoneInfo.FindSystemTimeZoneById(@"America/Sao_Paulo");

            bot = new TelegramBotClient(DB.GetKey("Telegram"));            

            bot.OnMessage += BotOnMessageReceived;
            bot.OnReceiveError += BotOnReceiveError;

            me = bot.GetMeAsync().Result;            

            bot.StartReceiving();
            tm = new Timer(TimerTick, null, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(10));

            Console.WriteLine($"Start listening for @{me.Username}");

            Console.ReadLine();

            bot.StopReceiving();
        }

        private static async void TimerTick(object state)
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brZone);

            if (now.Hour == 7 && now.DayOfWeek == DayOfWeek.Friday && DB.BreakfastMessageLastSent != DateTime.Today)
            {
                foreach (var chatId in DB.Chats)
                {
                    await bot.SendTextMessageAsync(chatId, "Hoje tem café da manhã");
                }

                DB.BreakfastMessageLastSent = DateTime.Today;
            }
            else if (now.Hour == 13 && DB.LunchTimeMessageSent != DateTime.Today)
            {
                foreach (var chatId in DB.Chats)
                {
                    await bot.SendTextMessageAsync(chatId, "Hora do almoço");
                }

                DB.LunchTimeMessageSent = DateTime.Today;
            }
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs args)
        {
            var message = args.Message;

            if (message == null) { return; }

            if (message.Type != MessageType.Text) { return; }

            try
            {
                await ProcessMessage(message);
            }
            catch (Exception ex)
            {
                await bot.SendTextMessageAsync(message.Chat.Id, ex.Message);
            }
        }

        private static async Task ProcessMessage(Message message)
        {
            var command = message.Text.Split(' ').First().ToLower();

            switch (command)
            {
                case "/start":
                    DB.AddChat(message.Chat.Id);                    
                    break;
                case "/code":
                    await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                    await bot.SendTextMessageAsync(message.Chat.Id, $"https://github.com/rodkulman/testemunha_bot");                
                    break;
                case "/stop":
                    DB.RemoveChat(message.Chat.Id);                    
                    break;
                default:
                    await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                    await bot.SendTextMessageAsync(message.Chat.Id, $"I'm sorry {message.From.FirstName}, I'm afraid I can't do that.");
                    break;
            }
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs args)
        {
            Console.WriteLine("Received error: {0} — {1}",
                args.ApiRequestException.ErrorCode,
                args.ApiRequestException.Message);
        }
    }
}
