namespace BirthdayGreeter.Data.Repositories;

using Microsoft.Data.Sqlite;
using BirthdayGreeter.Domain.Entities;
using BirthdayGreeter.Data.Interfaces;

public class PersonRepository : IPersonRepository, IDisposable
{
    private readonly SqliteConnection _connection;

    public PersonRepository(string connectionString)
    {
        _connection = new SqliteConnection(connectionString);
        _connection.Open();
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Persons (
                Id INTEGER PRIMARY KEY ,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                BirthDate TEXT NOT NULL,
                Notes TEXT,
                CreatedAt TEXT NOT NULL
            )";
        command.ExecuteNonQuery();
    }

    public async Task<IEnumerable<Person>> GetAllAsync()
    {
        var command = _connection.CreateCommand();
        command.CommandText = "SELECT * FROM Persons ORDER BY BirthDate";
        
        var persons = new List<Person>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            persons.Add(MapToPerson(reader));
        }
        
        return persons;
    }

    public async Task<Person?> GetByIdAsync(int id)
    {
        var command = _connection.CreateCommand();
        command.CommandText = "SELECT * FROM Persons WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);
        
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapToPerson(reader);
        }
        
        return null;
    }

    public async Task AddAsync(Person person)
    {
        var command = _connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Persons (FirstName, LastName, BirthDate, Notes, CreatedAt)
            VALUES (@firstName, @lastName, @birthDate, @notes, @createdAt)";
        
        command.Parameters.AddWithValue("@firstName", person.FirstName);
        command.Parameters.AddWithValue("@lastName", person.LastName);
        command.Parameters.AddWithValue("@birthDate", person.BirthDate.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@notes", (object?)person.Notes ?? DBNull.Value);
        command.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(Person person)
    {
        var command = _connection.CreateCommand();
        command.CommandText = @"
            UPDATE Persons 
            SET FirstName = @firstName, LastName = @lastName, 
                BirthDate = @birthDate, Notes = @notes
            WHERE Id = @id";
        
        command.Parameters.AddWithValue("@id", person.Id);
        command.Parameters.AddWithValue("@firstName", person.FirstName);
        command.Parameters.AddWithValue("@lastName", person.LastName);
        command.Parameters.AddWithValue("@birthDate", person.BirthDate.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@notes", (object?)person.Notes ?? DBNull.Value);
        
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var command = _connection.CreateCommand();
        command.CommandText = "DELETE FROM Persons WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);
        
        await command.ExecuteNonQueryAsync();
    }

    private Person MapToPerson(SqliteDataReader reader)
    {
        return new Person
        {
            Id = reader.GetInt32(0),
            FirstName = reader.GetString(1),
            LastName = reader.GetString(2),
            BirthDate = DateTime.Parse(reader.GetString(3)),
            Notes = reader.IsDBNull(4) ? null : reader.GetString(4),
            CreatedAt = DateTime.Parse(reader.GetString(5))
        };
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}