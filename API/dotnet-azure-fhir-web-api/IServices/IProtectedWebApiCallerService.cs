using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace HDR_UK_Web_Application.IServices
{
    public interface IProtectedWebApiCallerService
    {
        Task<JObject> ProtectedWebApiCaller(string webApiUrl);
    }
}
