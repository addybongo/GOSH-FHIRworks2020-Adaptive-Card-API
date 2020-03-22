using System.Collections.Generic;
using System.Threading.Tasks;
using HDR_UK_Web_Application.IServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace HDR_UK_Web_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ObservationController : ControllerBase
    {

        private readonly IObservationService _service;

        public ObservationController(IObservationService service)
        {
            _service = service;
        }

        // GET: api/Observation/<patient ID>
        [HttpGet("{id}", Name = "GetObservations")]
        public async Task<List<JObject>> GetObservations(string id)
        {
            return await _service.GetPatientObservations(id);
        }

        // GET: api/Observation/single/<observation ID>
        [HttpGet("single/{id}", Name = "GetSingleObservation")]
        public async Task<JObject> GetSingleObservation(string id)
        {
            return await _service.GetSingleObservation(id);
        }

        // GET: api/Observation/pages/<number of pages>/<patient ID>
        [HttpGet("pages/{pages}/{id}", Name = "GetPatientObservationPages")]
        public async Task<List<JObject>> GetPatientObservationPages(string id, int pages)
        {
            return await _service.GetPatientObservationPages(id, pages);
        }



    }
}