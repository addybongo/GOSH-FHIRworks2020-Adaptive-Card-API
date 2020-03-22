using System.Collections.Generic;
using System.Threading.Tasks;
using HDR_UK_Web_Application.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace HDR_UK_Web_Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class searchController : ControllerBase
    {

        private readonly searchSimpson _service;

        public searchController(searchSimpson service)
        {
            _service = service;
        }
        
        [HttpGet( "patient", Name = "searchPatients")]
        public async Task<List<JObject>> searchPatients(string firstname, string surname, string dob)
        {
            return await _service.searchPatients(firstname, surname, dob);
        }
        
        [HttpGet("observation/{id}", Name = "searchObservations")]
        public async Task<JObject> searchObservations(string id)
        {
            return await _service.searchObservations(id);
        }
        
    }
}