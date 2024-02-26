namespace JadeX.AllegroAPI.Tests.Integration.OrderEvents;

using System.Threading.Tasks;
using JadeX.AllegroAPI.Tests.Integration.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

[TestClass]
public class OrderEvents
{
    [TestMethod]
    public async Task GetOrderEvents()
    {
        var result = await IntegrationFixture.AllegroRestClient.API.GetOrderEventsUsingGET(limit: 1);
        result?.Events.Count.ShouldBe(1);
    }
}
