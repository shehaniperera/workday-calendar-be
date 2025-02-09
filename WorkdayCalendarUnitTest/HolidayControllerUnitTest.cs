using System.Net.Http.Json;
using System.Net;
using WorkdayCalendar.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WorkdayCalendar.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace WorkdayCalendarUnitTest
{
    public class HolidayControllerTests
    {
        private readonly WebApplicationFactory<Program> _factory;
        private HttpClient _client;

        public HolidayControllerTests()
        {
            // Set up a custom WebApplicationFactory to use in-memory database for testing
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Replace the real DbContext with an in-memory database for testing
                        services.AddDbContext<WorkdayCalendarDBContext>(options =>
                            options.UseInMemoryDatabase("TestDatabase"));
                    });
                });
        }

        [SetUp]
        public void Setup()
        {
            _client = _factory.CreateClient();  // Creates the HttpClient for testing
        }

        [Test]
        public async Task AddHoliday_ReturnsOk_WhenHolidayIsAdded()
        {
            var holiday = new Holiday
            {
                Id = Guid.NewGuid(),
                Name = "Test Holiday",
                Date = DateTime.Now
            };

            var response = await _client.PostAsJsonAsync("/api/holiday/AddHoliday", holiday);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.AreEqual("Holiday added successfully!", responseBody.GetProperty("message").ToString());
        }

        [Test]
        public async Task GetHolidays_ReturnsOk_WhenHolidaysExist()
        {
            var response = await _client.GetAsync("/api/holiday/GetHolidays");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();


            if (responseBody.TryGetProperty("result", out var result))
            {
                Assert.IsNotNull(result);
                Assert.IsTrue(result.ValueKind == JsonValueKind.Array);
                Assert.IsTrue(result.GetArrayLength() > 0); 
            }
            else
            {
                //fail the test
                Assert.Fail("The 'result' property was not found in the response.");
            }
        }


        [TearDown] // Clean up after each test
        public void TearDown()
        {
            _client?.Dispose(); // Dispose of the HttpClient after each test
        }

        [OneTimeTearDown] // Clean up once after all tests in the class have run
        public void OneTimeTearDown()
        {
            _factory?.Dispose(); // Dispose of the WebApplicationFactory once after all tests have run
        }
    }
}
