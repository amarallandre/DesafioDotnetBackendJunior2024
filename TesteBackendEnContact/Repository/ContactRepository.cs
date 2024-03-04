using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Data.SqlClient;
using TesteBackendEnContact.Core.Domain.ContactBook.Contact;
using TesteBackendEnContact.Core.Interface.ContactBook.Contact;
using TesteBackendEnContact.Database;
using TesteBackendEnContact.Repository.Interface;
using TesteBackendEnContact.Controllers.Models;

namespace TesteBackendEnContact.Repository
{
    public class ContactRepository : IContactRepository
    {
        private readonly DatabaseConfig databaseConfig;

        public ContactRepository(DatabaseConfig databaseConfig)
        {
            this.databaseConfig = databaseConfig;
        }

        public async Task<Contact> GetByIdAsync(int id)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            connection.Open();
            return await connection.QueryFirstOrDefaultAsync<Contact>(
                "SELECT * FROM Contact WHERE Id = @Id", new { Id = id });
        }


        public async Task<IEnumerable<Contact>> SearchAsync(string search, int? pageNumber)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            connection.Open();

            var query = @"
                    SELECT 
                        c.*, 
                        coalesce(co.Name, 'Sem Empresa') AS CompanyName,
                        cb.Name AS ContactBookName
                    FROM 
                        Contact c
                    LEFT JOIN 
                        ContactBook cb ON c.ContactBookId = cb.Id
                    LEFT JOIN 
                        Company co ON c.CompanyId = co.Id
                    WHERE 
                        c.Name LIKE @SearchTerm 
                        OR c.Phone LIKE @SearchTerm 
                        OR c.Email LIKE @SearchTerm 
                        OR c.Address LIKE @SearchTerm 
                        OR cb.Name LIKE @SearchTerm 
                        OR co.Name LIKE @SearchTerm";

            var contacts = await connection.QueryAsync<Contact>(query, new
            {
                SearchTerm = $"%{search}%"
            });

            return contacts;
        }



        public async Task<IEnumerable<Contact>> GetAllAsync()
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            connection.Open();
            return await connection.QueryAsync<Contact>("SELECT * FROM Contact");
        }

        public async Task<Contact> AddAsync(Contact contact)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            connection.Open();
            var id = await connection.ExecuteScalarAsync<int>(
                "INSERT INTO Contact (ContactBookId, Name, Phone, Email, Address) VALUES (@ContactBookId, @Name, @Phone, @Email, @Address); SELECT last_insert_rowid()",
                new
                {
                    ContactBookId = contact.ContactBookId,
                    Name = contact.Name,
                    Phone = contact.Phone,
                    Email = contact.Email,
                    Address = contact.Address
                });
            contact.Id = id;
            return contact;
        }

        public async Task<Contact> UpdateAsync(Contact contact)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            connection.Open();
            await connection.ExecuteAsync(
                "UPDATE Contact SET ContactBookId = @ContactBookId, Name = @Name, Phone = @Phone, Email = @Email, Address = @Address WHERE Id = @Id",
                new
                {
                    ContactBookId = contact.ContactBookId,
                    Name = contact.Name,
                    Phone = contact.Phone,
                    Email = contact.Email,
                    Address = contact.Address,
                    Id = contact.Id
                });
            return contact;
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            connection.Open();
            await connection.ExecuteAsync("DELETE FROM Contact WHERE Id = @Id", new { Id = id });
        }

        public async Task<bool> ImportFromCsvAsync(Stream csvStream, IEnumerable<SaveContactRequest> requests)
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var csvRecords = csv.GetRecords<SaveContactRequest>();

            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            connection.Open();

            foreach (var record in csvRecords)
            {
                
                if (!record.ContactBookId.HasValue || record.ContactBookId == 0)
                {
                    continue;
                }

                
                var contactBookExists = await connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM ContactBook WHERE Id = @ContactBookId", new { ContactBookId = record.ContactBookId });

                if (contactBookExists == 0)
                {
                    
                    continue;
                }

                
                if (record.CompanyId.HasValue)
                {
                    var companyExists = await connection.ExecuteScalarAsync<int>(
                        "SELECT COUNT(*) FROM Company WHERE Id = @CompanyId", new { CompanyId = record.CompanyId });

                    if (companyExists == 0)
                    {
                        
                        continue;
                    }
                }

                var existingContact = await connection.QueryFirstOrDefaultAsync<ContactDao>(
                    "SELECT * FROM Contact WHERE ContactBookId = @ContactBookId AND CompanyId = @CompanyId AND Name = @Name AND Phone = @Phone AND Email = @Email AND Address = @Address",
                    new
                    {
                        ContactBookId = record.ContactBookId,
                        CompanyId = record.CompanyId,
                        Name = record.Name,
                        Phone = record.Phone,
                        Email = record.Email,
                        Address = record.Address
                    });

                
                if (existingContact == null)
                {
                    var contact = new ContactDao
                    {
                        ContactBookId = record.ContactBookId.Value,
                        CompanyId = record.CompanyId,
                        Name = record.Name,
                        Phone = record.Phone,
                        Email = record.Email,
                        Address = record.Address
                    };

                    await connection.InsertAsync(contact);
                }
            }

            return true;
        }
    }

    [Table("Contact")]
    public class ContactDao : IContact
    {
        [Key]
        public int Id { get; set; }
        public int? ContactBookId { get; set; }
        public int? CompanyId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

    }
}