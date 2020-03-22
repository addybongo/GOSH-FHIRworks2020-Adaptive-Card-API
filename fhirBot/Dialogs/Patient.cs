using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace fhirBot.Dialogs
{
    public class Patient : ComponentDialog
    {
        private static HttpClient client;
        private HttpClientHandler clientHandler;
        private string id;
        
        public Patient() : base(nameof(Patient))
        {
            //Workaround SSL certificate issue- removes certificate validation
            clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            
            client = new HttpClient(clientHandler);
            client.BaseAddress = new Uri("https://localhost:5001/api/");
            client.DefaultRequestHeaders.Accept.Clear();

            var waterfallSteps = new WaterfallStep[]
            {
                patientCard,
                patientInfo,
                rerouteBackToStart
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));


            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> patientCard(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Type optionType = stepContext.Options.GetType();
            if (optionType.IsArray)
            {
                string[] optionArray = (string[]) stepContext.Options;
                id = optionArray[0];
                string apiRequest = "Card/patient/" + id;
                HttpResponseMessage result = await client.GetAsync(apiRequest, cancellationToken);

                if (!result.IsSuccessStatusCode)
                {
                    await stepContext.Context.SendActivityAsync("I'm sorry, I couldn't find a patient with that ID");
                    
                    await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
                }

                string cardAsString = await result.Content.ReadAsStringAsync();
                dynamic jsonObj = JsonConvert.DeserializeObject(cardAsString);

                Attachment card = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = jsonObj
                };

                var response = MessageFactory.Attachment(card);
                await stepContext.Context.SendActivityAsync(response, cancellationToken);

                PromptOptions promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("Hi " + jsonObj["body"][1]["columns"][1]["items"][0]["text"] + ", what would you like to know?"),
                };
                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            }

            return await stepContext.NextAsync(stepContext.Options, cancellationToken);
        }

        private async Task<DialogTurnResult> patientInfo(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            string nextCommand;
            PromptOptions promptOptions;
            string[] data = await getLatestObservationValue((string)stepContext.Result);
            string prompt;

            if (data != null)
            {
                string apiRequest = "Card/observation/search?patient=" + id + "&date=" + data[0] + "&observation=" + data[1];
                HttpResponseMessage result = await client.GetAsync(apiRequest, cancellationToken);

                sendCardFromAPI(result, stepContext, cancellationToken);
                
                prompt = "What else would you like to know?";
            }
            else
            {
                prompt = "Sorry, I couldn't find a measurement matching that name. Please try another category:";
            }

            promptOptions = new PromptOptions { Prompt = MessageFactory.Text(prompt)};
            
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> rerouteBackToStart(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.ReplaceDialogAsync(InitialDialogId, stepContext.Result, cancellationToken);
        }


        private async void sendCardFromAPI(HttpResponseMessage result, WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string cardAsString = await result.Content.ReadAsStringAsync();
            dynamic jsonObj = JsonConvert.DeserializeObject(cardAsString);
            
            Attachment card = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = jsonObj
            };
            
            var response = MessageFactory.Attachment(card);
            await stepContext.Context.SendActivityAsync(response, cancellationToken);
        }

        private async Task<string[]> getLatestObservationValue(string type)
        {
            string apiRequest = "search/observation/" + id;
            HttpResponseMessage result = await client.GetAsync(apiRequest);
            string cardAsString = await result.Content.ReadAsStringAsync();
            JObject jsonObj = (JObject) JsonConvert.DeserializeObject(cardAsString);

            JToken dates = jsonObj.Last;
            JArray observations;

            while (dates.Previous != null)
            {
                observations = (JArray) dates.First.SelectToken("items");
                for (int i = 0; i < observations.Count; i++)
                {
                    string currentType = (string) observations[i].First;
                    if (currentType.ToLower().Contains(type.ToLower()))
                    {
                        JProperty propertyToExtractName = (JProperty) dates;
                        return new string[]{ propertyToExtractName.Name, i.ToString() };
                    }
                }
                dates = dates.Previous;
            }
            //Do once more because after the loop breaks the first date hasn't been tested
            observations = (JArray) dates.First.SelectToken("items");
            for (int i = 0; i < observations.Count; i++)
            {
                string currentType = (string) observations[i].First;
                if (currentType.ToLower().Contains(type.ToLower()))
                {
                    return new string[]{ dates.Type.ToString(), i.ToString() };
                }
            }

            return null;
        }
    }
}