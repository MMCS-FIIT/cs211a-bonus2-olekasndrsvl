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
        using (var sw = new StreamWriter("global_log.txt", true))
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
               sticker: new Telegram.Bot.Types.InputFileUrl("https://raw.githubusercontent.com/MMCS-FIIT/cs211a-bonus2-olekasndrsvl/main/SimpleTGBot/Resources/sticker_interested.webp"),
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

            case "Коротко о главном":

                _ = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "<b>Коротко о главном</b> \r\n\r\nДанный текст содержит краткий обзор особенностей PascalABC.NET. \r\n\r\nPascalABC.NET – легковесная и мощная среда разработки программ с подробной справочной системой, средствами подсказки по коду, автоформатированием, встроенным отладчиком и встроенным дизайнером форм. Интегрированная среда разработки PascalABC.NET ориентирована на создание проектов малой и средней сложности, а также на обучение современному программированию. \r\nPascalABC.NET – мощный и современный язык программирования. По предоставляемым возможностям он превосходит язык Delphi и содержит практически все возможности языка C#. \r\nPascalABC.NET опирается на платформу Microsoft .NET - её языковые возможности и библиотеки, что делает его гибким, эффективным, постоянно развивающимся. Кроме того, можно легко сочетать библиотеки, разработанные на PascalABC.NET и других .NET-языках. \r\nКомпилятор PascalABC.NET генерирует код, выполняющийся так же быстро, как и код на C#, и несколько медленнее, чем код на C++ и Delphi. \r\nPascalABC.NET является представителем линейки современных языков Паскаль вместе с Delphi и Oxygene. \r\nМнение, что язык Паскаль устарел и утрачивает свои позиции, основано на представлении о старом Паскале и старых средах программирования (например, Free Pascal с его несовременной консольной оболочкой). К сожалению, масса отечественной учебной литературы с упорством, достойным лучшего применения, ориентируется на отживший Turbo Pascal с древней консольной оболочкой, бедной графической библиотекой и устаревшими средствами объектно-ориентированного программирования, развивая у обучающихся стойкое отвращение к языку Паскаль вообще. \r\nPascalABC.NET расширен современными языковыми возможностями для легкого, компактного и понятного програмирования. \r\nPascalABC.NET – достаточно зрелая среда. Ее прототип – учебная система Pascal ABC – появилась в 2002 году. PascalABC.NET – развивающаяся среда. Ведутся разработки новых языковых возможностей, новых библиотек. \r\nДалее приводится ряд программ с короткими комментариями, раскрывающих возможности и особенности языка PascalABC.NET.  \r\n\r\nТексты программ располагаются в рабочей папке (по умолчанию C:\\PABCWork.NET) в подпапке Samples\\!MainFeatures.\r\n\r\nДля запуска программ данное окно должно быть открыто через пункт меню \"Помощь/Коротко о главном\" так, чтобы оно не полностью закрывало окно оболочки PascalABC.NET.\r\n",
                replyMarkup: replyKeyboardMarkup,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Содержание \"коротко о главном\" ");

               Message message2 = await botClient.SendTextMessageAsync(
               chatId: chatId,
               text: "Ознакомиться более подробно со справкой по языку PascalABC.NET Вы всегда можете по ссылке:\n https://pascalabc.net/downloads/pabcnethelp/index.htm ",
               replyMarkup: replyKeyboardMarkup,
               parseMode: ParseMode.Html,
               cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Reminding message  ");




                break;

            case "Отличия языка PascalABC.NET от Delphi":
                _ = await botClient.SendTextMessageAsync(
               chatId: chatId,
               text: "<b>Отличия языка PascalABC.NET от Delphi</b>\r\n<i>Добавлено</i>\r\nОперации += -= для событий .NET и для процедурных переменных. \r\nОперации += -= *= для целых и  += -= *= /= для вещественных. \r\nОперация += для строк. \r\nПодпрограммы с переменным числом параметров. \r\nОперация new для вызова конструктора (ident := new type_name(params);). \r\nОперация new для создания динамического массива. \r\nОперация typeof . \r\nИспользование uses для подключения пространств имен .NET (реализовано в Delphi Prism). \r\nВид доступа internal (наряду с public, private, protected). \r\nИнициализация переменных: var a: integer := 1; \r\nИнициализация переменных: var a := 1; \r\nОбъявление локальных переменных в блоке. \r\nОбъявление параметра цикла в заголовке цикла: for var i := 1 to 10 do, foreach var x in a do. \r\nОператор lock, обеспечивающий синхронизацию потоков. \r\nМетоды в записях. \r\nИнициализаторы полей в классах и записях. \r\nОбобщенные классы (generics). \r\nРеализованы типизированные файлы (в отличие от Delphi Prism, где они убраны). \r\nУпрощенный синтаксис модулей. \r\nОписание методов внутри интерфейса класса или записи. \r\nРеализация записью интерфейса. \r\nМетоды расширения. \r\nЛямбда-выражения. \r\n<i>Изменено</i>\r\nТолько сокращенное вычисление логических выражений. \r\nДругой синтаксис foreach. \r\nИнтерфейсы interface в стиле .NET. \r\nДругой синтаксис перегрузки операций. \r\nСтатические методы классов вместо классовых методов. Отсутствие типа TClass. \r\nДеструкторы оставлены лишь для совместимости и не выполняют никаких действий. \r\nТип object - синоним System.Object. \r\nТип exception - синоним System.Exception. \r\nИндексация string с 1, директива переключения на индексацию с 0. \r\nПроцедура Write выводит любые типы. \r\nСтруктурная эквивалентность типов для процедурных переменных, динамических массивов, типизированных указателей и множеств (в Delphi Object Pascal - именная эквивалентность типов за исключением открытых массивов). \r\nМножества на базе произвольных типов (set of string). \r\nЗапрет использования указателей на управляемую память. \r\nПроцедурные переменные (делегаты) вместо procedure of object. \r\nС бестиповыми файлами file можно работать с помощью процедур Read, Write. \r\nМассивы массивов отличаются по типу от двумерных массивов (в частности, записи a[i][j] и a[i,j] неэквивалентны). \r\nПерегрузка выполняется без ключевого слова overload. \r\nВсе конструкторы имеют имя Create. \r\nАвтоматическое управление памятью с помощью сборщика мусора (за исключением указателей на неуправляемую память). \r\n",
               replyMarkup: replyKeyboardMarkup,
               parseMode: ParseMode.Html,
               cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Содержание \"Отличия языка PascalABC.NET от Delphi\" ");
               Message messager = await botClient.SendTextMessageAsync(
               chatId: chatId,
               text: "Ознакомиться более подробно со справкой по языку PascalABC.NET Вы всегда можете по ссылке:\n https://pascalabc.net/downloads/pabcnethelp/index.htm ",
               replyMarkup: replyKeyboardMarkup,
               parseMode: ParseMode.Html,
               cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Reminding message  ");


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