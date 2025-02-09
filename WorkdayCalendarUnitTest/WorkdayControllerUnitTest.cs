using System.Net.Http.Json;
using System.Net;
using WorkdayCalendar.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Moq;
using Microsoft.AspNetCore.Http;

namespace WorkdayCalendarUnitTest
{
    public class WorkdayControllerTests
    {
        private HttpClient _client;
        private WebApplicationFactory<Program> _factory;

        [SetUp]
        public void SetUp()
        {
            // Configure the WebApplicationFactory to mock services
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    // Mocking IConfiguration
                    var mockConfig = new Mock<IConfiguration>();
                    var mockStartSection = new Mock<IConfigurationSection>();
                    mockStartSection.Setup(x => x.Value).Returns("08:00"); // Mock "WorkdaySettings:WorkingHours:Start"
                    mockConfig.Setup(x => x.GetSection("WorkdaySettings:WorkingHours:Start")).Returns(mockStartSection.Object);

                    var mockEndSection = new Mock<IConfigurationSection>();
                    mockEndSection.Setup(x => x.Value).Returns("16:00"); // Mock "WorkdaySettings:WorkingHours:End"
                    mockConfig.Setup(x => x.GetSection("WorkdaySettings:WorkingHours:End")).Returns(mockEndSection.Object);

                });

            _client = _factory.CreateClient(); // Create an HttpClient instance for testing
        }

        [Test]
        public async Task CalculateWorkday()
        {
            var request = new WorkdayCalculation
            {
                StartDateTime = DateTime.Parse("2004-05-24T15:07:00"),
                WorkingDays = 0.25,
                Holidays = new List<Holiday>(), // Fixed to an empty list
                WorkingHours = new WorkingHours { Start = new TimeSpan(8, 0, 0), End = new TimeSpan(16, 0, 0) }
            };

            // POST - CalculateWorkday
            var response = await _client.PostAsJsonAsync("/api/workday/CalculateWorkDay", request);

            // Verify response status code - OK
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();

            // Ensure that 'Result' exists and check value
            if (responseBody.TryGetProperty("calculatedWorkday", out var calculatedWorkday))
            {
                Assert.NotNull(calculatedWorkday);
                string calculatedDateString = calculatedWorkday.GetString();

                DateTime parsedDate = DateTime.Parse(calculatedDateString);

               
                // Ensure that the calculated workday is the expected date
                var expectedDate = new DateTime(2004, 5, 25, 9, 7, 0);  // Expected date as DateTime
                Assert.AreEqual(expectedDate, parsedDate);  // Compare with expected DateTime result
            }
            else
            {
                // Fail the test 
                Assert.Fail("The 'calculatedWorkday' was not found in the response.");
            }


        }


        [TestCase("2004-05-24T15:07:00", 0.25, "2004-05-25T09:07:00", "[]")]
        [TestCase("2004-05-24T04:00:00", 0.5, "2004-05-24T12:00:00", "[]")]
        [TestCase("2004-05-24T18:05:00", -5.5, "2004-05-14T12:00:00", "[{\"Date\": \"2004-05-17\", \"IsRecurring\": true}, {\"Date\": \"2004-05-27\", \"IsRecurring\": false}]")]
        [TestCase("2004-05-24T19:03:00", 44.723656, "2004-07-27T13:47:21", "[{\"Date\": \"2004-05-17\", \"IsRecurring\": true}, {\"Date\": \"2004-05-27\", \"IsRecurring\": false}]")]
        [TestCase("2004-05-24T18:03:00", -6.7470217, "2004-05-13T10:01:25", "[{\"Date\": \"2004-05-17\", \"IsRecurring\": true}, {\"Date\": \"2004-05-27\", \"IsRecurring\": false}]")]
        [TestCase("2004-05-24T08:03:00", 12.782709, "2004-06-10T14:18:42", "[{\"Date\": \"2004-05-17\", \"IsRecurring\": true}, {\"Date\": \"2004-05-27\", \"IsRecurring\": false}]")]
        [TestCase("2004-05-24T07:03:00", 8.276628, "2004-06-04T10:12:46", "[{\"Date\": \"2004-05-17\", \"IsRecurring\": true}, {\"Date\": \"2004-05-27\", \"IsRecurring\": false}]")]
        public async Task CalculateWorkday_ReturnsExpectedResult(string startDateTime, double workingDays, string expectedResult, string holidaysList)
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

            // Assert response status - OK
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (responseBody.TryGetProperty("calculatedWorkday", out var calculatedWorkday))
            {
                DateTime calculatedDate = DateTime.Parse(calculatedWorkday.GetString());
                DateTime expectedDate = DateTime.Parse(expectedResult);

                // Compare only up to seconds - ignore millisec
                DateTime calculatedDateWithoutMilliseconds = calculatedDate.AddTicks(-(calculatedDate.Ticks % TimeSpan.TicksPerSecond));
                DateTime expectedDateWithoutMilliseconds = expectedDate.AddTicks(-(expectedDate.Ticks % TimeSpan.TicksPerSecond));

                // Assert if calculated workday date match expected date
                Assert.AreEqual(expectedDateWithoutMilliseconds, calculatedDateWithoutMilliseconds);
            }
            else
            {
                Assert.Fail("The 'calculatedWorkday' was not found in the response.");
            }
        }

        [TestCase("2004-05-24T15:07:00", 0.0, "2004-05-24T15:07:00", "[]")] // zero working days
        [TestCase("InvalidDate", 0.25, "2004-05-25T09:07:00", "[]")] // Invalid start date
       
        public async Task CalculateWorkday_Test_Edge(string startDateTime, double workingDays, string expectedResult, string holidaysList)
        {
            List<Holiday> holidays = new List<Holiday>();

            if (!string.IsNullOrEmpty(holidaysList) && holidaysList != "[]")
            {
                holidays = JsonSerializer.Deserialize<List<Holiday>>(holidaysList);
            }

            // Parse the startDateTime as DateTimeOffset to handle time zone
            DateTimeOffset parsedStartDateTime = DateTimeOffset.Parse(startDateTime);

            // payload
            var request = new WorkdayCalculation
            {
                // Convert DateTimeOffset to DateTime (ignores time zone)
                StartDateTime = parsedStartDateTime.DateTime, // This converts it to DateTime
                WorkingDays = workingDays,
                Holidays = holidays,
                WorkingHours = new WorkingHours { Start = new TimeSpan(8, 0, 0), End = new TimeSpan(16, 0, 0) }
            };

            var response = await _client.PostAsJsonAsync("/api/workday/CalculateWorkDay", request);

            // Assert response status - OK
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (responseBody.TryGetProperty("calculatedWorkday", out var calculatedWorkday))
            {
                DateTimeOffset calculatedDate = DateTimeOffset.Parse(calculatedWorkday.GetString());
                DateTimeOffset expectedDate = DateTimeOffset.Parse(expectedResult);

                // Compare the DateTimeOffset, which includes time zone
                DateTimeOffset calculatedDateWithoutMilliseconds = calculatedDate.AddTicks(-(calculatedDate.Ticks % TimeSpan.TicksPerSecond));
                DateTimeOffset expectedDateWithoutMilliseconds = expectedDate.AddTicks(-(expectedDate.Ticks % TimeSpan.TicksPerSecond));

                // Assert if calculated workday date match expected date, including time zone adjustments
                Assert.AreEqual(expectedDateWithoutMilliseconds, calculatedDateWithoutMilliseconds);
            }
            else
            {
                Assert.Fail("The 'calculatedWorkday' was not found in the response.");
            }
        }


        [Test]
        public async Task CalculateWorkday_ReturnsUnexpectedResult()
        {
            var request = new WorkdayCalculation
            {
                StartDateTime = DateTime.Parse("2004-05-24T15:07:00"),
                WorkingDays = 0.25,
                Holidays = new List<Holiday>(),
                WorkingHours = new WorkingHours { Start = new TimeSpan(8, 0, 0), End = new TimeSpan(16, 0, 0) }
            };

            // POST - CalculateWorkday
            var response = await _client.PostAsJsonAsync("/api/workday/CalculateWorkDay", request);

            // Verify response status code - OK
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (responseBody.TryGetProperty("calculatedWorkday", out var calculatedWorkday))
            {
                Assert.NotNull(calculatedWorkday);
                string calculatedDateString = calculatedWorkday.GetString();
                DateTime parsedDate = DateTime.Parse(calculatedDateString);

                var expectedDate = new DateTime(2004, 5, 25, 10, 7, 0);  // Incorrect expected date 

                Assert.AreEqual(expectedDate, parsedDate);  // Compare with expected DateTime result
            }
            else
            {
                // Fail the test
                Assert.Fail("The 'calculatedWorkday' was not found in the response.");
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
