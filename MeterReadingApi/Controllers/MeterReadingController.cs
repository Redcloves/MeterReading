using CsvHelper;
using MeterReadingApi.Entities;
using MeterReadingApi.Models;
using MeterReadingApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Globalization;
using System.Net;

namespace MeterReadingApi.Controllers
{
    [Route("api/meter-reading-uploads")]
    public class MeterReadingController : Controller
    {
        private IMeterReadingRepository _meterReadingRepository;
        private CsvHelper.Configuration.CsvConfiguration _csvConfig;

        public MeterReadingController(IMeterReadingRepository meterReadingRepository)
        {
            _meterReadingRepository = meterReadingRepository;
            _csvConfig = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null,
                MissingFieldFound = null,
                ShouldSkipRecord = args => args.Row.Parser.Record.All(string.IsNullOrWhiteSpace)
            };
        }

        [HttpPost]
        public async Task<IActionResult> UploadMeterReadings(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return NotFound("CSV file not found");
            }

            try
            {
                int accountsAddedCount = 0;
                int recordsCount = 0;
                DateTimeOffset uploadDatetime = DateTimeOffset.UtcNow;

                using (var reader = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(reader, _csvConfig))
                {
                    var records = csv.GetRecords<MeterReadingModel>().ToList();
                    recordsCount = records.Count;

                    foreach (var record in records)
                    {
                        var existingRead = await _meterReadingRepository.Get(record.AccountId);

                        var validationSummary = isValid(existingRead, record, uploadDatetime);

                        if (validationSummary.Errors.Count == 0)
                        {
                            await UpdateMeterReading(existingRead, record, uploadDatetime);
                            accountsAddedCount++;
                        }
                        else
                        {
                            Log.Error($"Record with accountId '{record.AccountId}' is invalid. Erros: {string.Join(" ,", validationSummary.Errors)}.");
                        }
                    }
                }
                return Ok($"File uploaded and processed successfully! Meter readings added: {accountsAddedCount}. Failed: {recordsCount-accountsAddedCount}");
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

        private ValidationSummary isValid(MeterReadingAccount existingRead, MeterReadingModel newRead, DateTimeOffset uploadDatetime)
        {
            var errors = new List<string>();

            if (existingRead == null)
            {
                errors.Add($"Account '{newRead.AccountId}' does not exist.");
                return new ValidationSummary()
                {
                    Errors = errors
                };
            }

            if (existingRead.UploadedAt == uploadDatetime)
            {
                errors.Add($"Entry with accountId '{newRead.AccountId}' already processed.");
            }

            bool readingValueIsValid = newRead.MeterReadValue != null && newRead.MeterReadValue.All(Char.IsDigit) && newRead.MeterReadValue.Length == 5;
            if (!readingValueIsValid)
            {
                errors.Add($"Reading value for accountId '{newRead.AccountId}' is invalid. Expected formad: NNNNN.");
            }

            if (existingRead.MeterReadingDateTime != null && newRead.MeterReadingDateTime < existingRead.MeterReadingDateTime)
            {
                errors.Add($"Uploaded reading from accountId '{newRead.AccountId}' is older than existing read.");
            }

            return new ValidationSummary()
            {
                Errors = errors
            };
        }

        private async Task UpdateMeterReading(MeterReadingAccount existingRead, MeterReadingModel newRead, DateTimeOffset uploadDatetime)
        {
            existingRead.MeterReadValue = newRead.MeterReadValue;
            existingRead.MeterReadingDateTime = newRead.MeterReadingDateTime;
            existingRead.UploadedAt = uploadDatetime;
            await _meterReadingRepository.Update(existingRead);
        }

    }
}
