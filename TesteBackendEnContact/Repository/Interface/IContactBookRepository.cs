﻿using System.Collections.Generic;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Interface.ContactBook;

namespace TesteBackendEnContact.Repository.Interface
{
    public interface IContactBookRepository
    {
        Task<IContactBook> SaveAsync(IContactBook contactBook);
        Task DeleteAsync(int id);
        Task<IEnumerable<IContactBook>> GetAllAsync();
        Task<IContactBook> GetAsync(int id);
        Task<IContactBook> UpdateAsync(int id, string newName);
    }
}
