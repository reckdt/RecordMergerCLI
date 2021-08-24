using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RecordMergerAPI.Tests
{
    public class Tests : IClassFixture<WebApplicationFactory<RecordMergerAPI.Startup>>
    {
        private readonly WebApplicationFactory<RecordMergerAPI.Startup> _factory;

        public Tests(WebApplicationFactory<RecordMergerAPI.Startup> factory)
        {
            _factory = factory;
        }

        private async Task PostRecordLines()
        {
            HttpClient client = _factory.CreateClient();

            ExpectedObject data = await PostAsync(client, "records", 
                "{\"Line\": \"Smith | John | johnsmith@example.com | Blue | 01/01/1990\"}");
            data = await PostAsync(client, "records", 
                "{\"Line\": \"Smith | Jane | janesmith@example.com | Green | 02/02/1990\"}");
            data = await PostAsync(client, "records", 
                "{\"Line\": \"Miller, Mike, mikemiller@example.com, Red, 05/05/1993\"}");
            data = await PostAsync(client, "records", 
                "{\"Line\": \"Miller, Jessica, jessicamiller@example.com, Blue, 03/03/1989\"}");
            data = await PostAsync(client, "records", 
                "{\"Line\": \"Williams Nick nickwilliams@example.com Purple 12/12/1985\"}");
            data = await PostAsync(client, "records", 
                "{\"Line\": \"Davis John johndavis@example.com Teal 11/01/1986\"}");

            Assert.Equal(6, data.id);
            Assert.Equal("Davis John johndavis@example.com Teal 11/01/1986", data.line);
            Assert.Equal(' ', data.delimiter);
        }

        [Fact]
        public async Task TestGetSortedByColor()
        {
            HttpClient client = _factory.CreateClient();
            await PostRecordLines();
            var response = await GetAsync(client, "color");
            var expected = string.Concat(
                "{",
                    "\"body\":",
                        "\"Smith,John,johnsmith@example.com,Blue,1/1/1990\\n",
                        "Miller,Jessica,jessicamiller@example.com,Blue,3/3/1989\\n",
                        "Smith,Jane,janesmith@example.com,Green,2/2/1990\\n",
                        "Williams,Nick,nickwilliams@example.com,Purple,12/12/1985\\n",
                        "Miller,Mike,mikemiller@example.com,Red,5/5/1993\\n",
                        "Davis,John,johndavis@example.com,Teal,11/1/1986",
                "\"}"
            );
            await AssertResponseAsync(response, expected);
        }

        [Fact]
        public async Task TestGetSortedByDOB()
        {
            HttpClient client = _factory.CreateClient();

            var response = await GetAsync(client, "birthdate");
            var expected = string.Concat(
                "{",
                    "\"body\":",
                        "\"Williams,Nick,nickwilliams@example.com,Purple,12/12/1985\\n",
                        "Davis,John,johndavis@example.com,Teal,11/1/1986\\n",
                        "Miller,Jessica,jessicamiller@example.com,Blue,3/3/1989\\n",
                        "Smith,John,johnsmith@example.com,Blue,1/1/1990\\n",
                        "Smith,Jane,janesmith@example.com,Green,2/2/1990\\n",
                        "Miller,Mike,mikemiller@example.com,Red,5/5/1993",
                "\"}"
            );
            await AssertResponseAsync(response, expected);
        }

        [Fact]
        public async Task TestGetSortedByLastName()
        {
            HttpClient client = _factory.CreateClient();

            var response = await GetAsync(client, "name");
            var expected = string.Concat(
                "{",
                    "\"body\":",
                        "\"Davis,John,johndavis@example.com,Teal,11/1/1986\\n",
                        "Miller,Mike,mikemiller@example.com,Red,5/5/1993\\n",
                        "Miller,Jessica,jessicamiller@example.com,Blue,3/3/1989\\n",
                        "Smith,John,johnsmith@example.com,Blue,1/1/1990\\n",
                        "Smith,Jane,janesmith@example.com,Green,2/2/1990\\n",
                        "Williams,Nick,nickwilliams@example.com,Purple,12/12/1985",
                "\"}"
            );
            await AssertResponseAsync(response, expected);
        }

        [Fact]
        public async Task TestBadRequest()
        {
            HttpClient client = _factory.CreateClient();

            var url = "records";
            var str = "{\"Line\": \"Smith:John:johnsmith@example.com:Blue:01/01/1990\"}";

            var content = new StringContent(str, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(url, content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private static async Task<HttpResponseMessage> GetAsync(HttpClient client, string sort)
        {
            HttpResponseMessage response = await client.GetAsync($"records/{sort}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            return response;
        }

        private static async Task<ExpectedObject> PostAsync(HttpClient client, string url, string str)
        {
            var content = new StringContent(str, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(url, content);
            string responseText = await response.Content.ReadAsStringAsync();
            ExpectedObject responseData = JsonSerializer.Deserialize<ExpectedObject>(responseText);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

            return responseData;
        }

        private static async Task AssertResponseAsync(HttpResponseMessage response, string expected)
        {
            byte[] bytes = await response.Content.ReadAsByteArrayAsync();
            string decoded = Encoding.UTF8.GetString(bytes);
            Assert.Equal(expected, decoded);
        }

        private class ExpectedObject
        {
            public long id { get; set; }
            public string line { get; set; }
            public char delimiter { get; set; } 
        }
    }
  
}
