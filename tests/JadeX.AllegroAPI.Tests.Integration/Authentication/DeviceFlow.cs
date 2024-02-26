namespace JadeX.AllegroAPI.Tests.Integration.Authentication;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

[TestClass]
public class DeviceFlow
{
    [ClassInitialize]
    public static void ClassInitialize(TestContext _) =>
        IntegrationFixture.AllegroRestClient.Options.Authentication.Flow = AuthenticationFlow.Device;

    [TestMethod]
    public async Task GetDeviceCode()
    {
        var result = await IntegrationFixture.AllegroRestClient.AuthenticationService.GetDeviceCode();
        result.IsSuccess.ShouldBeTrue();
        result.DeviceCode?.Length.ShouldBe(IntegrationFixture.AllegroRestClient.Options.Allegro.DeviceCodeLength);
    }

    [TestMethod]
    public async Task GetAccessTokens()
    {
        var result = await IntegrationFixture.AllegroRestClient.AuthenticationService.GetAccessTokens();
        result.IsSuccess.ShouldBeTrue();
        result.AccessToken?.Length.ShouldBe(IntegrationFixture.AllegroRestClient.Options.Allegro.AccessTokenLength);
        result.RefreshToken?.Length.ShouldBe(IntegrationFixture.AllegroRestClient.Options.Allegro.RefreshTokenLength);
    }

    [TestMethod]
    public async Task RefreshAccessTokens()
    {
        var result = await IntegrationFixture.AllegroRestClient.AuthenticationService.RefreshTokens();
        result.IsSuccess.ShouldBeTrue();
        result.AccessToken?.Length.ShouldBe(IntegrationFixture.AllegroRestClient.Options.Allegro.AccessTokenLength);
        result.RefreshToken?.Length.ShouldBe(IntegrationFixture.AllegroRestClient.Options.Allegro.RefreshTokenLength);
    }
}
