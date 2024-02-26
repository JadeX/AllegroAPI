namespace JadeX.AllegroAPI.API.Authentication;

using System.Threading;
using System.Threading.Tasks;
using MediatR;

public record RefreshTokens : IRequest<AccessTokensResponse> { }

internal sealed class RefreshTokensHandler(IAuthenticationService authenticationService)
    : IRequestHandler<RefreshTokens, AccessTokensResponse>
{
    public async Task<AccessTokensResponse> Handle(RefreshTokens request, CancellationToken cancellationToken) => await authenticationService.RefreshTokens();
}
