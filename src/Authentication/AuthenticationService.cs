namespace JadeX.AllegroAPI;

using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

/// <inheritdoc />
public class AuthenticationService(IAllegroAPI authenticationApi, IOptions<AllegroRestClientOptions> options)
    : IAuthenticationService, IDisposable
{
    private readonly IAuthenticationApi authenticationApi = authenticationApi;
    private readonly AllegroRestClientOptions options = options!.Value;
    private readonly SemaphoreSlim semaphore = new(1);
    private DateTime? tokenLastRefreshed;

    /// <inheritdoc />
    public async Task<AccessTokensResponse> GetAccessTokens(string? deviceCode = null, string? clientId = null, string? clientSecret = null)
    {
        deviceCode ??= this.options.Authentication.DeviceCode;
        clientId ??= this.options.Authentication.ClientId;
        clientSecret ??= this.options.Authentication.ClientSecret;

        var result = await this.authenticationApi.GetAccessTokens(
            ToBase64String(clientId!, clientSecret!),
            new()
            {
                { "grant_type", "urn:ietf:params:oauth:grant-type:device_code" },
                { "device_code", deviceCode! }
            }
        );

        this.options.Authentication.AccessToken = result.AccessToken;
        this.options.Authentication.RefreshToken = result.RefreshToken;

        return result;
    }

    /// <inheritdoc />
    public async Task<DeviceCodeResponse> GetDeviceCode(string? clientId = null, string? clientSecret = null)
    {
        clientId ??= this.options.Authentication.ClientId ?? throw new InvalidDataException($"{nameof(clientId)} must be provided either through settings or arguments");
        clientSecret ??= this.options.Authentication.ClientSecret ?? throw new InvalidDataException($"{nameof(clientSecret)} must be provided either through settings or arguments");

        if (clientId.Length != this.options.Allegro.ClientIdLength)
        {
            throw new InvalidDataException($"Invalid ClientID supplied: \"{clientId}\"");
        }
        if (clientSecret.Length != this.options.Allegro.ClientSecretLength)
        {
            throw new InvalidDataException($"Invalid ClientSecret supplied: \"{clientSecret}\"");
        }

        var result = await this.authenticationApi.GetDeviceCode(
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}")),
            new() { { "client_id", clientId } }
        );

        this.options.Authentication.DeviceCode = result.DeviceCode;
        this.options.Authentication.AccessToken = null;
        this.options.Authentication.RefreshToken = null;

        return result;
    }

    /// <inheritdoc />
    public async Task<AccessTokensResponse> RefreshTokens(string? refreshToken = null, string? clientId = null, string? clientSecret = null)
    {
        await this.semaphore.WaitAsync();

        try
        {
            if (this.tokenLastRefreshed != null && DateTime.UtcNow - this.tokenLastRefreshed < TimeSpan.FromSeconds(10))
            {
                Debug.WriteLine("Token already refreshed");
                return new AccessTokensResponse();
            }
            clientId ??= this.options.Authentication.ClientId;
            clientSecret ??= this.options.Authentication.ClientSecret;
            refreshToken ??= this.options.Authentication.RefreshToken;

            Debug.WriteLine("Access tokens refreshed!");
            var result = await this.authenticationApi.RefreshAccessTokens(
                ToBase64String(clientId!, clientSecret!),
                new()
                {
                {"grant_type", "refresh_token"},
                {"refresh_token", refreshToken!}
                }
            );

            this.options.Authentication.AccessToken = result.AccessToken;
            this.options.Authentication.RefreshToken = result.RefreshToken;
            this.tokenLastRefreshed = DateTime.UtcNow;
            return result;
        }
        finally
        {
            _ = this.semaphore.Release();
        }
    }

    private static string ToBase64String(string clientId, string clientSecret) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

    public void Dispose()
    {
        this.semaphore.Dispose();
        GC.SuppressFinalize(this);
    }
}
