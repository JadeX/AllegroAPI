namespace JadeX.AllegroAPI;

using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

public interface IAuthenticationApi
{
    [Post("/auth/oauth/device")]
    Task<DeviceCodeResponse> GetDeviceCode(
        [Authorize("Basic")] string token,
        [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data
    );

    [Post("/auth/oauth/token")]
    Task<AccessTokensResponse> GetAccessTokens(
        [Authorize("Basic")] string token,
        [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data
    );

    [Post("/auth/oauth/token")]
    Task<AccessTokensResponse> RefreshAccessTokens(
        [Authorize("Basic")] string token,
        [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, object> data
    );
}
