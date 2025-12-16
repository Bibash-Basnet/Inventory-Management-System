//namespace Inventory.Services
//{
//    public interface IApiClient
//    {
//        Task<T> GetAsync<T>(string endpoint);
//        Task<T> PostAsync<T>(string endpoint, object data);
//        Task<T> PutAsync<T>(string endpoint, object data);
//        Task<bool> DeleteAsync(string endpoint);
//        Task<T> PostFormDataAsync<T>(string endpoint, MultipartFormDataContent formData);
//        Task<T> PutFormDataAsync<T>(string endpoint, MultipartFormDataContent formData);
//        void SetAuthenticationHeader(string scheme, string parameter);
//        void SetHeader(string name, string value);
//    }
//}
namespace Inventory.Services
{
    public interface IApiClient
    {
        Task<T> GetAsync<T>(string endpoint);
        Task<T> PostAsync<T>(string endpoint, object data);
        Task<T> PutAsync<T>(string endpoint, object data);
        Task<bool> DeleteAsync(string endpoint);
        Task<T> PostFormDataAsync<T>(string endpoint, MultipartFormDataContent formData);
        Task<T> PutFormDataAsync<T>(string endpoint, MultipartFormDataContent formData);
        void SetAuthenticationHeader(string scheme, string parameter);
        void SetHeader(string name, string value);
    }
}