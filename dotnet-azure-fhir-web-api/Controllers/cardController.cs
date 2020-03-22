using System.Collections.Generic;
using System.Threading.Tasks;
using HDR_UK_Web_Application.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace HDR_UK_Web_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class cardController : ControllerBase
    {

        private readonly cardService _service;

        public cardController(cardService service)
        {
            _service = service;
        }

        // GET: api/Card/patients/search?id&surname&dob
        [HttpGet("observation/search", Name = "getSingleObservationCard")]
        public async Task<JObject> getSingleObservationCard(string patient, string date, int observation)
        {
            return await _service.getAppointmentCard(patient, date, observation);
        }
        
        // GET: api/Card/patients/search?id&surname&dob
        [HttpGet("patient/search", Name = "getPatientCardFromSearch")]
        public async Task<List<JObject>> getPatientCardFromSearch(string firstname, string surname, string dob)
        {
            return await _service.getPatientCardBySearch(firstname, surname, dob);
        }
        
        // GET: api/Card/patients/<patient ID>
        [HttpGet("patient/{id}", Name = "getPatientCard")]
        public async Task<JObject> getPatientCard(string id)
        {
            return await _service.getPatientCardByID(id);
        }
        //hi ilysm bye
    }
}