using System.Net.Http;
using System.Text;
using System.Text.Json;
using StudentMobile.Models;

namespace StudentMobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5156/api/";

        public ApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}weatherforecast");
                System.Diagnostics.Debug.WriteLine($"Test Connection Response: {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Test Connection Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest login)
        {
            try
            {
                var json = JsonSerializer.Serialize(login);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}account/login", content);

                if (!response.IsSuccessStatusCode) 
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"API Response: {responseJson}");
                
                return JsonSerializer.Deserialize<LoginResponse>(responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<List<AttendanceRecord>> GetMyAttendanceAsync(int studentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}attendance/mine/{studentId}");

                if (!response.IsSuccessStatusCode) return new List<AttendanceRecord>();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<AttendanceRecord>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting attendance: {ex.Message}");
                return new List<AttendanceRecord>();
            }
        }

        public async Task<List<NotificationRecord>> GetNotificationsAsync(int studentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}notifications/student/{studentId}");

                if (!response.IsSuccessStatusCode) return new List<NotificationRecord>();

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<NotificationRecord>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting notifications: {ex.Message}");
                return new List<NotificationRecord>();
            }
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
        {
            try
            {
                var response = await _httpClient.PutAsync($"{BaseUrl}notifications/{notificationId}/read", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marking notification as read: {ex.Message}");
                return false;
            }
        }
    }
}
