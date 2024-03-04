using TesteBackendEnContact.Core.Interface.ContactBook.Contact;

namespace TesteBackendEnContact.Core.Domain.ContactBook.Contact
{
    public class Contact
    {
        public int Id { get; set; }
        public int? ContactBookId { get; set; }
        public int? CompanyId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string CompanyName { get; set; }
        public string ContactBookName { get; set; }

        public Contact()
        {
        }

        public Contact(int id, int contactBookId, int companyId, string name, string phone, string email, string address, string contactBookName, string companyName)
        {
            Id = id;
            ContactBookId = contactBookId;
            CompanyId = companyId;
            Name = name;
            Phone = phone;
            Email = email;
            Address = address;
            CompanyName = companyName;
            ContactBookName = contactBookName;
            
        }
    }

    public class SearchResult
    {
        public string CompanyName { get; set; }
        public int ContactBookId { get; set; }
        public string ContactBookName { get; set; }
        public string ContactName { get; set; }
    }
}
