using System.Net.Http.Json;
using System.Net;
using WorkdayCalendar.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WorkdayCalendar.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;

namespace WorkdayCalendarUnitTest
{
    public class HolidayControllerTests
    {
        private readonly WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private ILogger<HolidayControllerTests> _logger;

        public HolidayControllerTests()
        {

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            _logger = loggerFactory.CreateLogger<HolidayControllerTests>();

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // in-memory db
                        services.AddDbContext<WorkdayCalendarDBContext>(options =>
                            options.UseInMemoryDatabase("TestDatabase"));
                    });
                });
        }

        [SetUp]
        public void Setup()
        {
            _client = _factory.CreateClient(); 
        }

        [TestCase("da2ab810-457c-45ad-b004-9546e497aad6", "Test Holiday", "2024-05-24T15:07:00", true)]
        [TestCase("da2ab810-457c-45ad-b004-9576e497aad6", "Test Holiday2", "2024-05-25T15:07:00", false)]
        public async Task AddHoliday_ReturnsExpectedResult(Guid id, string name, string dateTime , bool isRecurring )
        {
            var holiday = new Holiday
            {
                Id = id,
                Name = name,
                Date = DateTime.Parse(dateTime),
                IsRecurring = isRecurring
            };

            var response = await _client.PostAsJsonAsync("/api/holiday/AddHoliday", holiday);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.AreEqual("Holiday added successfully!", responseBody.GetProperty("message").ToString());
        }

        [Test]
        public async Task GetHolidays_ReturnsExpectedResult()
        {
            // GET
            var response = await _client.GetAsync("/api/holiday/GetHolidays");

            // Assert response status code - OK
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var rawJson = await response.Content.ReadAsStringAsync();

            var responseBody = JsonConvert.DeserializeObject<dynamic>(rawJson);

            // Assert response body - not null
            Assert.IsNotNull(responseBody);
           
           Assert.IsTrue(responseBody.result.Count > 0, "Result should contain at least one holiday.");

            var holiday = responseBody?.result[0];
            Assert.AreEqual("Test Holiday", holiday.name.ToString());

            var expectedDate = DateTime.Parse("2024-05-24T15:07:00");
            DateTime actualDate = DateTime.Parse(holiday.date.ToString());
            Assert.AreEqual(expectedDate, actualDate);
        }


        [Test]
        public async Task GetHolidays_ReturnsUnexpectedResult()
        {
            // GET
            var response = await _client.GetAsync("/api/holiday/GetHolidays");

            // Assert response status code - OK
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var rawJson = await response.Content.ReadAsStringAsync();

            var responseBody = JsonConvert.DeserializeObject<dynamic>(rawJson);

            // Assert response body - not null
            Assert.IsNotNull(responseBody);

            if (responseBody?.Count >= 1)
            {
                Assert.IsTrue(responseBody?.Count > 0, "There is atleast one holiday.");
            }


            Assert.IsTrue(responseBody.result.Count > 0, "Result should contain at least one holiday.");

            var holiday = responseBody?.result[0];
            Assert.AreEqual("Test Holiday2", holiday.name.ToString());

            var expectedDate = DateTime.Parse("2025-02-09T15:33:52");
            DateTime actualDate = DateTime.Parse(holiday.date.ToString());
            Assert.AreEqual(expectedDate, actualDate);
        }

        [Test]
        public async Task GetRecurringHolidays_ReturnsResult()
        {
            // GET
            var response = await _client.GetAsync("/api/holiday/GetRecurringHolidays");

            // Assert response status code - OK
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var rawJson = await response.Content.ReadAsStringAsync();

            var responseBody = JsonConvert.DeserializeObject<dynamic>(rawJson);

            // Assert response body - not null
            Assert.IsNotNull(responseBody);

            var isRecurring = responseBody?.result[0]?.isRecurring;

            Assert.IsTrue((bool)isRecurring, "Is a Recurring Holiday");

        }

        [Test]
        public async Task GetFixedHolidays_ReturnsResult()
        {
            // GET
            var response = await _client.GetAsync("/api/holiday/GetFixedHolidays");

            // Assert response status code - OK
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var rawJson = await response.Content.ReadAsStringAsync();

            var responseBody = JsonConvert.DeserializeObject<dynamic>(rawJson);

            // Assert response body - not null
            Assert.IsNotNull(responseBody);

            var isRecurring = responseBody?.result[0]?.isRecurring;

            Assert.IsFalse((bool)isRecurring, "Is a Fixed Holiday");

        }

        [TestCase("da2ab810-457c-45ad-b004-9546e497aad6")]
        public async Task GetHolidaysById_ReturnsExpectedResult(Guid id)
        {
            var requestUrl = $"/api/holiday/GetHolidaysById?id={id}";

            // GET
            var response = await _client.GetAsync(requestUrl);
           
            // Assert response status code - OK
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var rawJson = await response.Content.ReadAsStringAsync();

            var responseBody = JsonConvert.DeserializeObject<dynamic>(rawJson);

            // Assert response body - not null
            Assert.IsNotNull(responseBody);

            var holiday = responseBody?.result;
            Assert.AreEqual("Test Holiday", holiday?.name?.ToString());

        }

        [TestCase("da2ab810-457c-45ad-b004-9546e497aad6")]
        public async Task DeleteHoliday_ReturnsExpectedResult(Guid id)
        {
            var requestUrl = $"/api/holiday/DeleteHolidaysById?id={id}";

            // DELETE
            var response = await _client.DeleteAsync(requestUrl);

            // Assert response status code - OK
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);


        }


        [TestCase("da2ab810-457c-45ad-b004-9546e497aad6", "Test1 Holiday", "2024-05-24T15:07:00", true)]
        public async Task UpdateHoliday_ReturnsExpectedResult(Guid id, string name, string dateTime, bool isRecurring)
        {
            var holiday = new Holiday
            {
                Id = id,
                Name = name,
                Date = DateTime.Parse(dateTime),
                IsRecurring = isRecurring
            };

            var response = await _client.PatchAsJsonAsync("/api/holiday/UpdateHoliday", holiday);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.AreEqual("Holiday updated successfully!", responseBody.GetProperty("message").ToString());
        }

        [TearDown] 
        public void TearDown()
        {
            _client?.Dispose(); // Dispose  HttpClient
        }

        [OneTimeTearDown] 
        public void OneTimeTearDown()
        {
            _factory?.Dispose(); // Dispose WebAppFactory
        }
    }
}
