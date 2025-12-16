
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Inventory.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        // ✅ ADDED: Automatically inject JWT token from session
        private void SetAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            SetAuthorizationHeader(); // ✅ Inject token
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            SetAuthorizationHeader(); // ✅ Inject token
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        }

        public async Task<T> PutAsync<T>(string endpoint, object data)
        {
            SetAuthorizationHeader(); // ✅ Inject token
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            SetAuthorizationHeader(); // ✅ Inject token
            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }

        public async Task<T> PostFormDataAsync<T>(string endpoint, MultipartFormDataContent formData)
        {
            SetAuthorizationHeader(); // ✅ Inject token
            var response = await _httpClient.PostAsync(endpoint, formData);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        }

        public async Task<T> PutFormDataAsync<T>(string endpoint, MultipartFormDataContent formData)
        {
            SetAuthorizationHeader(); // ✅ Inject token
            var response = await _httpClient.PutAsync(endpoint, formData);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        }

        // Keep these for manual token setting if needed
        public void SetAuthenticationHeader(string scheme, string parameter)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(scheme, parameter);
        }

        public void SetHeader(string name, string value)
        {
            if (_httpClient.DefaultRequestHeaders.Contains(name))
                _httpClient.DefaultRequestHeaders.Remove(name);
            _httpClient.DefaultRequestHeaders.Add(name, value);
        }
    }
}