using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HDR_UK_Web_Application.IServices
{
    public interface IResourceFetchService
    {
        Task<JObject> GetSinglePage(string requestOptions);
        Task<List<JObject>> GetAllPages(string requestOptions);
        Task<List<JObject>> GetPages(string requestOptions, int pages);
    }
}
