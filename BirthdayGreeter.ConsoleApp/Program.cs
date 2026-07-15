namespace BirthdayGreeter.ConsoleApp;

using Microsoft.Extensions.DependencyInjection;
using BirthdayGreeter.Data.Repositories;
using BirthdayGreeter.Data.Interfaces;
using BirthdayGreeter.Services;
using BirthdayGreeter.Service.Interfaces;
using BirthdayGreeter.Domain.Entities;

class Program
{
    private static IBirthdayService _birthdayService;

    static async Task Main(string[] args)
    {
        // Настройка DI
        var services = new ServiceCollection();
        services.AddSingleton<IPersonRepository>(sp => 
            new PersonRepository("Data Source=birthdays.db"));
        services.AddSingleton<IBirthdayService, BirthdayService>();
        
        var serviceProvider = services.BuildServiceProvider();
        _birthdayService = serviceProvider.GetRequiredService<IBirthdayService>();

        Console.WriteLine("=== Поздравлятор ===");
        Console.WriteLine("Добро пожаловать!");
        
        // При запуске показываем сегодняшние и ближайшие ДР
        await ShowUpcomingBirthdaysAsync();
        
        bool exit = false;
        while (!exit)
        {
            ShowMenu();
            var choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    await ShowAllBirthdaysAsync();
                    break;
                case "2":
                    await ShowUpcomingBirthdaysAsync();
                    break;
                case "3":
                    await AddPersonAsync();
                    break;
                case "4":
                    await EditPersonAsync();
                    break;
                case "5":
                    await DeletePersonAsync();
                    break;
                case "0":
                    exit = true;
                    Console.WriteLine("До свидания!");
                    break;
                default:
                    Console.WriteLine("Неверный выбор. Попробуйте снова.");
                    break;
            }
        }
    }

    private static void ShowMenu()
    {
        Console.WriteLine("\n--- Меню ---");
        Console.WriteLine("1. Показать все дни рождения");
        Console.WriteLine("2. Показать сегодняшние и ближайшие ДР");
        Console.WriteLine("3. Добавить запись");
        Console.WriteLine("4. Редактировать запись");
        Console.WriteLine("5. Удалить запись");
        Console.WriteLine("0. Выход");
        Console.Write("Выберите действие: ");
    }

    private static async Task ShowAllBirthdaysAsync()
    {
        Console.WriteLine("\n=== Все дни рождения ===");
        var persons = await _birthdayService.GetAllBirthdaysAsync();
        
        if (!persons.Any())
        {
            Console.WriteLine("Список пуст.");
            return;
        }

        Console.WriteLine("ID |\t\tИмя/Фамилия\t\t| Дата рождения | Возраст | Дней до ДР");
        Console.WriteLine(new string('-', 80));
        
        foreach (var person in persons)
        {
            var status = person.IsBirthdayToday ? "!!!СЕГОДНЯ!!!" : 
                        person.DaysUntilBirthday <= 7 ? "Скоро!" : "";
            
            Console.WriteLine($"{person.Id,2} | {person.FirstName,16} {person.LastName,-17} |  " +
                            $"{person.BirthDate:dd.MM.yyyy}   | {person.Age,3} лет | " +
                            $"{person.DaysUntilBirthday,3} дн.{status}");
        }
    }

    private static async Task ShowUpcomingBirthdaysAsync()
    {
        Console.WriteLine("\n=== Сегодняшние и ближайшие дни рождения ===");
        
        var todayBirthdays = await _birthdayService.GetTodayBirthdaysAsync();
        var upcomingBirthdays = await _birthdayService.GetUpcomingBirthdaysAsync(7);
        
        if (todayBirthdays.Any())
        {
            Console.WriteLine("\nСегодня празднуют:");
            foreach (var person in todayBirthdays)
            {
                Console.WriteLine($"  • {person.FirstName} {person.LastName} - {person.Age} лет!");
            }
        }
        else
        {
            Console.WriteLine("\nСегодня нет дней рождения.");
        }

        var upcoming = upcomingBirthdays.Where(p => !p.IsBirthdayToday).ToList();
        if (upcoming.Any())
        {
            Console.WriteLine("\nБлижайшие 7 дней:");
            foreach (var person in upcoming)
            {
                Console.WriteLine($"  • {person.FirstName} {person.LastName} - через {person.DaysUntilBirthday} дн. ({person.BirthDate:dd.MM})");
            }
        }
    }

    private static async Task AddPersonAsync()
    {
        Console.WriteLine("\n=== Добавление записи ===");
        
        Console.Write("Имя: ");
        var firstName = Console.ReadLine();
        
        Console.Write("Фамилия: ");
        var lastName = Console.ReadLine();
        
        Console.Write("Дата рождения (дд.мм.гггг): ");
        var birthDateStr = Console.ReadLine();
        
        if (!DateTime.TryParseExact(birthDateStr, "dd.MM.yyyy", null, 
            System.Globalization.DateTimeStyles.None, out var birthDate))
        {
            Console.WriteLine("Неверный формат даты!");
            return;
        }

        Console.Write("Заметки (необязательно): ");
        var notes = Console.ReadLine();

        var person = new Person
        {
            FirstName = firstName,
            LastName = lastName,
            BirthDate = birthDate,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes
        };

        try
        {
            await _birthdayService.AddPersonAsync(person);
            Console.WriteLine("Запись успешно добавлена!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private static async Task EditPersonAsync()
    {
        Console.WriteLine("\n=== Редактирование записи ===");
        Console.Write("Введите ID записи: ");
        
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Неверный ID!");
            return;
        }

        var person = await _birthdayService.GetPersonByIdAsync(id);
        if (person == null)
        {
            Console.WriteLine("Запись не найдена!");
            return;
        }

        Console.WriteLine($"Текущие данные: {person.FirstName} {person.LastName}, {person.BirthDate:dd.MM.yyyy}");
        
        Console.Write($"Новое имя [{person.FirstName}]: ");
        var firstName = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(firstName)) person.FirstName = firstName;

        Console.Write($"Новая фамилия [{person.LastName}]: ");
        var lastName = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(lastName)) person.LastName = lastName;

        Console.Write($"Новая дата рождения [{person.BirthDate:dd.MM.yyyy}]: ");
        var birthDateStr = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(birthDateStr))
        {
            if (DateTime.TryParseExact(birthDateStr, "dd.MM.yyyy", null,
                System.Globalization.DateTimeStyles.None, out var birthDate))
            {
                person.BirthDate = birthDate;
            }
            else
            {
                Console.WriteLine("Неверный формат даты, оставлено старое значение.");
            }
        }

        Console.Write($"Новые заметки [{person.Notes}]: ");
        var notes = Console.ReadLine();
        person.Notes = string.IsNullOrWhiteSpace(notes) ? person.Notes : notes;

        try
        {
            await _birthdayService.UpdatePersonAsync(person);
            Console.WriteLine("Запись успешно обновлена!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private static async Task DeletePersonAsync()
    {
        Console.WriteLine("\n=== Удаление записи ===");
        Console.Write("Введите ID записи: ");
        
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Неверный ID!");
            return;
        }

        var person = await _birthdayService.GetPersonByIdAsync(id);
        if (person == null)
        {
            Console.WriteLine("Запись не найдена!");
            return;
        }

        Console.WriteLine($"Вы уверены, что хотите удалить {person.FirstName} {person.LastName}? (да/нет)");
        var confirmation = Console.ReadLine();
        
        if (confirmation?.ToLower() == "да")
        {
            await _birthdayService.DeletePersonAsync(id);
            Console.WriteLine("Запись удалена!");
        }
        else
        {
            Console.WriteLine("Удаление отменено.");
        }
    }
}