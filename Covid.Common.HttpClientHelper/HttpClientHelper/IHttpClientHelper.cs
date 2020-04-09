using Covid.Common.HttpClientHelper.Model;
using System.Threading.Tasks;

namespace Covid.Common.HttpClientHelper
{
    public interface IHttpClientHelper
    {
        Task<AsyncResult<int>> PostAsync<T>(string resourceUri, T request, string apiVersion) where T : class;

        Task<bool> PutAsync<T>(string resourceUri, T request, string apiVersion) where T : class;

        Task<bool> PatchAsync<T>(string resourceUri, T request, string apiVersion) where T : class;

        Task<bool> DeleteAsync(string resourceUri, string apiVersion);

        Task<AsyncResult<T>> GetAsync<T>(string resourceUri, string apiVersion) where T : class;
    }
}
