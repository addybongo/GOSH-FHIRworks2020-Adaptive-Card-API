using HDR_UK_Web_Application.IServices;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HDR_UK_Web_Application.Services
{
    public class PatientService : IPatientService
    {
        private static readonly string requestOption = "/Patient/";
        private readonly IResourceFetchService _resource;
        private readonly ILoggerManager _logger;

        public PatientService(IResourceFetchService resource, ILoggerManager logger)
        {
            _resource = resource;
            _logger = logger;
        }

        public async Task<List<JObject>> GetPatients()
        {
            _logger.LogInfo("Class: PatientService, Method: GetAllPages");
            return await _resource.GetAllPages(requestOption);
        }

        public async Task<List<JObject>> GetPatientPages(int pages)
        {
            _logger.LogInfo("Class: PatientService, Method: GetPatientPages");
            return await _resource.GetPages(requestOption, pages);
        }

        public async Task<JObject> GetPatient(string id)
        {
            _logger.LogInfo("Class: PatientService, Method: GetPatient");
            return await _resource.GetSinglePage($"{requestOption}{id}");
        }









    }
}
