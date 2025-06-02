using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using zoomIntegration.models;

namespace zoomIntegration.services
{
    public class ZoomService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public ZoomService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _config = config;
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            var clientId = _config["Zoom:ClientId"];
            var clientSecret = _config["Zoom:ClientSecret"];
            var accountId = _config["Zoom:AccountId"];

            using var client = new HttpClient();

            var byteArray = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var form = new Dictionary<string, string>
    {
        { "grant_type", "account_credentials" },
        { "account_id", accountId }
    };

            var response = await client.PostAsync("https://zoom.us/oauth/token", new FormUrlEncodedContent(form));
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // هنا نطبع الخطأ الكامل اللي رجعه Zoom
                throw new Exception($"Zoom Token Request failed with status {response.StatusCode}: {json}");
            }

            var tokenObject = System.Text.Json.JsonDocument.Parse(json);
            return tokenObject.RootElement.GetProperty("access_token").GetString();
        }


        public async Task<zoomResponse?> CreateMeetingAsync()
        {
            var accessToken = await GetAccessTokenAsync();

            var meetingData = new
            {
                topic = "Test Meeting",
                type = 2,
                start_time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                duration = 30,
                timezone = "UTC"
            };

            var json = JsonSerializer.Serialize(meetingData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.zoom.us/v2/users/me/meetings");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            var responseJson = await response.Content.ReadAsStringAsync();

            Console.WriteLine("======== Zoom API Response Debugging ========");
            Console.WriteLine($"HTTP Status: {response.StatusCode}");
            Console.WriteLine($"Response JSON: {responseJson}");
            Console.WriteLine("============================================");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Zoom API error: {responseJson}");
            }

            var zoomResponse = JsonSerializer.Deserialize<zoomResponse>(responseJson);

            return zoomResponse;
        }
    
}
}
