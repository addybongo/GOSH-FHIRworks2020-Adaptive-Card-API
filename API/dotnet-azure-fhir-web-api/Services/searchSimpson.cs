using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HDR_UK_Web_Application.IServices;
using HDR_UK_Web_Application.Models;
using Newtonsoft.Json.Linq;

namespace HDR_UK_Web_Application.Services
{
    public class searchSimpson
    {
        private static readonly string requestOption = "/search/";
        private readonly ILoggerManager _logger;
        private readonly IPatientService _patientService;
        private IObservationService _observationService;

        public searchSimpson(ILoggerManager logger, IPatientService patientService, IObservationService observationService)
        {
            _logger = logger;
            _patientService = patientService;
            _observationService = observationService;
        }

        //Searches patient by first name, surname and date of birth
        public async Task<List<JObject>> searchPatients(string firstname, string surname, string dob)
        {
            List<JObject> patients = await _patientService.GetPatients();
            
            List<JObject> results = recursivePatientSearch(patients, firstname.ToLower(), surname.ToLower(), dob);

            return results;
        }
        
        //Gets observations and sorts by appointment date
        public async Task<JObject> searchObservations(string id)
        {
            List<JObject> observations = await _observationService.GetPatientObservations(id);
            
            JObject results = recursiveObservationSearch(observations, new JObject(), new Dictionary<string, int>());

            return results;
        }

        //Helper method used to recursively find patients from bundles
        private List<JObject> recursivePatientSearch(List<JObject> patients, string firstname, string surname, string dob)
        {
            List<JObject> searchedPatients = new List<JObject>();
            
            for (int i = 0; i < patients.Count; i++)
            {
                if ((string) patients[i].SelectToken("resourceType") == "Bundle")
                {
                    List<JObject> patientSubsearch = new List<JObject>();
                    int count = patients[i].SelectToken("entry").Count();

                    for (int j = 0; j < count; j++)
                    {
                        patientSubsearch.Add((JObject) patients[i].SelectToken("entry")[j].SelectToken("resource"));
                    }
                    
                    searchedPatients.AddRange(recursivePatientSearch(patientSubsearch, firstname, surname, dob));
                }
                else if((string) patients[i].SelectToken("resourceType") == "Patient" && searchIndividualPatientName(patients[i], firstname, surname, dob))
                {
                    searchedPatients.Add(patients[i]);
                }
            }

            return searchedPatients;
        }

        //Helper method used to recursively find observations from bundles
        private JObject recursiveObservationSearch(List<JObject> observations, JObject sortedObservations,
            Dictionary<string, int> dateMappings)
        {
            for (int i = 0; i < observations.Count; i++)
            {
                if ((string) observations[i].SelectToken("resourceType") == "Bundle")
                {
                    List<JObject> observationSubsearch = new List<JObject>();
                    int count = observations[i].SelectToken("entry").Count();

                    for (int j = 0; j < count; j++)
                    {
                        observationSubsearch.Add((JObject) observations[i].SelectToken("entry")[j]
                            .SelectToken("resource"));
                    }
                    recursiveObservationSearch(observationSubsearch, sortedObservations, dateMappings);
                }
                else if ((string) observations[i].SelectToken("resourceType") == "Observation")
                {
                    ObservationModel observationModel = observations[i].ToObject<ObservationModel>();
                    string date = observationModel.EffectiveDateTime.ToString("yyyyMMdd");
                    int id;
                    if (dateMappings.TryGetValue(date, out id))
                    {
                        JArray items = (JArray) sortedObservations.SelectToken(date).SelectToken("items");
                        valueContainer(items, observationModel);
                    }
                    else
                    {
                        int newKey = sortedObservations.Count;
                        JArray items = new JArray();
                        valueContainer(items, observationModel);

                        JProperty entry = new JProperty(date,
                            new JObject
                            {
                                new JProperty("items", items)
                            }
                        );
                        sortedObservations.Add(entry);
                        dateMappings.Add(date, newKey);
                    }
                }
            }
            return sortedObservations;
        }

        private void valueContainer(JArray values, ObservationModel observation)
        {
            //If an observation has more than one value, it stores it as a component of ValueQuantity properties
            //We loop over them and add them to our array if this is the case
            if (observation.Component != null)
            {
                for (int i = 0; i < observation.Component.Length; i++)
                {
                    JObject entry = new JObject
                    {
                        new JProperty("type", observation.Code.Text),
                        new JProperty("value", getCorrectValueTypeForComponent(observation.Component[i]).Value),
                        new JProperty("unit", getCorrectValueTypeForComponent(observation.Component[i]).Unit)
                    };
                    values.Add(entry);
                }
            }
            //The observation may be stored as a valueCodeableConcept, which is just a comment about a topic
            //e.g. is the patient a smoker- the value stored might be "former smoker"
            else if(observation.valueCodeableConcept != null)
            {
                values.Add(new JObject
                {
                    new JProperty("type", observation.Code.Text),
                    new JProperty("value",
                        observation.valueCodeableConcept.value)
                });
            }
            else
            {
                values.Add(new JObject
                {
                    new JProperty("type", observation.Code.Text),
                    new JProperty("value",
                        getCorrectValueType(observation).Value),
                    new JProperty("unit",
                        getCorrectValueType(observation).Unit)
                });
            }
        }

        private ValueClass getCorrectValueType(ObservationModel observationModel)
        {
            if (observationModel.valueQuantity != null) return observationModel.valueQuantity;
            if (observationModel.valueBoolean != null) return observationModel.valueBoolean;
            if (observationModel.valueInteger != null) return observationModel.valueInteger;
            if (observationModel.valueString != null) return observationModel.valueString;
            if (observationModel.valueRange != null) return observationModel.valueRange;
            return new ValueClass{
                Value = 0,
                Unit = "N/A",
                Code = "Error: Missing type of value from FHIR. Please update parser to include this new type",
                System = new Uri("N/A")
                };
        }
        
        //Made this because I'm lazy to handle Components
        private ValueClass getCorrectValueTypeForComponent(valueComponent observationModel)
        {
            if (observationModel.valueQuantity != null) return observationModel.valueQuantity;
            if (observationModel.valueBoolean != null) return observationModel.valueBoolean;
            if (observationModel.valueInteger != null) return observationModel.valueInteger;
            if (observationModel.valueString != null) return observationModel.valueString;
            if (observationModel.valueRange != null) return observationModel.valueRange;
            return new ValueClass{
                Value = 0,
                Unit = "N/A",
                Code = "Error: Missing type of value from FHIR. Please update parser to include this new type",
                System = new Uri("N/A")
            };
        }

        private bool searchIndividualPatientName(JObject patient, string firstname, string surname, string dob)
        {
            cardService stevieWonder = new cardService(_logger, _patientService, this);
            PatientModel patientModel = patient.ToObject<PatientModel>();
            
            string patientName = stevieWonder.getPatientName(patientModel);
            
            //Check first name, surname and DOB conform to the parameters for search
            if (firstname != null && !patientName.ToLower().Contains(firstname)) return false;
            if (surname != null && !patientName.ToLower().Contains(surname)) return false;
            string patientDateOfBirth = patientModel.BirthDate.ToString("yyyyMMdd");
            if (dob != null && patientModel.BirthDate.ToString("yyyyMMdd") != dob) return false;
            
            //If all conditions passed, return true
            return true;
        }
    }
}