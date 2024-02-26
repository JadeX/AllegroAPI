namespace JadeX.AllegroAPI;
using System.Threading.Tasks;

public interface IAuthenticationService
{
    Task<AccessTokensResponse> GetAccessTokens(string? deviceCode = null, string? clientId = null, string? clientSecret = null);
    Task<DeviceCodeResponse> GetDeviceCode(string? clientId = null, string? clientSecret = null);
    Task<AccessTokensResponse> RefreshTokens(string? refreshToken = null, string? clientId = null, string? clientSecret = null);
}
