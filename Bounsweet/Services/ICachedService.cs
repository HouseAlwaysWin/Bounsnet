namespace Bounsweet.Services
{
    public interface ICachedService
    {
        Task<bool> DeleteAsync(string id);
        T GetAndSet<T>(string key, Func<T> acquire, TimeSpan? time = null) where T : class;
        Task<T> GetAndSetAsync<T>(string key, Func<Task<T>> acquire, TimeSpan? time = null) where T : class;
        Task<T> GetAndSetJsonDataAsync<T>(string key, T data, TimeSpan? time = null) where T : class;
        T GetJsonData<T>(string key) where T : class;
        Task<T> GetJsonDataAsync<T>(string key) where T : class;
        bool SetJsonData<T>(string key, T data, TimeSpan? time) where T : class;
        Task<bool> SetJsonDataAsync<T>(string key, T data, TimeSpan? time = null) where T : class;
    }
}