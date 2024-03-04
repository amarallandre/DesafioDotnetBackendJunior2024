using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook.Contact;
using TesteBackendEnContact.Controllers.Models;

namespace TesteBackendEnContact.Repository.Interface
{
    public interface IContactRepository
    {
        Task<Contact> GetByIdAsync(int id);
        Task<IEnumerable<Contact>> GetAllAsync();
        Task<Contact> AddAsync(Contact contact);
        Task<Contact> UpdateAsync(Contact contact);
        Task DeleteAsync(int id);
        Task<bool> ImportFromCsvAsync(Stream csvStream, IEnumerable<SaveContactRequest> requests);
        Task<IEnumerable<Contact>> SearchAsync(string search, int? pageNumber);
    }
}