using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Newtonsoft.Json;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using WorkdayCalendar.IService;
using WorkdayCalendar.Models;

namespace WorkdayCalendarUnitTest
{
    internal class GetAllHolidayUnitTest
    {

        private HttpClient _client;
        private WebApplicationFactory<Program> _factory;
        private Mock<IHolidayService> _mockHolidayService;

        [SetUp]
        public void SetUp()
        {

            var mockRecurringHolidays = new List<Holiday>
            {
                new Holiday { Id = Guid.NewGuid(), Name = "Christmas", IsRecurring = true, Date = new DateTime(2025, 12, 25) },
                new Holiday { Id = Guid.NewGuid(), Name = "New Year", IsRecurring = false, Date = new DateTime(2026, 1, 1) }
            };

            _mockHolidayService = new Mock<IHolidayService>();
            _mockHolidayService.Setup(service => service.GetAllHolidaysAsync()).ReturnsAsync(mockRecurringHolidays);

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddScoped<IHolidayService>(_ => _mockHolidayService.Object);
                    });
                });

            _client = _factory.CreateClient();
        }

        [Test]
        public async Task GetAllHolidays_ReturnsResult_WhenHolidaysExist()
        {
            var response = await _client.GetAsync("/api/holiday/GetHolidays");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var rawJson = await response.Content.ReadAsStringAsync();
            var responseBody = JsonConvert.DeserializeObject<dynamic>(rawJson);

            Assert.IsNotNull(responseBody);

            var result = responseBody?.result;
            Assert.IsNotNull(result, "Result field is missing or null.");

            int? count = responseBody?.count;
            Assert.IsTrue(count.HasValue, "Count field is missing or null.");
            Assert.AreEqual(2, count.Value, "Returned count doesn't match the actual holidays.");

            var firstHoliday = result?[0];
            Assert.AreEqual("Christmas", (string)firstHoliday?.name, "Holiday name mismatch.");

            var expectedDate = new DateTime(2025, 12, 25).ToString("MM/dd/yyyy HH:mm:ss");
            Assert.AreEqual(expectedDate, (string)firstHoliday?.date, "Holiday date mismatch.");
        }


        [TearDown]
        public void TearDown()
        {
            _client?.Dispose(); // Dispose HttpClient 
            _factory?.Dispose(); // Dispose WebAppFactory
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _factory?.Dispose();
        }

    }
}
