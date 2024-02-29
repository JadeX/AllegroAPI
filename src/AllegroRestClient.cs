namespace JadeX.AllegroAPI;

using Microsoft.Extensions.DependencyInjection;
using System;
using Refit;
using JadeX.AllegroAPI.Domain;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

public class AllegroRestClient
{
    public IAuthenticationService AuthenticationService { get; private set; }
    public IAllegroAPI API { get; private set; }
    public AllegroRestClientOptions Options { get; private set; }

    public AllegroRestClient(Action<AllegroRestClientOptions> clientOptions)
    {
        var options = new AllegroRestClientOptions();
        clientOptions(options);

        var services = new ServiceCollection();

        _ = services
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(AllegroRestClient).Assembly))
            .AddScoped(typeof(AuthenticationHeaderHandler))
            .AddScoped(typeof(IAuthenticationService), typeof(AuthenticationService))
#if DEBUG
            .AddScoped(typeof(DebugHandler))
#endif
            .AddOptions<AllegroRestClientOptions>().Configure(clientOptions);

        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        };

        jsonSerializerOptions.Converters.Add(new JsonStringEnumMemberConverter());
        jsonSerializerOptions.Converters.Add(new DateTimeOffsetNullHandlingConverter());

        var refit = services.AddRefitClient<IAllegroAPI>(new RefitSettings()
        {
            ContentSerializer = new SystemTextJsonContentSerializer(jsonSerializerOptions),
        })
        .ConfigureHttpClient(c =>
        {
            c.BaseAddress = new Uri($"https://{options.EnvironmentApiUrl}");
            c.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(options.Allegro.Language));
            if (options.Timeout != default)
            {
                c.Timeout = options.Timeout;
            }
        })
        .AddHttpMessageHandler<AuthenticationHeaderHandler>();

#if DEBUG
        refit.AddHttpMessageHandler<DebugHandler>();
#endif

        var provider = services.BuildServiceProvider();
        this.Options = provider.GetRequiredService<IOptions<AllegroRestClientOptions>>().Value;
        this.AuthenticationService = provider.GetRequiredService<IAuthenticationService>();
        this.API = provider.GetRequiredService<IAllegroAPI>();
    }
}
