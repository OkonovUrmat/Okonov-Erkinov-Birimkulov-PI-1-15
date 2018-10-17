using System;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ApiAiSDK.Model;
using ApiAiSDK;

namespace Telegram_Bot_Console
{
    class Program
    {
        static TelegramBotClient Bot;
        static ApiAi apiAi;

        static void Main(string[] args)
        {
            Bot = new TelegramBotClient("638687925:AAH1-dP7iqNfQ0UsBFcJEWqqj274oIVmPWw");
            AIConfiguration config = new AIConfiguration("6b8c414af71a49aaa5025565f213c3cd", SupportedLanguage.Russian);
            apiAi = new ApiAi(config);

            Bot.OnMessage += Bot_OnMessageReceived;
            Bot.OnCallbackQuery += Bot_OnCallbackQueryReceived;

            var me = Bot.GetMeAsync().Result;

            Console.WriteLine(me.FirstName);

            Bot.StartReceiving();

            Console.ReadLine();

            Bot.StartReceiving();
        }

        private static async void Bot_OnCallbackQueryReceived(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            string buttonText = e.CallbackQuery.Data;
            string name = $"{e.CallbackQuery.From.FirstName} {e.CallbackQuery.From.LastName}";
            Console.WriteLine($"{name} нажал кнопку {buttonText}");

            if (buttonText == "Картинка")
            {
                await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "https://www.jpl.nasa.gov/spaceimages/images/wallpaper/PIA17563-1920x1200.jpg");
            }
            else if (buttonText == "Видео")
            {
                await Bot.SendTextMessageAsync(e.CallbackQuery.From.Id, "https://www.youtube.com/watch?v=W62aVty77Lw");
            }
            await Bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id, $"Вы нажали кнопку {buttonText}");
        }

        private static async void Bot_OnMessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var message = e.Message;

            if (message == null || message.Type != MessageType.Text)
                return;    

            string name = $"{message.From.FirstName} {message.From.LastName}";

            Console.WriteLine($"{name} отправил сообщение: '{message.Text}'");

            switch (message.Text)
            {
                case "/start":
                    string text = 
                        @"Список команд:
                        /start - запуск бота
                        /inline - вывод меню
                        /keyboard - вывод клавиатуры
                        ";
                    await Bot.SendTextMessageAsync(message.From.Id, text);
                    break;

                case "/inline":
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new []
                        {
                            InlineKeyboardButton.WithUrl("Instagram","https://www.instagram.com/nbamemes/"),
                            InlineKeyboardButton.WithUrl("Telegram","https://web.telegram.org/#/im?p=@BotFather"),
                        },

                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Картинка"),
                            InlineKeyboardButton.WithCallbackData("Видео")
                        }
                    });
                    await Bot.SendTextMessageAsync(message.From.Id, "Выберите пункт меню",
                            replyMarkup: inlineKeyboard);
                    break;

                case "/keyboard":
                    var replyKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                    new[]
                    {
                        new KeyboardButton("Привет"),
                        new KeyboardButton("Как дела?")
                    },
                    new[]
                    {
                        new KeyboardButton("Контакт") { RequestContact = true },
                        new KeyboardButton("Геолакация") {RequestLocation = true }
                    }
                    });
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Сообщение",
                        replyMarkup: replyKeyboard);
                    break;

                default:
                    var response = apiAi.TextRequest(message.Text);
                    string answer = response.Result.Fulfillment.Speech;
                    if (answer == "")

                        answer = "Я не понял что ты сказал";
                    await Bot.SendTextMessageAsync(message.From.Id, answer);

                    break;
            }
        }
    }
}
