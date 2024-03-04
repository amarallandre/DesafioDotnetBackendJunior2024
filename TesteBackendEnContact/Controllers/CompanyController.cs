using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using TesteBackendEnContact.Controllers.Models;
using TesteBackendEnContact.Core.Interface.ContactBook.Company;
using TesteBackendEnContact.Core.Domain.ContactBook.Contact;
using TesteBackendEnContact.Repository.Interface;

namespace TesteBackendEnContact.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly ILogger<CompanyController> _logger;
        private readonly ICompanyRepository _companyRepository;

        public CompanyController(ICompanyRepository companyRepository, ILogger<CompanyController> logger)
        {
            _companyRepository = companyRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ICompany>> Post(SaveCompanyRequest company, [FromServices] ICompanyRepository companyRepository)
        {
            return Ok(await companyRepository.SaveAsync(company.ToCompany()));
        }

        [HttpPut("{id}")]
        public async Task<ICompany> Put(int id, string newName, int newContactBookId, [FromServices] ICompanyRepository companyRepository)
        {
            return await companyRepository.UpdateAsync(id, newName, newContactBookId);
        }

        [HttpDelete]
        public async Task Delete(int id, [FromServices] ICompanyRepository companyRepository)
        {
            await companyRepository.DeleteAsync(id);
        }

        [HttpGet]
        public async Task<IEnumerable<ICompany>> Get([FromServices] ICompanyRepository companyRepository)
        {
            return await companyRepository.GetAllAsync();
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchAsync(string search)
        {
            try
            {
                var searchResults = await _companyRepository.SearchAsync(search);

                var groupedResults = searchResults.GroupBy(r => new { CompanyName = r.CompanyName })
                                                  .Select(g => new
                                                  {
                                                      CompanyName = g.Key.CompanyName,
                                                      ContactBooks = g.Select(r => new
                                                      {
                                                          ContactBookId = r.ContactBookId,
                                                          ContactBookName = r.ContactBookName,
                                                          ContactNames = string.Join(", ", g.Select(r => r.ContactName))
                                                      }).ToList()
                                                  });

                return Ok(groupedResults);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao pesquisar contatos: {ex.Message}");
                return StatusCode(500, "Erro interno ao pesquisar contatos");
            }
        }
        [HttpGet("{id}")]
        public async Task<ICompany> Get(int id, [FromServices] ICompanyRepository companyRepository)
        {
            return await companyRepository.GetAsync(id);
        }
    }
}