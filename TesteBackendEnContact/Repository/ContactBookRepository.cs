using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook;
using TesteBackendEnContact.Core.Interface.ContactBook;
using TesteBackendEnContact.Database;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Repository
{
    public class ContactBookRepository : IContactBookRepository
    {
        private readonly DatabaseConfig databaseConfig;

        public ContactBookRepository(DatabaseConfig databaseConfig)
        {
            this.databaseConfig = databaseConfig;
        }


        public async Task<IContactBook> SaveAsync(IContactBook contactBook)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            var dao = new ContactBookDao(contactBook);

            dao.Id = await connection.InsertAsync(dao);

            return dao.Export();
        }


        public async Task DeleteAsync(int id)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            var sql = new StringBuilder();
            sql.AppendLine("DELETE FROM ContactBook WHERE Id = @id;");
            sql.AppendLine("DELETE FROM sqlite_sequence WHERE name  = 'ContactBook';");
            sql.AppendLine("UPDATE Contact SET ContactBookId = null WHERE ContactBookId = @id;");


            await connection.ExecuteAsync(sql.ToString(), new { id }, transaction);
            transaction.Commit();
        }

        public async Task<IContactBook> UpdateAsync(int id, string newName)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                var sql = "UPDATE ContactBook SET Name = @NewName WHERE Id = @Id";
                await connection.ExecuteAsync(sql, new { Id = id, NewName = newName });

                
                var updatedContactBook = await connection.QuerySingleOrDefaultAsync<ContactBookDao>("SELECT * FROM ContactBook WHERE Id = @Id", new { Id = id });

                transaction.Commit();

                return updatedContactBook?.Export();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                
                throw new Exception("Erro ao atualizar a empresa", ex);
            }
        }

        public async Task<IEnumerable<IContactBook>> GetAllAsync()
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM ContactBook";
            var result = await connection.QueryAsync<ContactBookDao>(query);

            var returnList = new List<IContactBook>();

            foreach (var AgendaSalva in result.ToList())
            {
                IContactBook Agenda = new ContactBook(AgendaSalva.Id, AgendaSalva.Name.ToString());
                returnList.Add(Agenda);
            }

            return returnList.ToList();
        }
        public async Task<IContactBook> GetAsync(int id)
        {
            var list = await GetAllAsync();

            return list.ToList().Where(item => item.Id == id).FirstOrDefault();
        }
    }

    [Table("ContactBook")]
    public class ContactBookDao : IContactBook
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        public ContactBookDao()
        {
        }

        public ContactBookDao(IContactBook contactBook)
        {
            Id = contactBook.Id;
            
            Name = contactBook.Name;
        }

        public IContactBook Export() => new ContactBook(Id, Name);
    }
}
