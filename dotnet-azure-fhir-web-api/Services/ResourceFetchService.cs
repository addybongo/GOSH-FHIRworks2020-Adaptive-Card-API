using HDR_UK_Web_Application.IServices;
using HDR_UK_Web_Application.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HDR_UK_Web_Application.Services
{
    public class ResourceFetchService : IResourceFetchService
    {
        private readonly IProtectedWebApiCallerService _caller;
        private readonly ILoggerManager _logger;
        private readonly AuthenticationConfig config = AuthenticationConfig.ReadFromJsonFile("appsettings.json");

        public ResourceFetchService(IProtectedWebApiCallerService caller, ILoggerManager logger)
        {
            _caller = caller;
            _logger = logger;
        }

        public async Task<JObject> GetSinglePage(string requestOptions)
        {
            try
            {
                _logger.LogInfo("Class: ResourceFetchService, Method: GetSinglePage");
                return await _caller.ProtectedWebApiCaller($"{config.BaseAddress}{requestOptions}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Class: ResourceFetchService, Method: GetSinglePage, Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<List<JObject>> GetAllPages(string requestOptions)
        {
            List<JObject> list = new List<JObject>();

            try
            {
                _logger.LogInfo("Class: ResourceFetchService, Method: GetAllPages");
                var json = await _caller.ProtectedWebApiCaller($"{config.BaseAddress}{requestOptions}");
                return await RetrieveAllPages(json, list);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Class: ResourceFetchService, Method: GetAllPages, Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<List<JObject>> GetPages(string requestOptions, int pages)
        {
            List<JObject> list = new List<JObject>();

            try
            {
                _logger.LogInfo("Class: ResourceFetchService, Method: GetPages");
                var json = await _caller.ProtectedWebApiCaller($"{config.BaseAddress}{requestOptions}");
                return await RetrievePages(json, list, pages);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Class: ResourceFetchService, Method: GetPages, Exception: {ex.Message}");
                return null;
            }
        }



        private async Task<List<JObject>> RetrieveAllPages(JObject json, List<JObject> list)
        {
            if(json == null)
            {
                return null;
            }


            string relation = (string)json["link"][0]["relation"];
            list.Add(json);

            if (relation.Equals("next"))
            {
                string url = (string)json["link"][0]["url"];
                var response = await _caller.ProtectedWebApiCaller(url);
                return await RetrieveAllPages(response, list);
            }
            else
            {
                return list;
            }
        }

        private async Task<List<JObject>> RetrievePages(JObject json, List<JObject> list, int pages)
        {
            if(json == null)
            {
                return null;
            }


            string relation = (string)json["link"][0]["relation"];
            list.Add(json);

            while (list.Count < pages)
            {
                if (relation.Equals("next"))
                {
                    string url = (string)json["link"][0]["url"];
                    var response = await _caller.ProtectedWebApiCaller(url);
                    return await RetrievePages(response, list, pages);
                }
                else
                {
                    return list;
                }
            }
            return list;
        }
    }
}



