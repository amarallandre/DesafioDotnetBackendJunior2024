using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using TesteBackendEnContact.Core.Domain.ContactBook.Contact;
using TesteBackendEnContact.Repository.Interface;
using Microsoft.AspNetCore.Http;
using TesteBackendEnContact.Controllers.Models;

namespace TesteBackendEnContact.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IContactRepository _contactRepository;

        public ContactController(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var contact = await _contactRepository.GetByIdAsync(id);
            if (contact == null)
            {
                return NotFound();
            }
            return Ok(contact);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchAsync(string search, int? pageNumber)
        {

            int pageSize = 10;


            int currentPage = pageNumber ?? 1; 


            var contacts = await _contactRepository.SearchAsync(search, pageNumber);


            int totalPages = (int)Math.Ceiling((double)contacts.Count() / pageSize);

            if (currentPage < 1 || currentPage > totalPages)
            {
                return BadRequest("Página solicitada inválida");
            }

            var paginatedContacts = contacts.Skip((currentPage - 1) * pageSize).Take(pageSize);

            var htmlBuilder = new StringBuilder();
            htmlBuilder.Append("<!DOCTYPE html>");
            htmlBuilder.Append("<html lang='en'>");
            htmlBuilder.Append("<head>");
            htmlBuilder.Append("<meta charset='UTF-8'>");
            htmlBuilder.Append("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            htmlBuilder.Append("<title>Contact Search Results</title>");
            htmlBuilder.Append("</head>");
            htmlBuilder.Append("<body>");
            htmlBuilder.Append("<h1>Contact Search Results</h1>");


            htmlBuilder.Append("<ul>");
            for (int page = 1; page <= totalPages; page++)
            {
                htmlBuilder.Append($"<li><a href='/contact/search?search={search}&pageNumber={page}'>Page {page}</a></li>");
            }
            htmlBuilder.Append("</ul>");

            htmlBuilder.Append($"<h2>Page {currentPage}</h2>");
            htmlBuilder.Append("<ul>");
            foreach (var contact in paginatedContacts)
            {
                htmlBuilder.Append($"<li>{contact.Name} - {contact.Email} - {contact.CompanyName} - {contact.ContactBookName}</li>");
            }
            htmlBuilder.Append("</ul>");

            htmlBuilder.Append("</body>");
            htmlBuilder.Append("</html>");

            return new ContentResult
            {
                Content = htmlBuilder.ToString(),
                ContentType = "text/html",
                StatusCode = 200
            };
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Contact contact)
        {
            if (id != contact.Id)
            {
                return BadRequest();
            }

            var existingContact = await _contactRepository.GetByIdAsync(id);
            if (existingContact == null)
            {
                return NotFound();
            }

            await _contactRepository.UpdateAsync(contact);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existingContact = await _contactRepository.GetByIdAsync(id);
            if (existingContact == null)
            {
                return NotFound();
            }

            await _contactRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var contacts = await _contactRepository.GetAllAsync();
            return Ok(contacts);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportFromCsv(IFormFile file, [FromServices] IContactRepository contactRepository)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Arquivo não fornecido");

            using (var stream = file.OpenReadStream())
            {

                await contactRepository.ImportFromCsvAsync(stream, null);
            }

            return Ok("Importação concluída com sucesso");
        }
    }
}