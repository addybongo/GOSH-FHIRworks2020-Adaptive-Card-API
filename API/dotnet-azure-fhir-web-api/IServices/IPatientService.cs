using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HDR_UK_Web_Application.IServices
{
    public interface IPatientService
    {
        Task<List<JObject>> GetPatients();
        Task<List<JObject>> GetPatientPages(int pages);
        Task<JObject> GetPatient(string id);
    }
}
