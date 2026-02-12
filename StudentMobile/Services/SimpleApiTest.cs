using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace StudentMobile.Services
{
    public class SimpleApiTest
    {
        public static async Task<string> TestApiCall()
        {
            try
            {
                using var client = new HttpClient();
                // Test with our new test endpoint
                var response = await client.GetAsync("http://localhost:5156/api/account/test");
                return $"Status: {response.StatusCode}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
