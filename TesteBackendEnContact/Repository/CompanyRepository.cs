using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook.Company;
using TesteBackendEnContact.Core.Interface.ContactBook.Company;
using TesteBackendEnContact.Core.Domain.ContactBook.Contact;
using TesteBackendEnContact.Database;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Repository
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly DatabaseConfig databaseConfig;

        public CompanyRepository(DatabaseConfig databaseConfig)
        {
            this.databaseConfig = databaseConfig;
        }

        public async Task<ICompany> SaveAsync(ICompany company)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            var dao = new CompanyDao(company);

            if (dao.Id == 0)
                dao.Id = await connection.InsertAsync(dao);
            else
                await connection.UpdateAsync(dao);

            return dao.Export();
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            var sql = new StringBuilder();
            sql.AppendLine("DELETE FROM Company WHERE Id = @id;");
            sql.AppendLine("DELETE FROM sqlite_sequence WHERE name  = 'Company';");
            sql.AppendLine("UPDATE Contact SET CompanyId = null WHERE CompanyId = @id;");


            await connection.ExecuteAsync(sql.ToString(), new { id }, transaction);
            transaction.Commit();
        }


            public async Task<IEnumerable<ICompany>> GetAllAsync()
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM Company";
            var result = await connection.QueryAsync<CompanyDao>(query);

            return result?.Select(item => item.Export());
        }

        public async Task<ICompany> UpdateAsync(int id, string newName, int? contactBookId)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                
                if (contactBookId.HasValue)
                {
                    var contactBookExistsQuery = "SELECT COUNT(*) FROM ContactBook WHERE Id = @ContactBookId";
                    var contactBookExists = await connection.ExecuteScalarAsync<int>(contactBookExistsQuery, new { ContactBookId = contactBookId.Value });

                    if (contactBookExists == 0)
                    {
                        throw new Exception($"O ContactBook com o ID {contactBookId} não existe.");
                    }
                }

                var parameters = new DynamicParameters();
                parameters.Add("@Id", id);

                var sql = new StringBuilder("UPDATE Company SET ");

                if (!string.IsNullOrEmpty(newName))
                {
                    sql.Append("Name = @NewName, ");
                    parameters.Add("@NewName", newName);
                }

                if (contactBookId.HasValue)
                {
                    sql.Append("ContactBookId = @ContactBookId, ");
                    parameters.Add("@ContactBookId", contactBookId.Value);
                }

                
                sql.Remove(sql.Length - 2, 2);

                sql.Append(" WHERE Id = @Id");

                await connection.ExecuteAsync(sql.ToString(), parameters, transaction);

                
                var updatedCompany = await connection.QuerySingleOrDefaultAsync<CompanyDao>("SELECT * FROM Company WHERE Id = @Id", new { Id = id });

                transaction.Commit();

                return updatedCompany?.Export();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception("Erro ao atualizar a empresa", ex);
            }
        }

        private async Task<bool> IsContactBookIdValid(int contactBookId)
        {
            
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Contact WHERE Id = @ContactBookId", new { ContactBookId = contactBookId });
            return count > 0;
        }

        public async Task<ICompany> GetAsync(int id)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var query = "SELECT * FROM Company where Id = @id";
            var result = await connection.QuerySingleOrDefaultAsync<CompanyDao>(query, new { id });

            return result?.Export();
        }

        public async Task<IEnumerable<SearchResult>> SearchAsync(string search)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            var query = @"
        SELECT 
            Company.Name AS CompanyName,
            ContactBook.Id AS ContactBookId,
            ContactBook.Name AS ContactBookName,
            Contact.Name AS ContactName
        FROM 
            Contact
        INNER JOIN 
            Company ON Contact.CompanyId = Company.Id 
        INNER JOIN 
            ContactBook ON Contact.ContactBookId = ContactBook.Id
        WHERE 
            Company.Name LIKE @SearchTerm
    ";
            return await connection.QueryAsync<SearchResult>(query, new { SearchTerm = $"%{search}%" });
        }
    }

    [Table("Company")]
    public class CompanyDao : ICompany
    {
        [Key]
        public int Id { get; set; }
        public int ContactBookId { get; set; }
        public string Name { get; set; }

        public CompanyDao()
        {
        }

        public CompanyDao(ICompany company)
        {
            Id = company.Id;
            ContactBookId = company.ContactBookId;
            Name = company.Name;

        }

        public ICompany Export() => new Company(Id, ContactBookId, Name);
    }
}
