using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdaptiveCards;
using HDR_UK_Web_Application.Cards;
using HDR_UK_Web_Application.Controllers;
using HDR_UK_Web_Application.IServices;
using HDR_UK_Web_Application.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HDR_UK_Web_Application.Services
{
    public class cardService
    {
        private static readonly string requestOption = "/Card/";
        private readonly ILoggerManager _logger;
        private readonly IPatientService _patientService;
        private readonly searchSimpson _searchSimpson;

        public cardService(ILoggerManager logger, IPatientService patientService, searchSimpson searchSimpson)
        {
            _logger = logger;
            _patientService = patientService;
            _searchSimpson = searchSimpson;
        }

        public async Task<JObject> getPatientCardByID(string id)
        {
            _logger.LogInfo("Class: cardService, Method: getPatientCard");
            JObject json = await _patientService.GetPatient(id);

            //If the patient ID doesn't exist, we return the error JSON as is
            if (json.ContainsKey("OperationOutcome"))
            {
                return json;
            }
            
            PatientModel patient = json.ToObject<PatientModel>();

            return standardPatientCardCreator(patient);
        }
        
        public async Task<List<JObject>> getPatientCardBySearch(string firstname, string surname, string dob)
        {
            _logger.LogInfo("Class: cardService, Method: getPatientCardFromSearch");
            List<JObject> json = await _searchSimpson.searchPatients(firstname, surname, dob);
            
            if (json.Count == 0)
            {
                string error = File.ReadAllText("Cards/errorJson.json");
                List<JObject> result = new List<JObject>();
                result.Add(JObject.Parse(error));
                return result;
            }
            
            List<JObject> cards = new List<JObject>();

            for (int i = 0; i < json.Count; i++)
            {
                string card = simplePatientCardCreator(json[i].ToObject<PatientModel>()).ToJson();
                cards.Add((JObject) JsonConvert.DeserializeObject(card));
            }

            return cards;
        }

        public async Task<JObject> getAppointmentCard(string patientId, string date, int observationId)
        {
            JObject observationsByPerson = await _searchSimpson.searchObservations(patientId);
            JToken dateEntry, observationEntry;
            bool successfulSearch = observationsByPerson.TryGetValue(date, out dateEntry);
            
            if (!successfulSearch) return generateErrorMessage("Observation Not Found", "No Observation found with that date"); 
            
            try { observationEntry = dateEntry.SelectToken("items")[observationId]; }
            catch (Exception e) {
                return generateErrorMessage("Observation Not Found", "No Observation found with that observation id");
            }

            return singleObservationCardCreator(date, (string)observationEntry["type"], (string)observationEntry["value"],
                (string)observationEntry["unit"]);
        }

        private dynamic singleObservationCardCreator(string date, string type, string value, string unit)
        {
            cardCreator creator = new cardCreator();
            AdaptiveCard card = new AdaptiveCard();

            DateTime dateFormatted = DateTime.ParseExact(date, "yyyyMMdd", null);
            
            card.Body.Add(creator.createTitle(type));
            card.Body.Add(creator.createSection("Observation Taken on " + dateFormatted.ToString("dd/MM/yyyy"),
                new string[] {type}, new string[] {value + " " + unit}));
            return JsonConvert.DeserializeObject(card.ToJson());
        }

        //Creates a simple patient card result
        private AdaptiveCard simplePatientCardCreator(PatientModel patient)
        {
            cardCreator creator = new cardCreator();
            AdaptiveCard card = new AdaptiveCard();
            
            card.SelectAction = new AdaptiveSubmitAction()
            {
                Id = "Select Patient",
                Data = patient.id,
                Title = "Show Patient"
            };
            
            card.Body.Add(creator.createTitle(getPatientName(patient)));

            //Create two columns; puts an image in the first one, leaves the other empty (image is intended as a user icon)
            AdaptiveColumnSet profileContainer = creator.createPhotoTextColumnSection(
                "https://www.kindpng.com/picc/m/495-4952535_create-digital-profile-icon-blue-user-profile-icon.png", 60);
            
            //Adds the user's name at the top
            profileContainer.Columns[1].Items.Add(new AdaptiveTextBlock()
            {
                Text = "id: " + patient.id,
                Size = AdaptiveTextSize.Small,
                Weight = AdaptiveTextWeight.Bolder,
                Wrap = true
            });

            //Profile Section (gender, DOB, Deceased Date)
            string[] profileHeadings = {"Gender: ", "Date of Birth: ", "Deceased Date: "};
            string[] profile =
            {patient.Gender, patient.BirthDate.ToString("dd/MM/yyyy"), 
                patient.DeceasedDateTime == DateTimeOffset.MinValue ? "N/A" : patient.DeceasedDateTime.ToString("dd/MM/yyyy")};

            profileContainer.Columns[1].Items.Add(creator.createDualTextColumnSection(profileHeadings, profile));
            card.Body.Add(profileContainer);

            return card;
        }
        
        //Creates a standard, more detailed card
        private dynamic standardPatientCardCreator(PatientModel patient)
        {
            cardCreator creator = new cardCreator();
            AdaptiveCard card = new AdaptiveCard();
            
            card.Body.Add(creator.createTitle("Patient Record"));

            //Create two columns; puts an image in the first one, leaves the other empty (image is intended as a user icon)
            AdaptiveColumnSet profileContainer = creator.createPhotoTextColumnSection(
                "https://www.kindpng.com/picc/m/495-4952535_create-digital-profile-icon-blue-user-profile-icon.png", 90);
            
            //Adds the user's name at the top
            profileContainer.Columns[1].Items.Add(new AdaptiveTextBlock()
            {
                Text = getPatientName(patient),
                Size = AdaptiveTextSize.Medium,
                Weight = AdaptiveTextWeight.Bolder
            });

            //Profile Section (gender, DOB, Deceased Date)
            string[] profileHeadings = {"Gender: ", "Date of Birth: ", "Deceased Date: "};
            string[] profile =
            {patient.Gender, patient.BirthDate.ToString("dd/MM/yyyy"), 
                patient.DeceasedDateTime == DateTimeOffset.MinValue ? "N/A" : patient.DeceasedDateTime.ToString("dd/MM/yyyy")};

            profileContainer.Columns[1].Items.Add(creator.createDualTextColumnSection(profileHeadings, profile));
            card.Body.Add(profileContainer);

            //Personal Information Section
            string[] personal = {patient.MaritalStatus.Text, separatedLanguageString(patient.Communication, ",")};
            string[] personalFields =  {"Marital Status:", "Languages:"};
            card.Body.Add(creator.createSection("Personal Information", personalFields, personal));

            //Medical Section
            string[] medicalFields = {"Medical Practice:", "General Practitioner:"};
            string[] medical = {patient.gpOrganization ?? "N/A", patient.assignedGP ?? "N/A"};
            card.Body.Add(creator.createSection("Medical Information", medicalFields, medical));

            //Address Section
            string[] address =
            {
                listToDelimitedString(patient.Address[0].Line, ","), patient.Address[0].City,
                patient.Address[0].State, patient.Address[0].Country
            };
            card.Body.Add(creator.createSection("Address", address));

            //Contact Section
            string[] contactFields = {patient.Telecom[0].System + ":", "Use:"};
            string[] contacts = {patient.Telecom[0].Value, patient.Telecom[0].Use};
            card.Body.Add(creator.createSection("Contact Details", contactFields, contacts));

            return JsonConvert.DeserializeObject(card.ToJson());
        }
        
        //Separate method to listToDelimitedString because Communication doesn't use an array of strings
        private string separatedLanguageString(Communication[] communicationList, string delimiter)
        {
            string listAsString = "";
            for (int i = 0; i < communicationList.Length; i++)
            {
                listAsString += communicationList[i].Language.Text + delimiter + " ";
            }

            if (listAsString.Length <= 1 ) return "N/A";

            return listAsString.Substring(0, listAsString.Length - 2);
        }

        private JObject generateErrorMessage(string code, string description)
        {
            string error = File.ReadAllText("Cards/errorJson.json");
            dynamic errorJson = JsonConvert.DeserializeObject(error);
            errorJson["issue"][0]["code"] = code;
            errorJson["issue"][0]["diagnostics"] = description;
            return errorJson;
        }
        
        private string listToDelimitedString(String[] list, string delimiter)
        {
            string listAsString = "";
            for (int i = 0; i < list.Length; i++)
            {
                listAsString += list[i] + delimiter + " ";
            }

            if (listAsString.Length <= 1 ) return "N/A";
            
            return listAsString.Substring(0, listAsString.Length - 2);
        }
        
        public string getPatientName(PatientModel patient)
        {
            int i = 0;

            //Searches through the list of names and finds the one which is labelled "official"
            while (i < patient.Name.Length && patient.Name[i].Use != "official")
            {
                ++i;
            }

            Regex numberCatcher = new Regex("[0-9]"); //Used to remove numbers from name
            Name patientNameModel = patient.Name[i];
            string name = "";

            //Concatenates all forenames together as a string
            for (int j = 0; j < patientNameModel.Given.Length; j++)
            {
                name += patientNameModel.Given[j] + " ";
            }

            //Adds the surname to the end and removes all numbers from the string
            return numberCatcher.Replace((name + " " + patientNameModel.Family), "");
        }
    }
}