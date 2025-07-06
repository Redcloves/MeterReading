using MeterReadingApi.Entities;
using MeterReadingApi.Repositories.Interfaces;
using MeterReadingApp.Repositories;

namespace MeterReadingApi.Repositories
{
    public class MeterReadingRepository: Repository<MeterReadingAccount>, IMeterReadingRepository
    {
        public MeterReadingRepository(StorageDbContext context) : base(context)
        {
        }
    }
}
