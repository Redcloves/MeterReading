using CsvHelper;
using MeterReadingApi.Entities;
using MeterReadingApi.Models;
using MeterReadingApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Globalization;
using System.Net;

namespace MeterReadingApi.Server.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private IMeterReadingRepository _meterReadingRepository;
        public AccountController(IMeterReadingRepository meterReadingRepository) 
        {
            _meterReadingRepository = meterReadingRepository;
        }

        [HttpPost]
        public async Task<IActionResult> UploadAccounts(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return NotFound("CSV file not found");
            }

            try
            {
                int accountsAddedCount = 0;
                DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow;

                using (var reader = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    var records = csv.GetRecords<AccountModel>().ToList();

                    foreach (var record in records)
                    {
                        if (await _meterReadingRepository.Get(record.AccountId) == null)
                        {
                            var entity = new MeterReadingAccount()
                            {
                                AccountId = record.AccountId,
                                FirstName = record.FirstName,
                                LastName = record.LastName,
                            };
                            await _meterReadingRepository.Add(entity);
                            accountsAddedCount++;
                        }
                    }
                }

                return Ok($"File uploaded and processed successfully! Accounts added: {accountsAddedCount}.");

            }
            catch (Exception ex)
            {
                Log.Error("Error processing CSV file: " + ex.Message);
                return BadRequest("Error processing CSV file: " + ex.Message);
            }
            
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll()
        {
            var sensorList = await _meterReadingRepository.GetAll();

            return Ok(sensorList);
        }
    }
}
