namespace BirthdayGreeter.Services;

using BirthdayGreeter.Domain.Entities;
using BirthdayGreeter.Data.Interfaces;
using BirthdayGreeter.Service.Interfaces;

public class BirthdayService : IBirthdayService
{
    private readonly IPersonRepository _repository;

    public BirthdayService(IPersonRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Person>> GetAllBirthdaysAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<IEnumerable<Person>> GetUpcomingBirthdaysAsync(int daysAhead = 7)
    {
        var allPersons = await _repository.GetAllAsync();
        return allPersons
            .Where(p => p.DaysUntilBirthday <= daysAhead)
            .OrderBy(p => p.DaysUntilBirthday);
    }

    public async Task<IEnumerable<Person>> GetTodayBirthdaysAsync()
    {
        var allPersons = await _repository.GetAllAsync();
        return allPersons
            .Where(p => p.IsBirthdayToday)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName);
    }

    public async Task<Person?> GetPersonByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task AddPersonAsync(Person person)
    {
        ValidatePerson(person);
        await _repository.AddAsync(person);
    }

    public async Task UpdatePersonAsync(Person person)
    {
        ValidatePerson(person);
        await _repository.UpdateAsync(person);
    }

    public async Task DeletePersonAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    private void ValidatePerson(Person person)
    {
        if (string.IsNullOrWhiteSpace(person.FirstName))
            throw new ArgumentException("Имя не может быть пустым");
        
        if (string.IsNullOrWhiteSpace(person.LastName))
            throw new ArgumentException("Фамилия не может быть пустой");
        
        if (person.BirthDate > DateTime.Today)
            throw new ArgumentException("Дата рождения не может быть в будущем");
        
        if (person.BirthDate < DateTime.Today.AddYears(-150))
            throw new ArgumentException("Некорректная дата рождения");
    }
}