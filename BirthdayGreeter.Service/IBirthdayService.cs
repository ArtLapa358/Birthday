namespace BirthdayGreeter.Service.Interfaces;

using BirthdayGreeter.Domain.Entities;

public interface IBirthdayService
{
    Task<IEnumerable<Person>> GetAllBirthdaysAsync();
    Task<IEnumerable<Person>> GetUpcomingBirthdaysAsync(int daysAhead = 7);
    Task<IEnumerable<Person>> GetTodayBirthdaysAsync();
    Task<Person?> GetPersonByIdAsync(int id);
    Task AddPersonAsync(Person person);
    Task UpdatePersonAsync(Person person);
    Task DeletePersonAsync(int id);
}