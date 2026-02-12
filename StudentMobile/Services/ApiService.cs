using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using StudentMobile.Models;

namespace StudentMobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        //private const string BaseUrl = "https://10.0.2.2:7298/api/";
        private const string BaseUrl = "https://localhost:7298/api/";

        public ApiService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            _httpClient = new HttpClient(handler);
        }

        public async Task<List<Student>> GetStudentsAsync()
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}attendance/students");

            if (!response.IsSuccessStatusCode)
                return new List<Student>();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<Student>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<bool> MarkAllAttendanceAsync(List<AttendanceDto> attendances)
        {
            var json = JsonSerializer.Serialize(attendances);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{BaseUrl}attendance/markall", content);

            return response.IsSuccessStatusCode;
        }
    }

}
