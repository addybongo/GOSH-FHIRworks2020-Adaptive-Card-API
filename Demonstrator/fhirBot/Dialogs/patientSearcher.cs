using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Newtonsoft.Json.Linq;

namespace fhirBot.Dialogs
{
    public class patientSearcher : ComponentDialog
    {
        private static HttpClient client;
        private HttpClientHandler clientHandler;
        private string firstname;
        private string surname;
        private string dob;

        private string patientId; 
        
        public patientSearcher()
        {
            //Workaround SSL certificate issue- removes certificate validation
            clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            
            client = new HttpClient(clientHandler);
            client.BaseAddress = new Uri("https://localhost:5001/api/");
            client.DefaultRequestHeaders.Accept.Clear();

            var waterfallSteps = new WaterfallStep[]
            {
                firstnameAsync,
                surnameAsync,
                dobAsync,
                resultAsync,
                confirmIdentityAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new DateTimePrompt(nameof(DateTimePrompt), DateTimePromptValidator));
            AddDialog(new Patient());


            InitialDialogId = nameof(WaterfallDialog);
        }
        
        private async Task<DialogTurnResult> firstnameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string welcomeMessage = stepContext.Options == null ? "Hi there! Please can I have your first name?" : 
                stepContext.Options.ToString();
            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text(welcomeMessage) };
            return await stepContext.PromptAsync(nameof(TextPrompt),promptOptions, cancellationToken);
        }
        
        private async Task<DialogTurnResult> surnameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            firstname = (string) stepContext.Result;
            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Now can I please have your surname?") };
            return await stepContext.PromptAsync(nameof(TextPrompt),promptOptions, cancellationToken);
        }
        
        private async Task<DialogTurnResult> dobAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            surname = (string) stepContext.Result;
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("What is your date of birth? Please include the date, month and year."),
                RetryPrompt = MessageFactory.Text("I'm sorry, please can you include the day, month and year.")
            };
            return await stepContext.PromptAsync(nameof(DateTimePrompt), promptOptions, cancellationToken);
        }
        
        private async Task<DialogTurnResult> resultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string timexFormattedDob = ((List<DateTimeResolution>)stepContext.Result)[0].Timex;
            dob = timexFormattedDob.Split("-")[0] + timexFormattedDob.Split("-")[1] + timexFormattedDob.Split("-")[2];

            stepContext.Context.SendActivityAsync(
                "I am now searching for your medical records. Please bare with me, this may take a moment...");
            
            string apiRequest = "card/patient/search?firstname=" + firstname + "&surname=" + surname + "&dob=" + dob;
            HttpResponseMessage result = await client.GetAsync(apiRequest, cancellationToken);
            string cardAsString = await result.Content.ReadAsStringAsync();
            JArray jsonObj = (JArray) JsonConvert.DeserializeObject(cardAsString);

            if ((string) jsonObj[0]["resourceType"] == "OperationOutcome")
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, 
                    "I'm sorry, I couldn't find a patient with those details. Let's start again. What is your first name?", cancellationToken);
            }

            for (int i = 0; i < jsonObj.Count; i++)
            {
                Attachment card = new Attachment()
                {
                    ContentType = "application/vnd.microsoft.card.adaptive",
                    Content = jsonObj[i]
                };

                var response = MessageFactory.Attachment(card);
                await stepContext.Context.SendActivityAsync(response, cancellationToken);
            }

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please select a card from the results above or type in your unique patient ID number."),
                RetryPrompt = MessageFactory.Text("I'm sorry, please use a valid ID number or select a patient card from above.")
            };
            
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> confirmIdentityAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(Patient), new String[] { stepContext.Result.ToString() }, cancellationToken);
        }

        
        //This method was taken from the CoreBot template by Microsoft
        private static Task<bool> DateTimePromptValidator(PromptValidatorContext<IList<DateTimeResolution>> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                // This value will be a TIMEX. And we are only interested in a Date so grab the first result and drop the Time part.
                // TIMEX is a format that represents DateTime expressions that include some ambiguity. e.g. missing a Year.
                var timex = promptContext.Recognized.Value[0].Timex.Split('T')[0];

                //Checks whether the data has a day month and year
                var isDefinite = new TimexProperty(timex).Types.Contains(Constants.TimexTypes.Definite);

                return Task.FromResult(isDefinite);
            }

            return Task.FromResult(false);
        }


        private async Task<DialogTurnResult> patientCard(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string apiRequest = "Card/patients/" + stepContext.Result;
            HttpResponseMessage result = await client.GetAsync(apiRequest, cancellationToken);

            if (!result.IsSuccessStatusCode)
            {
                await stepContext.Context.SendActivityAsync("I'm sorry, I couldn't find a patient with that ID");
                return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
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
            return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
        }
    }
}