namespace BirthdayGreeter.Domain.Entities;

public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public int Age => CalculateAge();
    public int DaysUntilBirthday => CalculateDaysUntilBirthday();
    public bool IsBirthdayToday => DaysUntilBirthday == 0;

    private int CalculateAge()
    {
        var today = DateTime.Today;
        int age = today.Year - BirthDate.Year;
        if (BirthDate.Date > today.AddYears(-age)) age--;
        return age;
    }

    private int CalculateDaysUntilBirthday()
    {
        var today = DateTime.Today;
        var nextBirthday = new DateTime(today.Year, BirthDate.Month, BirthDate.Day);
        if (nextBirthday.Date < today.Date)
            nextBirthday = nextBirthday.AddYears(1);
        return (nextBirthday.Date - today.Date).Days;
    }

    public override string ToString()
    {
        return $"{FirstName} {LastName} - {BirthDate:dd.MM.yyyy} ({Age} лет, через {DaysUntilBirthday} дн.)";
    }
}