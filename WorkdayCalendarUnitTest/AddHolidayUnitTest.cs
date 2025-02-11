using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using WorkdayCalendar.IService;
using WorkdayCalendar.Models;
using System.Net;

namespace WorkdayCalendarUnitTest
{
    [TestFixture]
    internal class AddHolidayUnitTest
    {
        private HttpClient _client;
        private WebApplicationFactory<Program> _factory;
        private Mock<IHolidayService> _mockHolidayService;

        [SetUp]
        public void SetUp()
        {
            _mockHolidayService = new Mock<IHolidayService>();
            _mockHolidayService.Setup(service => service.AddHolidayAsync(It.IsAny<Holiday>())).ReturnsAsync(true);

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

        [TestCase("3fa85f64-5717-4562-b3fc-2c963f66afa6", "2025-12-25T09:07:00", "Christmas", true)]
        public async Task AddHoliday_ReturnsOk_WhenHolidayAddedSuccessfully(Guid id, string date, string name, bool isRecurring)
        {
            var holiday = new Holiday
            {
                Id = id,
                Date = DateTime.Parse(date),
                Name = name,
                IsRecurring = isRecurring
            };

            var response = await _client.PostAsJsonAsync("/api/holiday/AddHoliday", holiday);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.AreEqual("Holiday added successfully!", responseBody.GetProperty("message").GetString());
        }

        [Test]
        public async Task AddHoliday_ReturnsBadRequest_WhenHolidayIsNull()
        {
            var emptyHoliday = new Holiday
            {
                Id = Guid.Empty,
                Date = DateTime.MinValue,
                Name = null,
                IsRecurring = false
            };

            var response = await _client.PostAsJsonAsync("/api/holiday/AddHoliday", emptyHoliday);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.AreEqual("Holiday has incomplete or empty values", responseBody.GetProperty("message").GetString());
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
