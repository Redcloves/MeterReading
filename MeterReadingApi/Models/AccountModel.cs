using MeterReadingApi.Repositories.Interfaces;

namespace MeterReadingApi.Models
{
    public class AccountModel : IEntity
    {
        public int AccountId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
