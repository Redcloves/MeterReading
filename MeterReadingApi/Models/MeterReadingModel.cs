using MeterReadingApi.Repositories.Interfaces;

namespace MeterReadingApi.Models
{
    public class MeterReadingModel: IEntity
    {
        public int AccountId { get; set; }
        public DateTimeOffset MeterReadingDateTime { get; set; }
        public string? MeterReadValue { get; set; }

    }
}
