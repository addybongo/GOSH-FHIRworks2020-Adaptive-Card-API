using HDR_UK_Web_Application.IServices;
using HDR_UK_Web_Application.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;

namespace HDR_UK_Web_Application.Services
{
    public class AccessTokenService : IAccessTokenService
    {
        private readonly ILoggerManager _logger;
        private readonly IMemoryCache _cache;

        public AccessTokenService(ILoggerManager logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        private async Task<AuthenticationResult> CreateAccessToken()
        {
            AuthenticationConfig config = AuthenticationConfig.ReadFromJsonFile("appsettings.json");

            IConfidentialClientApplication app;

            app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                .WithClientSecret(config.ClientSecret)
                .WithAuthority(new Uri(config.Authority))
                .Build();

            string[] scopes = new string[] { config.Scope };

            AuthenticationResult result = null;
            try
            {
                _logger.LogInfo("Class: AccessTokenService, Method: CreateAccessToken, Info: Acquire Token For Client.");
                result = await app.AcquireTokenForClient(scopes)
                            .ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                _logger.LogError($"Class: AccessTokenService, Method: CreateAccessToken, {Environment.NewLine} Exception: {ex}, {Environment.NewLine} Message: {ex.Message}, {Environment.NewLine} StackTrace: {ex.StackTrace}");
                return result;

            }
            catch (MsalServiceException ex)
            {
                _logger.LogError($"Class: AccessTokenService, Method: CreateAccessToken, {Environment.NewLine} Exception: {ex}, {Environment.NewLine} Message: {ex.Message}, {Environment.NewLine} StackTrace: {ex.StackTrace}");
                return result;
            }

            return result;
        }

        public async Task<AuthenticationResult> GetAuthenticationResult()
        {
            AuthenticationResult result = null;

            try
            {
                _logger.LogInfo("Class: AccessTokenService, Method: GetAuthenticationResult, Info: GetOrCreateAsync AuthenticationResult.");
                result = await _cache.GetOrCreateAsync("AuthenticationResult", cacheEntry =>
                {
                    cacheEntry.SetAbsoluteExpiration(TimeSpan.FromMinutes(59));
                    return CreateAccessToken();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Class: AccessTokenService, Method: GetAuthenticationResult, {Environment.NewLine} Exception: {ex}, {Environment.NewLine} Message: {ex.Message}, {Environment.NewLine} StackTrace: {ex.StackTrace}");
                return result;
            }

            return result;
        }
    }
}
