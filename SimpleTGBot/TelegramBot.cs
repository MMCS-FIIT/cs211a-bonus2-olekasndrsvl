namespace SimpleTGBot;

using System.Runtime.Serialization.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


public class TelegramBot
{
    // Токен TG-бота. Можно получить у @BotFather
    private const string BotToken = "6698708401:AAGIk59DKdFf4o2w6ZFqVsX6JkGxaISf0rk";
    /// <summary>
    /// Инициализирует и обеспечивает работу бота до нажатия клавиши Esc
    /// </summary>


    string path = @"message_log.txt";
    Dictionary<long, List<string>> logbase = new Dictionary<long, List<string>>();
    DataContractJsonSerializer formatter = new DataContractJsonSerializer(typeof(Dictionary<long, List<string>>));

    /// <summary>
    /// Записывает в файл global_log.txt строку
    /// </summary>
    /// <param name="message"></param>
    public async void WriteToLog(string message)
    {
        using (var sw = new StreamWriter("global_log.txt"))
        {
            sw.WriteLine($"[{DateTime.Now}] {message} ");
        }
    }

  


    public async Task Run()
    {
        // Если вам нужно хранить какие-то данные во время работы бота (массив информации, логи бота,
        // историю сообщений для каждого пользователя), то это всё надо инициализировать в этом методе.


        // TODO: Инициализация необходимых полей


        // Инициализируем наш клиент, передавая ему токен.
        var botClient = new TelegramBotClient(BotToken);

        // Служебные вещи для организации правильной работы с потоками
        using CancellationTokenSource cts = new CancellationTokenSource();

        // Разрешённые события, которые будет получать и обрабатывать наш бот.
        // Будем получать только сообщения. При желании можно поработать с другими событиями.
        ReceiverOptions receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = new[] { UpdateType.Message }
        };

        // Привязываем все обработчики и начинаем принимать сообщения для бота
        botClient.StartReceiving(
            updateHandler: OnMessageReceived,
            pollingErrorHandler: OnErrorOccured,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        // Проверяем что токен верный и получаем информацию о боте
        var me = await botClient.GetMeAsync(cancellationToken: cts.Token);
        Console.WriteLine($"Бот @{me.Username} запущен.\nДля остановки нажмите клавишу Esc...");

        // Ждём, пока будет нажата клавиша Esc, тогда завершаем работу бота
        while (Console.ReadKey().Key != ConsoleKey.Escape) { }

        // Отправляем запрос для остановки работы клиента.
        cts.Cancel();
    }

    /// <summary>
    /// Обработчик события получения сообщения.
    /// </summary>
    /// <param name="botClient">Клиент, который получил сообщение</param>
    /// <param name="update">Событие, произошедшее в чате. Новое сообщение, голос в опросе, исключение из чата и т. д.</param>
    /// <param name="cancellationToken">Служебный токен для работы с многопоточностью</param>
    async Task OnMessageReceived(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Работаем только с сообщениями. Остальные события игнорируем
        var message = update.Message;
        if (message is null)
        {
            return;
        }
        // Будем обрабатывать только текстовые сообщения.
        // При желании можно обрабатывать стикеры, фото, голосовые и т. д.
        //
        // Обратите внимание на использованную конструкцию. Она эквивалентна проверке на null, приведённой выше.
        // Подробнее об этом синтаксисе: https://medium.com/@mattkenefick/snippets-in-c-more-ways-to-check-for-null-4eb735594c09
        if (message.Text is not { } messageText)
        {
            return;
        }

        // Получаем ID чата, в которое пришло сообщение. Полезно, чтобы отличать пользователей друг от друга.
        var chatId = message.Chat.Id;

        // Печатаем на консоль факт получения сообщения
        Console.WriteLine($"Получено сообщение в чате {chatId}: '{messageText}'");

        
        if (!logbase.ContainsKey(chatId))
        {
            logbase.Add(chatId, new List<string>());
            logbase[chatId].Add(message.Text);
        }
        else
        {
            logbase[chatId].Add(message.Text);
        }

        // Клавиатура главного меню
        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
           {
                new KeyboardButton[] { "Общие сведения" },
                new KeyboardButton[] { "Справочник по языку" },
                new KeyboardButton[] { "Стандартные модули" },
             })
        {
            ResizeKeyboard = true
        };

        // Клавиатура меню общие сведения
        ReplyKeyboardMarkup replyKeyboardMarkup1 = new(new[]
           {
                new KeyboardButton[] { "О системе PascalABC.NET  " },
                new KeyboardButton[] { "Отличия языка PascalABC.NET от Delphi" },
                new KeyboardButton[] { "Коротко о главном" },
             })
        {
            ResizeKeyboard = true
        };
        // Клавиатура справочник по языку
        ReplyKeyboardMarkup replyKeyboardMarkup2 = new(new[]
           {
                new KeyboardButton[] { "Основы" },
                new KeyboardButton[] { "Типы данных" },
                new KeyboardButton[] { "Операторы" },
                new KeyboardButton[] { "Структурное программирование" },
                new KeyboardButton[] { "Объектно-ориентированное программирование" },
                new KeyboardButton[] {"Функциональное программирование"},
             })
        {
            ResizeKeyboard = true
        };


        // TODO: Обработка пришедших сообщений
        switch (messageText)
        {
            case "/start": // Стартовое сообщение и ознакомление с функционалом бота
                WriteToLog($"User: {chatId}  Message: {messageText}");
               

                Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Выберите в меню, чем я могу быть вам полезен.",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: \"Выберите в меню, чем я могу быть вам полезен.\" ");

                break;

            case "/help": // Стартовое сообщение и ознакомление с функционалом бота
                WriteToLog($"User: {chatId}  Message: {messageText}");


                _ = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Выберите в меню, чем я могу быть вам полезен.",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: \"Выберите в меню, чем я могу быть вам полезен.\" ");

                break;
            case "Общие сведения":

                WriteToLog($"User: {chatId}  Message: {messageText}");
                Message message1 = await botClient.SendStickerAsync(
               chatId: chatId,
               sticker: new Telegram.Bot.Types.InputFileUrl("https://raw.githubusercontent.com/MMCS-FIIT/cs211a-bonus2-olekasndrsvl/main/SimpleTGBot/Resources/sticker_glad.webp"),
               replyMarkup: replyKeyboardMarkup1,
               cancellationToken: cancellationToken);

                _ = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Выберите интересующий вас пункт в меню",
                replyMarkup: replyKeyboardMarkup1,
                cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: \"Выберите интересующий вас пункт в меню\" ");
               

                break;

            case "Справочник по языку":


                WriteToLog($"User: {chatId}  Message: {messageText}");
                _ = await botClient.SendStickerAsync(
               chatId: chatId,
               sticker: new Telegram.Bot.Types.InputFileUrl("https://raw.githubusercontent.com/MMCS-FIIT/cs211a-bonus2-olekasndrsvl/main/SimpleTGBot/Resources/sticker_glad.webp"),
               replyMarkup: replyKeyboardMarkup2,
               cancellationToken: cancellationToken);

                _ = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Выберите интересующий вас пункт в меню",
                replyMarkup: replyKeyboardMarkup2,
                cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: \"Выберите интересующий вас пункт в меню\" ");


               


                break;
                




            default:

                // Отправляем обратно то же сообщение, что и получили
                WriteToLog($"User: {chatId}  Message: {messageText}"); // logging
                _ = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Вы написали: \"" + messageText+ "\" Но я не знаю такой команды! Попробуйте спросить меня о чем-то связанном с PascalABC.NET!  Используйте /help , чтобы узнать что я умею!",
                cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: {"Вы написали: \"" + messageText + "\" Но я не знаю такой команды! Попробуйте спросить меня о чем-то связанном с PascalABC.NET! Используйте /help , чтобы узнать что я умею!"} ");

                

                Message message3 = await botClient.SendStickerAsync(
                chatId: chatId,
                sticker: new Telegram.Bot.Types.InputFileUrl("https://raw.githubusercontent.com/MMCS-FIIT/cs211a-bonus2-olekasndrsvl/main/SimpleTGBot/Resources/sad_sticker.webp"),
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
           

                break;

        }






        // Сериализация JSON файла логов сообщений

        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            formatter.WriteObject(fs, logbase);
        }


       



    }

    /// <summary>
    /// Обработчик исключений, возникших при работе бота
    /// </summary>
    /// <param name="botClient">Клиент, для которого возникло исключение</param>
    /// <param name="exception">Возникшее исключение</param>
    /// <param name="cancellationToken">Служебный токен для работы с многопоточностью</param>
    /// <returns></returns>
    Task OnErrorOccured(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        // В зависимости от типа исключения печатаем различные сообщения об ошибке
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",

            _ => exception.ToString()
        };
        WriteToLog(errorMessage);
        Console.WriteLine(errorMessage);

        // Завершаем работу
        return Task.CompletedTask;
    }
}