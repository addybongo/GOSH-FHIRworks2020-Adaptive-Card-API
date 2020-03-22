using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace HDR_UK_Web_Application.IServices
{
    public interface IAccessTokenService
    {
        Task<AuthenticationResult> GetAuthenticationResult();
    }
}
