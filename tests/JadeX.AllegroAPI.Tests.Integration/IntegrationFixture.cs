namespace JadeX.AllegroAPI.Tests.Integration.Authentication;

using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class IntegrationFixture
{
    private static UserSecretStore secretStore = new();
    private static string? secretPath;

    public static AllegroRestClient AllegroRestClient { get; private set; } = default!;
    public static IConfigurationRoot Configuration { get; private set; } = default!;

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext _)
    {
        Configuration = new ConfigurationBuilder().AddUserSecrets<DeviceFlow>().AddEnvironmentVariables().Build();
        AllegroRestClient = new AllegroRestClient(x =>
        {
            x.Environment = AllegroEnvironment.Sandbox;
            x.Authentication.Flow = AuthenticationFlow.Device;
            x.Authentication.ClientId = Configuration["CLIENT_ID"];
            x.Authentication.ClientSecret = Configuration["CLIENT_SECRET"];

            x.Authentication.OnDeviceCodeChanged += OnDeviceCodeChanged;
            x.Authentication.OnAccessTokenChanged += OnAccessTokenChanged;
            x.Authentication.OnRefreshTokenChanged += OnRefreshTokenChanged;
        });

        var secretsId = (Assembly.GetExecutingAssembly().GetCustomAttribute<UserSecretsIdAttribute>()?.UserSecretsId) ?? throw new InvalidOperationException("User secret ID is missing, use \"dotnet user-secrets init\"");

        secretPath = PathHelper.GetSecretsPathFromSecretsId(secretsId);

        if (!File.Exists(secretPath))
        {
            new DirectoryInfo(secretPath).Parent?.Create();
            await File.WriteAllTextAsync(secretPath, JsonSerializer.Serialize(value: secretStore)).ConfigureAwait(false);
        }
        else
        {
            var secretsJson = await File.ReadAllTextAsync(secretPath).ConfigureAwait(false);
            secretStore = JsonSerializer.Deserialize<UserSecretStore>(secretsJson)!;

            AllegroRestClient.Options.Authentication.DeviceCode = secretStore?.DeviceCode;
            AllegroRestClient.Options.Authentication.AccessToken = secretStore?.AccessToken;
            AllegroRestClient.Options.Authentication.RefreshToken = secretStore?.RefreshToken;
        }
    }

    public static async void OnDeviceCodeChanged(object? _, string? deviceCode)
    {
        if (secretPath is null || deviceCode == secretStore.DeviceCode)
        {
            return;
        }

        secretStore!.DeviceCode = deviceCode!;
        await File.WriteAllTextAsync(secretPath, JsonSerializer.Serialize(secretStore)).ConfigureAwait(false);
    }

    public static async void OnAccessTokenChanged(object? _, string? accessToken)
    {
        if (secretPath is null || accessToken == secretStore.AccessToken)
        {
            return;
        }

        secretStore.AccessToken = accessToken;
        await File.WriteAllTextAsync(secretPath, JsonSerializer.Serialize(secretStore)).ConfigureAwait(false);
    }

    public static async void OnRefreshTokenChanged(object? _, string? refreshToken)
    {
        if (secretPath is null || refreshToken == secretStore.RefreshToken)
        {
            return;
        }

        secretStore.RefreshToken = refreshToken;
        await File.WriteAllTextAsync(secretPath, JsonSerializer.Serialize(secretStore)).ConfigureAwait(false);
    }

    private sealed record UserSecretStore
    {
        public string? DeviceCode { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
