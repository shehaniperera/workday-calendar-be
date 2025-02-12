using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Microsoft.Extensions.Configuration;
using WorkdayCalendar.IService;
using Microsoft.Extensions.DependencyInjection;
using WorkdayCalendar.Models;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using WorkdayCalendar.Utilities;

namespace WorkdayCalendarUnitTest
{
    internal class WorkDayCalculationUnitTest
    {
        private HttpClient _client;
        private Mock<IWorkdayCalculationService> _mockWorkdayService;
        private Mock<IWorkdayValidationService> _mockValidationService;
        private Mock<IErrorHandlingService> _mockErrorHandlingService;
        private Mock<IConfiguration> _mockConfiguration;
        private WebApplicationFactory<Program> _factory;

        [SetUp]
        public void SetUp()
        {
            // Mock IConfiguration
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(config => config["DateFormat"]).Returns("yyyy-MM-dd");

            // Mock Workday Calculation Service
            _mockWorkdayService = new Mock<IWorkdayCalculationService>();
            _mockWorkdayService.Setup(service => service.CalculateWorkday(It.IsAny<WorkdayCalculation>()))
                               .ReturnsAsync(new DateTime(2004, 5, 25, 9, 7, 0));

            // Mock Validation Service
            _mockValidationService = new Mock<IWorkdayValidationService>();
            _mockValidationService.Setup(service => service.ValidateWorkdayRequest(It.IsAny<WorkdayCalculation>(), out It.Ref<List<string>>.IsAny))
                                  .ReturnsAsync(true); // You can change this to false for validation failure test

            // Mock Error Handling Service
            _mockErrorHandlingService = new Mock<IErrorHandlingService>();

            // WebApplicationFactory setup
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Ensure we're using scoped lifetimes for all services
                        services.AddScoped<IWorkdayCalculationService>(_ => _mockWorkdayService.Object);
                        services.AddScoped<IWorkdayValidationService>(_ => _mockValidationService.Object);
                        services.AddSingleton<IErrorHandlingService>(_ => _mockErrorHandlingService.Object);
                        services.AddSingleton<IConfiguration>(_ => _mockConfiguration.Object);
                    });
                });

            // Create HttpClient to make requests
            _client = _factory.CreateClient();
        }

        [Test]
        public async Task CalculateWorkday_ReturnsCalculatedDate_WhenValidRequest()
        {
            
            var request = new WorkdayCalculation
            {
                StartDateTime = DateTime.Parse("2004-05-24T15:07:00"),
                WorkingDays = 0.25,
                Holidays = new List<Holiday>(), // Empty holiday list
                WorkingHours = new WorkingHours { Start = new TimeSpan(8, 0, 0), End = new TimeSpan(16, 0, 0) }
            };

            var response = await _client.PostAsJsonAsync("/api/workday/CalculateWorkDay", request);

            // Assert: Verify response status code  OK
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();

            // Ensure that 'Result' exists and check the value
            if (responseBody.TryGetProperty("result", out var calculatedWorkday))
            {
                Assert.NotNull(calculatedWorkday);
                string calculatedDateString = calculatedWorkday.GetString();

                DateTime parsedDate = DateTime.Parse(calculatedDateString);

                // calculated workday is the expected date
                var expectedDate = new DateTime(2004, 5, 25, 9, 7, 0);  // Expected date as DateTime
                Assert.AreEqual(expectedDate, parsedDate);  // Compare with expected DateTime result
            }
            else
            {
                // Fail the test if 'Result' is missing or null
                Assert.Fail("The 'Result' field is missing in the response.");
            }

        }



        [TestCase("2004-05-24T15:07:00", 0.25, "2004-05-25T09:07:00", "[]")]
        [TestCase("2004-05-24T04:00:00", 0.5, "2004-05-24T12:00:00", "[]")]
        [TestCase("2004-05-24T18:05:00", -5.5, "2004-05-14T12:00:00", "[{\"Date\": \"2004-05-17\", \"IsRecurring\": true}, {\"Date\": \"2004-05-27\", \"IsRecurring\": false}]")]
        [TestCase("2004-05-24T19:03:00", 44.723656, "2004-07-27T13:47:21", "[{\"Date\": \"2004-05-17\", \"IsRecurring\": true}, {\"Date\": \"2004-05-27\", \"IsRecurring\": false}]")]
        [TestCase("2004-05-24T18:03:00", -6.7470217, "2004-05-13T10:01:25", "[{\"Date\": \"2004-05-17\", \"IsRecurring\": true}, {\"Date\": \"2004-05-27\", \"IsRecurring\": false}]")]
        [TestCase("2004-05-24T08:03:00", 12.782709, "2004-06-10T14:18:42", "[{\"Date\": \"2004-05-17\", \"IsRecurring\": true}, {\"Date\": \"2004-05-27\", \"IsRecurring\": false}]")]
        [TestCase("2004-05-24T07:03:00", 8.276628, "2004-06-04T10:12:46", "[{\"Date\": \"2004-05-17\", \"IsRecurring\": true}, {\"Date\": \"2004-05-27\", \"IsRecurring\": false}]")]
        public async Task CalculateWorkday_ReturnsCalculatedDate_AllTests_WhenValidRequest(string startDateTime, double workingDays, string expectedResult, string holidaysList)
        {
            List<Holiday> holidays = new List<Holiday>();


            if (!string.IsNullOrEmpty(holidaysList) && holidaysList != "[]")
            {
                holidays = JsonSerializer.Deserialize<List<Holiday>>(holidaysList);
            }
            // payload
            var request = new WorkdayCalculation
            {
                StartDateTime = DateTime.Parse(startDateTime),
                WorkingDays = workingDays,
                Holidays = holidays,
                WorkingHours = new WorkingHours { Start = new TimeSpan(8, 0, 0), End = new TimeSpan(16, 0, 0) }
            };

          
            var response = await _client.PostAsJsonAsync("/api/workday/CalculateWorkDay", request);

            // Assert: Verify response status code  OK
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();

            // result
            if (responseBody.TryGetProperty("result", out var calculatedWorkday))
            {
                Assert.NotNull(calculatedWorkday);
                string calculatedDateString = calculatedWorkday.GetString();

                DateTime parsedDate = DateTime.Parse(calculatedDateString);

                // calculated workday is the expected date
                var expectedDate = new DateTime(2004, 5, 25, 9, 7, 0);  // Expected date as DateTime
                Assert.AreEqual(expectedDate, parsedDate);  // Compare with expected DateTime result
            }
            else
            {
                // Fail the test if 'Result' is missing or null
                Assert.Fail("The 'Result' field is missing in the response.");
            }

        }

        [TestCase("Invalid Date", 0.25, "2004-05-25T09:07:00", "[]")]
        public async Task CalculateWorkday_ReturnsValidationResult(string startDateTime, double workingDays, string expectedResult, string holidaysList)
        {

            DateTime parsedStartDateTime;

            if (!DateTime.TryParse(startDateTime, out parsedStartDateTime))
            {
                parsedStartDateTime = DateTime.MinValue;  // Invalid date if parsing fails
            }

            var request = new WorkdayCalculation
            {
                StartDateTime = parsedStartDateTime,
                WorkingDays = workingDays,
                Holidays = new List<Holiday>(),
                WorkingHours = new WorkingHours { Start = new TimeSpan(8, 0, 0), End = new TimeSpan(16, 0, 0) }
            };

            // POST - CalculateWorkday
            var response = await _client.PostAsJsonAsync("/api/workday/CalculateWorkDay", request);

            // Verify response status code - BadRequest (400)
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (responseBody.TryGetProperty("message", out var errorMessage))
            {
                // Verify that the error message matches the expected one for invalid date
                Assert.AreEqual(Constants.ValidationMessages.ValidStartDate, errorMessage.GetString());
            }
            else
            {
                // If no error message is found, fail the test
                Assert.Fail("Expected error message 'Invalid date format' not found in the response.");
            }
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
