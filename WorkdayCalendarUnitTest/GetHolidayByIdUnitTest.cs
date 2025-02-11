using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Newtonsoft.Json;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using WorkdayCalendar.IService;
using WorkdayCalendar.Models;

namespace WorkdayCalendarUnitTest
{
    internal class GetHolidayByIdUnitTest
    {

        private HttpClient _client;
        private WebApplicationFactory<Program> _factory;
        private Mock<IHolidayService> _mockHolidayService;

        [SetUp]
        public void SetUp()
        {

            var mockRecurringHolidays = new List<Holiday>
            {
                new Holiday { Id = new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6"), Name = "Christmas", IsRecurring = true, Date = new DateTime(2025, 12, 25) },
                new Holiday { Id = new Guid("3fa87f64-5717-4562-b3fc-2c963f66afa6"), Name = "New Year", IsRecurring = false, Date = new DateTime(2026, 1, 1) }
            };

            var firstHoliday = mockRecurringHolidays.First();

            // Setup the mock service
            _mockHolidayService = new Mock<IHolidayService>();

            // This setup returns the first holiday when a valid ID is passed
            _mockHolidayService.Setup(service => service.GetHolidayByIdAsync(It.Is<Guid>(id => id == firstHoliday.Id)))
                               .ReturnsAsync(firstHoliday);

           
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

        [TestCase("3fa85f64-5717-4562-b3fc-2c963f66afa6")]
        public async Task GetHolidayById_ReturnsResult_WhenHolidaysExist(Guid id)
        {
            var requestUrl = $"/api/holiday/GetHolidaysById?id={id}";
            var response = await _client.GetAsync(requestUrl);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var rawJson = await response.Content.ReadAsStringAsync();
            var responseBody = JsonConvert.DeserializeObject<dynamic>(rawJson);

            Assert.IsNotNull(responseBody);

            var result = responseBody?.result;
            Assert.IsNotNull(result, "Result field is missing or null.");

            Assert.AreEqual("Christmas", (string)result?.name, "Holiday name mismatch.");

            var expectedDate = new DateTime(2025, 12, 25).ToString("MM/dd/yyyy HH:mm:ss");
            Assert.AreEqual(expectedDate, (string)result?.date, "Holiday date mismatch.");
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
