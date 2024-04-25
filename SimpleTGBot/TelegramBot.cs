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
                new KeyboardButton[] {"Назад на главную"},
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
                new KeyboardButton[] {"Назад на главную"},
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

            case "Назад на главную": // Стартовое сообщение и ознакомление с функционалом бота
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

            case "О системе PascalABC.NET":

                _ = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "<b>Система PascalABC.NET</b>\r\nPascalABC.NET – это система программирования и язык Pascal нового поколения для платформы Microsoft .NET. Язык PascalABC.NET содержит все основные элементы современных языков программирования: модули, классы, перегрузку операций, интерфейсы, исключения, обобщенные классы, сборку мусора, лямбда-выражения, а также некоторые средства параллельности, в том числе директивы OpenMP. Система PascalABC.NET включает в себя также простую интегрированную среду, ориентированную на эффективное обучение современному программированию. \r\n\r\n",
            replyMarkup: replyKeyboardMarkup1,
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Содержание \"Основы PascalABC.NET\" ");
                _ = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Ознакомиться более подробно с особенностями языка PascalABC.NET Вы всегда можете по ссылке:\n https://pascalabc.net/downloads/pabcnethelp/index.htm ",
                replyMarkup: replyKeyboardMarkup1,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Reminding message  ");


                break;

            case "Стандартные модули":
                _ = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "<b>Стандартные модули</b>\r\nПоскольку в PascalABC.NET можно пользоваться всеми библиотеками платформы .NET, стандартные модули немногочисленны и ориентированы на обучение:\r\n\r\nМодуль растровой графики GraphWPF\r\nМодуль растровой графики GraphABC (для Linux)\r\nМодуль 3D графики и анимации Graph3D (на основе свободной библиотеки Helix Toolkit)\r\nМодуль векторной графики WPFObjects\r\nМодуль векторной графики ABCObjects (устаревший)\r\nМодуль FormsABC для создания графических интерфейсов (на базе технологии Windows Forms)\r\nМодуль NumLibABC (библиотека численных методов)\r\nМодуль PlotWPF для визуализации данных\r\nМодули исполнителей Робот и Чертёжник (школьная информатика)\r\nМодуль электронного задачника Programming Taskbook (базовая версия)",
            replyMarkup: replyKeyboardMarkup,
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Содержание \"Основы PascalABC.NET\" ");
                _ = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Ознакомиться более подробно с каждым модулем Вы всегда можете по ссылке:\n https://pascalabc.net/downloads/pabcnethelp/index.htm ",
                replyMarkup: replyKeyboardMarkup,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Reminding message  ");
                break;

            case "Коротко о главном":

                _ = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "<b>Коротко о главном</b> \r\n\r\nДанный текст содержит краткий обзор особенностей PascalABC.NET. \r\n\r\nPascalABC.NET – легковесная и мощная среда разработки программ с подробной справочной системой, средствами подсказки по коду, автоформатированием, встроенным отладчиком и встроенным дизайнером форм. Интегрированная среда разработки PascalABC.NET ориентирована на создание проектов малой и средней сложности, а также на обучение современному программированию. \r\nPascalABC.NET – мощный и современный язык программирования. По предоставляемым возможностям он превосходит язык Delphi и содержит практически все возможности языка C#. \r\nPascalABC.NET опирается на платформу Microsoft .NET - её языковые возможности и библиотеки, что делает его гибким, эффективным, постоянно развивающимся. Кроме того, можно легко сочетать библиотеки, разработанные на PascalABC.NET и других .NET-языках. \r\nКомпилятор PascalABC.NET генерирует код, выполняющийся так же быстро, как и код на C#, и несколько медленнее, чем код на C++ и Delphi. \r\nPascalABC.NET является представителем линейки современных языков Паскаль вместе с Delphi и Oxygene. \r\nМнение, что язык Паскаль устарел и утрачивает свои позиции, основано на представлении о старом Паскале и старых средах программирования (например, Free Pascal с его несовременной консольной оболочкой). К сожалению, масса отечественной учебной литературы с упорством, достойным лучшего применения, ориентируется на отживший Turbo Pascal с древней консольной оболочкой, бедной графической библиотекой и устаревшими средствами объектно-ориентированного программирования, развивая у обучающихся стойкое отвращение к языку Паскаль вообще. \r\nPascalABC.NET расширен современными языковыми возможностями для легкого, компактного и понятного програмирования. \r\nPascalABC.NET – достаточно зрелая среда. Ее прототип – учебная система Pascal ABC – появилась в 2002 году. PascalABC.NET – развивающаяся среда. Ведутся разработки новых языковых возможностей, новых библиотек. \r\nДалее приводится ряд программ с короткими комментариями, раскрывающих возможности и особенности языка PascalABC.NET.  \r\n\r\nТексты программ располагаются в рабочей папке (по умолчанию C:\\PABCWork.NET) в подпапке Samples\\!MainFeatures.\r\n\r\nДля запуска программ данное окно должно быть открыто через пункт меню \"Помощь/Коротко о главном\" так, чтобы оно не полностью закрывало окно оболочки PascalABC.NET.\r\n",
                replyMarkup: replyKeyboardMarkup1,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Содержание \"коротко о главном\" ");

                Message message2 = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Ознакомиться более подробно со справкой по языку PascalABC.NET Вы всегда можете по ссылке:\n https://pascalabc.net/downloads/pabcnethelp/index.htm ",
                replyMarkup: replyKeyboardMarkup1,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Reminding message  ");
                break;

            case "Отличия языка PascalABC.NET от Delphi":
                _ = await botClient.SendTextMessageAsync(
               chatId: chatId,
               text: "<b>Отличия языка PascalABC.NET от Delphi</b>\r\n<i>Добавлено</i>\r\nОперации += -= для событий .NET и для процедурных переменных. \r\nОперации += -= *= для целых и  += -= *= /= для вещественных. \r\nОперация += для строк. \r\nПодпрограммы с переменным числом параметров. \r\nОперация new для вызова конструктора (ident := new type_name(params);). \r\nОперация new для создания динамического массива. \r\nОперация typeof . \r\nИспользование uses для подключения пространств имен .NET (реализовано в Delphi Prism). \r\nВид доступа internal (наряду с public, private, protected). \r\nИнициализация переменных: var a: integer := 1; \r\nИнициализация переменных: var a := 1; \r\nОбъявление локальных переменных в блоке. \r\nОбъявление параметра цикла в заголовке цикла: for var i := 1 to 10 do, foreach var x in a do. \r\nОператор lock, обеспечивающий синхронизацию потоков. \r\nМетоды в записях. \r\nИнициализаторы полей в классах и записях. \r\nОбобщенные классы (generics). \r\nРеализованы типизированные файлы (в отличие от Delphi Prism, где они убраны). \r\nУпрощенный синтаксис модулей. \r\nОписание методов внутри интерфейса класса или записи. \r\nРеализация записью интерфейса. \r\nМетоды расширения. \r\nЛямбда-выражения. \r\n<i>Изменено</i>\r\nТолько сокращенное вычисление логических выражений. \r\nДругой синтаксис foreach. \r\nИнтерфейсы interface в стиле .NET. \r\nДругой синтаксис перегрузки операций. \r\nСтатические методы классов вместо классовых методов. Отсутствие типа TClass. \r\nДеструкторы оставлены лишь для совместимости и не выполняют никаких действий. \r\nТип object - синоним System.Object. \r\nТип exception - синоним System.Exception. \r\nИндексация string с 1, директива переключения на индексацию с 0. \r\nПроцедура Write выводит любые типы. \r\nСтруктурная эквивалентность типов для процедурных переменных, динамических массивов, типизированных указателей и множеств (в Delphi Object Pascal - именная эквивалентность типов за исключением открытых массивов). \r\nМножества на базе произвольных типов (set of string). \r\nЗапрет использования указателей на управляемую память. \r\nПроцедурные переменные (делегаты) вместо procedure of object. \r\nС бестиповыми файлами file можно работать с помощью процедур Read, Write. \r\nМассивы массивов отличаются по типу от двумерных массивов (в частности, записи a[i][j] и a[i,j] неэквивалентны). \r\nПерегрузка выполняется без ключевого слова overload. \r\nВсе конструкторы имеют имя Create. \r\nАвтоматическое управление памятью с помощью сборщика мусора (за исключением указателей на неуправляемую память). \r\n",
               replyMarkup: replyKeyboardMarkup1,
               parseMode: ParseMode.Html,
               cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Содержание \"Отличия языка PascalABC.NET от Delphi\" ");
                Message messager = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Ознакомиться более подробно со справкой по языку PascalABC.NET Вы всегда можете по ссылке:\n https://pascalabc.net/downloads/pabcnethelp/index.htm ",
                replyMarkup: replyKeyboardMarkup1,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Reminding message  ");


                break;


            case "Основы":
                _ = await botClient.SendTextMessageAsync(
              chatId: chatId,
              text: "<b>Внешний вид программы</b> \n Программа содержит ключевые слова, идентификаторы, комментарии. Ключевые слова используются для выделения синтаксических конструкций и подсвечиваются жирным шрифтом в редакторе. Идентификаторы являются именами объектов программы и не могут совпадать с ключевыми словами.\r\n\r\n Программа на языке PascalABC.NET имеет следующий вид:\r\n\r\nprogram имя программы;\r\nраздел uses\r\nраздел описаний\r\nbegin\r\n  операторы\r\nend.\r\n\r\nПервая строка называется заголовком программы и не является обязательной.\r\n\r\nРаздел uses состоит из нескольких подряд идущих секций uses, каждая из которых начинается с ключевого слова uses, за которым следует список имен модулей и пространств имен .NET, перечисляемых через запятую.\r\n\r\nРаздел описаний может включать следующие подразделы:\r\n\r\nраздел описания переменных\r\nраздел описания констант\r\nраздел описания типов\r\nраздел описания меток\r\nраздел описания процедур и функций\r\nДанные подразделы следуют друг за другом в произвольном порядке.\n <b>Выражения и операции: обзор</b>\r\nВыражение - это конструкция, возвращающая значение некоторого типа. Простыми выражениями являются переменные и константы. Более сложные выражения строятся из простых с помощью операций, вызовов функций и скобок. Данные, к которым применяются операции, называются операндами. \r\n<b>В PascalABC.NET имеются следующие операции: @, not, ^, *, /, div, mod, and, shl, shr, +, -, or, xor, =, &gt,   &lt; , &lt;&gt; ,  &lt;=, &gt=, as, is, in, а также операция new и операция приведения типа. Операции @, -, +, ^, not, операция приведения типа и операция new являются унарными (имеют один операнд), остальные являются бинарными (имеют два операнда), операции + и - являются и бинарными и унарными.</b>\r\n\r\n",
              replyMarkup: replyKeyboardMarkup2,
              parseMode: ParseMode.Html,
              cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Содержание \"Основы PascalABC.NET\" ");
                _ = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Ознакомиться более подробно со справкой по языку PascalABC.NET Вы всегда можете по ссылке:\n https://pascalabc.net/downloads/pabcnethelp/index.htm ",
                replyMarkup: replyKeyboardMarkup2,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Reminding message  ");

                break;

            case "Типы данных":
                _ = await botClient.SendTextMessageAsync(
              chatId: chatId,
              text: "<b>Обзор типов</b>\r\nТипы в PascalABC.NET подразделяются на простые, структурированные, типы указателей, процедурные типы и последовательности.\r\n\r\nК простым относятся целые и вещественные типы, логический, символьный, перечислимый и диапазонный тип.\r\n\r\nТип данных называется структурированным, если в одной переменной этого типа может содержаться множество значений.\r\n\r\nК структурированным типам относятся массивы, строки, записи, кортежи, множества, файлы и классы.\r\n\r\nОсобым типом данных является последовательность, которая хранит по-существу алгоритм получения данных последовательности один за другим.\r\n\r\nВсе простые типы, кроме вещественного, называются порядковыми. Только значения этих типов могут быть индексами статических массивов и параметрами цикла for. Кроме того, для порядковых типов используются функции Ord, Pred и Succ, а также процедуры Inc и Dec.\r\n\r\nВсе типы, кроме типов указателей, являются производными от типа Object. Каждый тип в PascalABC.NET имеет отображение на тип .NET. Тип указателя принадлежит к неуправляемому коду и моделируется типом void*.\r\n\r\nБольшинство типов в PascalABC.NET подразделяются на размерные, ссылочные и типы указателей. Сравнение размерных и ссылочных типов дано здесь.\r\n\r\nКроме того, в PascalABC.NET имеется несколько типов, унаследованных от Delphi Object Pascal, которые трудно отнести к размерному или ссылочному типу. Это статические массивы, множества, размерные строки и файлы. По представлению в памяти они относятся к ссылочному типу, но по поведению - к размерному.\r\n",
              replyMarkup: replyKeyboardMarkup2,
              parseMode: ParseMode.Html,
              cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Содержание \"Типы данных\" ");
                _ = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Ознакомиться с каждым типом языка PascalABC.NET в отдельности Вы всегда можете по ссылке в разделе <b>Типы Данных</b>:\n https://pascalabc.net/downloads/pabcnethelp/index.htm ",
                replyMarkup: replyKeyboardMarkup2,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Reminding message  ");

                break;

            case "Операторы":
                _ = await botClient.SendTextMessageAsync(
              chatId: chatId,
              text: "<b>Операторы: обзор</b>\r\nВ PascalABC.NET определены следующие операторы.\r\n\r\nОператоры присваивания\r\nСоставной оператор\r\nОператор описания переменной\r\nОператор цикла for\r\nОператор цикла foreach\r\nОператоры цикла while и repeat\r\nУсловный оператор if\r\nОператор выбора варианта case\r\nОператор вызова процедуры\r\nОператор try except\r\nОператор try finally\r\nОператор raise\r\nОператоры break, continue и exit\r\nОператор goto\r\nОператор lock \r\nОператор with\r\nПустой оператор",
              replyMarkup: replyKeyboardMarkup2,
              parseMode: ParseMode.Html,
              cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Содержание \"Операторы\" ");
                _ = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Ознакомиться с каждым оператором языка PascalABC.NET в отдельности Вы всегда можете по ссылке в разделе <b>Операторы</b>:\n https://pascalabc.net/downloads/pabcnethelp/index.htm ",
                replyMarkup: replyKeyboardMarkup2,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Reminding message  ");

                break;

            case "Структурное программирование":
                _ = await botClient.SendTextMessageAsync(
              chatId: chatId,
              text: "<b>Процедуры и функции: обзор</b>\r\nЧто такое процедуры и функции\r\nПроцедура или функция представляет собой последовательность операторов, которая имеет имя, список параметров и может быть вызвана из различных частей программы. Функции, в отличие от процедур, в результате своего выполнения возвращают значение, которое может быть использовано в выражении. Для единообразия функции и процедуры называются подпрограммами.\r\n\r\nОписание процедур и функций\r\nЛюбая используемая в программе процедура или функция должна быть предварительно описана в разделе описаний.\r\n\r\nОписание процедуры имеет вид:\r\n\r\nprocedure имя(список формальных параметров);\r\nраздел описаний \r\nbegin\r\n  операторы \r\nend;\r\n\r\nОписание функции имеет вид:\r\n\r\nfunction имя(список формальных параметров): тип возвращаемого значения;\r\nраздел описаний \r\nbegin\r\n  операторы \r\nend;\r\n\r\nОператоры подпрограммы, окаймленные операторными скобками begin/end, называются телом этой подпрограммы.\r\n\r\nСписок формальных параметров вместе с окружающими скобками может отсутствовать. Он состоит из одной или нескольких секций, разделенных символом \";\". Каждая секция состоит из списка переменных, перечисляемых через запятую, после которого следуют двоеточие и тип. Каждая секция может предваряться ключевым словом var или const, что указывает на то, что параметры передаются по ссылке (см.п. Параметры процедур и функций). Тип формального параметра должен быть либо именем, либо динамическим массивом, либо множеством, либо процедурной переменной (для последних трех типов имеет место структурная эквивалентность типов).\r\n\r\nРаздел описаний процедуры или функции устроен так же, как и раздел описаний основной программы. Здесь описываются так называемые локальные переменные и константы, типы (за исключением классов - классы можно описывать только глобально) а также вложенные процедуры и функции. Все такие локальные объекты доступны лишь внутри данной подпрограммы и не видны извне.\r\n\r\nВ разделе описаний подпрограммы можно описывать другие подпрограммы. Исключение составляют методы класса, описываемые непосредственно в теле класса: в них нельзя описывать вложенные подпрограммы ввиду синтаксической неоднозначности.\r\n\r\n",
              replyMarkup: replyKeyboardMarkup2,
              parseMode: ParseMode.Html,
              cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Содержание \"Структурное программирование\" ");

                _ = await botClient.SendTextMessageAsync(
             chatId: chatId,
             text: "<b>Структура модуля</b>\r\nМодули предназначены для разбиения текста программы на несколько файлов. В модулях описываются переменные, константы, типы, классы, процедуры и функции. Для того чтобы эти объекты можно было использовать в вызывающем модуле (которым может быть и основная программа), следует указать имя файла модуля (без расширения .pas) в разделе uses вызывающего модуля. Файл модуля (.pas) или откомпилированный файл модуля (.pcu) должен находиться либо в том же каталоге, что и основная программа, либо в подкаталоге Lib системного каталога программы PascalABC.NET.\r\n\r\nМодуль имеет следующую структуру:\r\n\r\nunit имя модуля;\r\n\r\ninterface\r\n\r\nраздел интерфейса \r\n\r\nimplementation\r\n\r\nраздел реализации \r\n\r\ninitialization\r\n\r\nраздел инициализации\r\n\r\nfinalization\r\n\r\nраздел финализации\r\n\r\nend. \r\n\r\nИмеется также упрощенный синтаксис модулей без разделов интерфейса и реализации.\r\n\r\nПервая строка обязательна и называется заголовком модуля. Имя модуля должно совпадать с именем файла.\r\n\r\nРаздел интерфейса и раздел реализации модуля могут начинаться с раздела uses подключения внешних модулей и пространств имен .NET. Имена в двух разделах uses не должны пересекаться.\r\n\r\nРаздел интерфейса включает объявление всех имен, которые экспортируются данным модулем в другие модули (при подключении его в разделе uses). Это могут быть константы, переменные, процедуры, функции, классы, интерфейсы. Реализация методов классов может быть дана прямо в разделе интерфейса, но это не рекомендуется.\r\n\r\nРаздел реализации содержит реализацию всех процедур, функций и методов, объявленных в разделе интерфейса. Кроме этого, в разделе реализации могут быть описания внутренних имен, которые не видны вне модуля и используются лишь как вспомогательные. \r\n\r\nРаздел инициализации и раздел финализации представляют собой последовательность операторов, разделяемых символом ;. Операторы из раздела инициализации модуля выполняются до начала основной программы, операторы из раздела финализации модуля - после окончания основной программы. Порядок выполнения разделов инициализации и разделов финализации подключенных модулей непредсказуем. Как раздел инициализации, так и раздел финализации могут отсутствовать. \r\n\r\nВместо разделов инициализации и финализации может присутствовать только раздел инициализации в виде\r\n\r\nbegin\r\n  последовательность операторов\r\nend.\r\n\r\n",
             replyMarkup: replyKeyboardMarkup2,
             parseMode: ParseMode.Html,
             cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Содержание \"Структурное программирование\" ");

                _ = await botClient.SendTextMessageAsync(
             chatId: chatId,
             text: "<b>Библиотеки dll</b> \r\nБиблиотеки dll (dynamically linked libraries): \r\n\r\nсодержат группу взаимосвязанных подпрограмм \r\nнаходятся в откомпилированном файле \r\nпредназначены для обращения к ним из различных программ \r\nОни находятся в файле с расширением .dll либо в текущем каталоге приложения (локальные), либо в системном каталоге (глобальные библиотеки). Глобальными библиотеками могут пользоваться одновременно несколько приложений. \r\n\r\nПо своему назначению библиотеки очень похожи на модули, однако, имеют ряд важных отличий.\r\n\r\nОтличия библиотек от модулей \r\nПри создании из модулей исполняемого файла .exe программа-линковщик помещает в него только те подпрограммы, переменные, типы и константы, которые используются (вызываются) в основной программе. При компиляции же библиотеки в нее добавляются все подпрограммы, потому что неизвестно, какие подпрограммы потребуются конкретному приложению. Библиотеки .dll при выполнении программы полностью загружаются в оперативную память. \r\nБиблиотеки .dll часто используются одновременно несколькими программами. \r\nБиблиотека .dll может быть написана и откомпилирована на одном языке, а обращаться к ней можно из программ, написанных на других языках. Например, программа на PascalABC.NET может вызывать функцию из библиотеки, созданной на языке C# и наобороот. Таким образом, библиотеки обеспечивают межъязыковое взаимодействие. \r\nСтруктура библиотеки\r\nБиблиотека имеет практически ту же структуру, что и модуль:\r\n\r\nlibrary имя библиотеки;\r\n\r\ninterface\r\n\r\nраздел интерфейса \r\n\r\nimplementation\r\n\r\nраздел реализации \r\n\r\nend. \r\n\r\nИмя библиотеки должно совпадать с именем pas-файла, в котором библиотека находится.\r\n\r\n",
             replyMarkup: replyKeyboardMarkup2,
             parseMode: ParseMode.Html,
             cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Содержание \"Структурное программирование\" ");
                _ = await botClient.SendTextMessageAsync(
           chatId: chatId,
           text: "<b>Документирующие комментарии</b>\r\nМожно помечать заголовки процедур, функций, методов, имена классов, типов, констант и переменных так называемыми документирующими комментариями. Документирующие комментарии всплывают в подсказках редактора при наведении курсора мыши на слово, при открытии скобки после имени подпрограммы и при выборе поля из списка полей, выпадающих при нажатии точки после имени. Система всплывающих подсказок в редакторе получила название Intellisense.\r\n\r\nДокументирующий комментарий располагается на строчке, предшествующей помечаемому объекту, и начинается с символов ///. Например:\r\n\r\nconst \r\n  /// Константа Pi\r\n  Pi = 3.14;\r\n\r\ntype\r\n  /// TTT - синоним целого типа\r\n  TTT = integer;\r\n  /// Документирующий комментарий класса XXX\r\n  XXX = class\r\n  end;\r\n\r\n/// Документирующий комментарий процедуры p\r\nprocedure p(a : integer);\r\nbegin\r\nend;\r\n\r\nvar \r\n  /// Документирующий комментарий переменной t1\r\n  t1: TTT;\r\n\r\nДокументирующие комментарии могут занимать несколько строк, каждая из которых должна начинаться с /// . Для комментирования подпрограмм можно использовать в первой строке документирующий комментарий ///-, тогда его содержимое меняет заголовок подпрограммы в подсказке при наведении курсора мыши. Например:\r\n\r\n///- Exclude(var s : set of T; el : T)\r\n///Удаляет элемент el из множества s\r\nprocedure Exclude(var s: TypedSet; el: object);\r\n\r\nЕсли первая строка документирующего комментария имеет вид ///--, то подсказка не всплывает. Это делается для элементов, которые хочется скрыть от системы всплывающих подсказок.\r\n",
           replyMarkup: replyKeyboardMarkup2,
           parseMode: ParseMode.Html,
           cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Содержание \"Структурное программирование\" ");
                _ = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Ознакомиться с  каждой структурой данных языка PascalABC.NET в отдельности Вы всегда можете по ссылке в разделе <b>Структурное программирование</b>:\n https://pascalabc.net/downloads/pabcnethelp/index.htm ",
                replyMarkup: replyKeyboardMarkup2,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Reminding message  ");

                break;

            case "Объектно-ориентированное программирование":

                _ = await botClient.SendTextMessageAsync(
             chatId: chatId,
             text: "<b>Обзор классов и объектов</b>\r\nОписание классов\r\nКласс представляет собой составной тип, состоящий из полей (переменных), методов (процедур и функций) и свойств. Описание класса имеет вид:\r\n\r\ntype \r\n  имя класса = class\r\n    секция1  \r\n    секция2  \r\n       ...\r\n  end;\r\n\r\nКаждая секция имеет вид:\r\n\r\nмодификатор доступа  \r\n   описания полей\r\n   объявления или описания методов и описания свойств\r\n\r\nМодификатор доступа в первой секции может отсутствовать, при этом подразумевается модификатор internal (видимость всюду внутри сборки).\r\n\r\nМетоды могут описываться как внутри, так и вне класса. При описании метода внутри класса его имя предваряется именем класса с последующей точкой. Например:\r\n\r\ntype\r\n  Person = class\r\n  private\r\n    fName: string;\r\n    fAge: integer;\r\n  public\r\n    constructor Create(Name: string; Age: integer);\r\n    begin\r\n      fName := Name;\r\n      fAge := Age;\r\n    end;\r\n    procedure Print;\r\n    property Name: string read fName;\r\n    property Age: integer read fAge;\r\n  end;\r\n\r\nprocedure Person.Print;\r\nbegin\r\n  Writeln($'Имя: {Name} Возраст: {Age}');\r\nend;\r\n\r\nПосле слова class в скобках может быть указано имя класса-предка (см. Наследование), а также через запятую список поддерживаемых интерфейсов. \r\n\r\nПеред словом class может быть указано ключевое слово sealed – в этом случае от класса запрещено наследовать.\r\n\r\nВсе описания и объявления внутри класса образуют тело класса. Поля и методы образуют интерфейс класса. Инициализаторы полей описаны здесь. \r\n\r\nКлассы могут описываться только на глобальном уровне. Локальные определения классов (т.е. определения в разделе описания подпрограмм) запрещены.\r\n\r\nПеременные типа класс\r\nВ языке PascalABC.NET классы являются ссылочными типами. Это значит, что переменная типа класс хранит в действительности ссылку на объект. \r\n\r\nПеременные типа класс называются объектами или экземплярами класса. Они инициализируются вызовом конструктора класса - специального метода, выделяющего память под объект класса и инициализирующего его поля:\r\n\r\nvar p: Person := new Person('Иванов',20);\r\n\r\nПосле инициализации через переменную типа класс можно обращаться к публичным членам класса (полям, методам, свойствам), используя точечную нотацию:\r\n\r\nPrint(p.Name,p.Age);\r\np.Print;\r\n\r\nВывод переменной типа класс\r\nПо умолчанию процедура Write для переменной типа класс выводит содержимое её публичных полей и свойств в круглых скобках через запятую:\r\n\r\nWrite(p); // Иванов 20\r\n\r\nЧтобы изменить это поведение, в классе следует переопределить виртуальный метод ToString класса Object - в этом случае именно он будет вызываться при выводе объекта. \r\n\r\nНапример:\r\n\r\ntype\r\n  Person = class\r\n    ...\r\n    function ToString: string; override;\r\n    begin\r\n      Result := $'Имя: {Name} Возраст: {Age}'; \r\n    end;\r\n  end;\r\n  ...\r\nvar p: Person := new Person('Иванов',20);\r\nWriteln(p); // Имя: Иванов  Возраст: 20\r\n\r\nПрисваивание и передача в качестве параметров подпрограмм\r\nПеременная типа класс является ссылкой и хранит ссылку на объект, создаваемый вызовом конструктора.\r\n\r\nКак ссылка переменная типа класс может хранить значение nil:\r\n\r\np := nil;\r\n...\r\nif p = nil then ...\r\n\r\nПри присваивании переменных типа класс копируется только ссылка. После присваивания обе переменные типа класс будут ссылаться на один объект и совместно модифицировать его:\r\n\r\nvar p1,p2: Person;\r\n...\r\np1 := new Person('Петров',20);\r\np2 := p1;\r\np1.IncAge;\r\np2.Print; // Имя: Петров  Возраст: 21\r\n\r\nСравнение на равенство\r\nПри сравнении переменных типа класс на равенство сравниваются ссылки, а не значения. \r\n\r\nvar p1 := new Person('Петров',20);\r\nvar p2 := new Person('Петров',20);\r\nwriteln(p1=p2); // False\r\np2 := p1; \r\nwriteln(p1=p2); // True\r\n\r\nЭто поведение можно изменить, перегрузив операцию = для класса.\r\n",
             replyMarkup: replyKeyboardMarkup2,
             parseMode: ParseMode.Html,
             cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Содержание \"Объектно-ориентированное программирование\" ");
                _ = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Ознакомиться с более подробно с элементами объектно-ориентированного программирования на PascalABC.NET в отдельности Вы всегда можете по ссылке в разделе <b>Объектно-ориентированное программирование</b>:\n https://pascalabc.net/downloads/pabcnethelp/index.htm ",
                replyMarkup: replyKeyboardMarkup2,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Reminding message  ");

                break;

            case "Функциональное программирование":


                _ = await botClient.SendTextMessageAsync(
             chatId: chatId,
             text: "<b>Лямбда-выражения</b>\r\nЛямбда-выражение - это выражение специального вида, которое на этапе компиляции заменяется на имя подпрограммы, соответствующей лямбда-выражению и генерируемой компилятором \"на лету\". \r\n\r\nЗдесь излагается полный синтаксис лямбда-выражений.\r\n\r\nЗдесь рассказывается о захвате лямбда-выражением переменных из внешнего контекста.\r\n\r\nЛямбда-выражения запрещается использовать при инициализации полей класса или записи, внутри вложенных подпрограмм, в подпрограмме при наличии вложенной подпрограммы, в разделе инициализации модуля.\r\n\r\nЛямбда-выражения запрещается использовать совместно с метками label и оператором goto в одной подпрограмме.\r\n\r\nСинтаксис лямбда-выражений достаточно сложен и в данном пункте иллюстрируется на примерах. \r\n\r\nПример 1.\r\n\r\nvar f: integer -> integer := x -> x*x;\r\nf(2);\r\nЗапись x -> x является лямбда-выражением, представляющем собой функцию с одним параметром x типа integer, возвращающую x*x типа integer. По данной записи компилятор генерирует следующий код:\r\n\r\nfunction #fun1(x: integer): integer;\r\nbegin\r\n  Result := x*x;\r\nend;\r\n...\r\nvar f: integer -> integer := #fun1;\r\nf(2);\r\nЗдесь #fun1 - это имя, генерируемое компилятором. Кроме того, код функции #fun1 также генерируется компилятором. \r\n\r\nПример 2. Фильтрация четных\r\n\r\nОбычно лямбда-выражение передаётся как параметр подпрограммы. Например, в следующем коде \r\n\r\nvar a := Seq(3,2,4,8,5,5);\r\na.Where(x -> x mod 2 = 0).Print;\r\nлямбда-выражение x -> x mod 2 = 0 задаёт условие отбора чётных чисел из массива a.\r\n\r\nПример 3. Сумма квадратов\r\n\r\nvar a := Seq(1,3,5);\r\nwriteln(a.Aggregate(0,(s,x)->s+x*x));\r\nИногда необходимо явно задавать тип параметров в лямбда-выражении. \r\n\r\nПример 4. Выбор перегруженной версии процедуры с параметром-лямбдой.\r\n\r\nprocedure p(f: integer -> integer);\r\nbegin\r\n  write(f(1));\r\nend;\r\n\r\nprocedure p(f: real -> real);\r\nbegin\r\n  write(f(2.5));\r\nend;\r\n\r\nbegin\r\n  p((x: real)->x*x);\r\nend.\r\nВ данном примере вызов p(x -> x) вызовет ошибку компиляции, потому что компилятор не может выбрать, какую версию процедуры p выбирать. Задание типа параметра лямбды помогает устранить эту неоднозначность.\r\n\r\nПример 5. Лямбда-процедура.\r\n\r\nprocedure p(a: integer -> ());\r\nbegin\r\n  a(1)\r\nend;\r\n\r\nbegin\r\n  p(procedure(x) -> write(x));\r\nend.",
             replyMarkup: replyKeyboardMarkup2,
             parseMode: ParseMode.Html,
             cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Содержание \"Функциональное программирование\" ");
                _ = await botClient.SendTextMessageAsync(
          chatId: chatId,
          text: "<b>Захват переменных в лямбда-выражении</b>\r\nЛямбда-выражение может использовать переменные из внешнего контекста. Такие переменные называются захваченными лямбда-выражением.\r\n\r\nПример 1. Захват переменной в запросе Select.\r\n\r\nbegin\r\n  var a := Seq(2,3,4);\r\n  var z := 1;\r\n  var q := a.Select(x->x+z);\r\n  q.Println;\r\n  z := 2;\r\n  q.Println;\r\nend.\r\nЗдесь лямбда-выражение x->x+z захватывает внешнюю переменную z. Важно заметить, что при изменении значения переменной z запрос a.Select(x->x+z), хранящийся в переменной q, выполняется с новым значением z.\r\n\r\nПример 2. Накопление суммы во внешней переменной.\r\n\r\nbegin\r\n  var sum := 0;\r\n  var AddToSum: integer -> () := procedure (x) -> begin sum += x; end;\r\n\r\n  AddToSum(1);\r\n  AddToSum(3);\r\n  AddToSum(5);\r\n\r\n  writeln(sum); \r\nend.\r\n \r\n",
          replyMarkup: replyKeyboardMarkup2,
          parseMode: ParseMode.Html,
          cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Содержание \"Функциональное программирование\" ");
                _ = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Ознакомиться с более подробно с элементами функционального программирования на PascalABC.NET в отдельности Вы всегда можете по ссылке в разделе <b>Функциональное программирование</b>:\n https://pascalabc.net/downloads/pabcnethelp/index.htm ",
                replyMarkup: replyKeyboardMarkup2,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
                WriteToLog($"User: Bot  Message: Reminding message  ");

                break;


            default:

                // Отправляем обратно то же сообщение, что и получили
                WriteToLog($"User: {chatId}  Message: {messageText}"); // logging
                _ = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Вы написали: \"" + messageText + "\" Но я не знаю такой команды! Попробуйте спросить меня о чем-то связанном с PascalABC.NET!  Используйте /help , чтобы узнать что я умею!",
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