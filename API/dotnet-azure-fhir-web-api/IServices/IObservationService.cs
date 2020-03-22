using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HDR_UK_Web_Application.IServices
{
    public interface IObservationService
    {
        Task<List<JObject>> GetPatientObservations(string id);
        Task<List<JObject>> GetPatientObservationPages(string id, int pages);
        Task<JObject> GetSingleObservation(string id);
    }
}
