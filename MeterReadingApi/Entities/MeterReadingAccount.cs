using MeterReadingApi.Repositories.Interfaces;

namespace MeterReadingApi.Entities
{
    public class MeterReadingAccount : IEntity
    {
        public int AccountId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset? MeterReadingDateTime { get; set; }
        public string? MeterReadValue { get; set; }
        public DateTimeOffset? UploadedAt { get; set; }

    }
}
