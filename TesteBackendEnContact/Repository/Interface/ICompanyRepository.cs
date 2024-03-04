﻿using System.Collections.Generic;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Interface.ContactBook.Company;
using TesteBackendEnContact.Core.Domain.ContactBook.Contact;

namespace TesteBackendEnContact.Repository.Interface
{
    public interface ICompanyRepository
    {
        Task<ICompany> SaveAsync(ICompany company);
        Task DeleteAsync(int id);
        Task<IEnumerable<ICompany>> GetAllAsync();
        Task<ICompany> GetAsync(int id);
        Task<ICompany> UpdateAsync(int id, string newName, int? contactBookId);
        Task<IEnumerable<SearchResult>> SearchAsync(string search);
    }
}
