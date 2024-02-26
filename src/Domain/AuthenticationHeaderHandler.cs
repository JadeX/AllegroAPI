namespace JadeX.AllegroAPI.Domain;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

internal sealed class AuthenticationHeaderHandler(IMediator mediator, IOptions<AllegroRestClientOptions> options) : DelegatingHandler
{
    private readonly AsyncRetryPolicy<HttpResponseMessage> retryPolicy =
        Policy
            .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.Unauthorized)
            .RetryAsync(1, async (_, _) => _ = await mediator.Send(new API.Authentication.RefreshTokens()));

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        await this.retryPolicy.ExecuteAsync(async () =>
        {
            var opts = options.Value;

            if ((request.Headers?.Authorization?.Scheme) == "Basic")
            {
                if (request.RequestUri?.Host == opts.EnvironmentApiUrl)
                {
                    request.RequestUri = new Uri($"https://{opts.EnvironmentAuthUrl}{request.RequestUri.PathAndQuery}");
                }
            }
            else
            {
                var accessToken = opts.Authentication.AccessToken;

                if (accessToken is not null)
                {
                    request.Headers!.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
            }

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            return response;
        });
}
