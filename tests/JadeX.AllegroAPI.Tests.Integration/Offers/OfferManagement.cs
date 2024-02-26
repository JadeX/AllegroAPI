namespace JadeX.AllegroAPI.Tests.Integration.OfferManagement;

using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using System;
using System.Globalization;
using JadeX.AllegroAPI.Tests.Integration.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class OfferManagement
{
    [TestMethod]
    public async Task PatchOfferPriceAndStock()
    {
        var offer = await IntegrationFixture.AllegroRestClient.API.SearchOffersUsingGET(limit: 1);
        offer?.Count?.ShouldBe(1);

        var newProductOffer = new SaleProductOfferPatchRequestV1
        {
            SellingMode = new SellingMode()
            {
                Price = new PriceModificationFixedPriceHolder()
                {
                    Amount = Random.Shared.Next(5000).ToString(CultureInfo.InvariantCulture) + ".00"
                }
            },
            Stock = new SaleProductOffersRequestStock()
            {
                Available = Random.Shared.Next(500),
                Unit = SaleProductOffersRequestStockUnit.UNIT
            }
        };

        var result = await IntegrationFixture.AllegroRestClient.API.EditProductOffers(offer?.Offers?.FirstOrDefault()?.Id!, newProductOffer);
        result?.SellingMode?.Price?.Amount.ShouldBe(newProductOffer.SellingMode.Price.Amount);
        result?.Stock?.Available.ShouldBe(newProductOffer.Stock.Available);
    }

    [TestMethod]
    public async Task EndListing()
    {
        var offers = await IntegrationFixture.AllegroRestClient.API.SearchOffersUsingGET(publication_status: [Anonymous.ACTIVE]);
        if (offers.Count == 0)
        {
            Assert.Inconclusive("No active offers found.");
        }
        var offerId = offers.Offers?.FirstOrDefault()?.Id;
        var publicationChangeCommandDto = new PublicationChangeCommandDto()
        {
            Publication = new Publication_modification()
            {
                Action = Publication_modificationAction.END
            },
            OfferCriteria = [new OfferCriterium() { Offers = [new OfferId() { Id = offerId }], Type = OfferCriteriumType.CONTAINS_OFFERS }]
        };

        var result = await IntegrationFixture.AllegroRestClient.API.ChangePublicationStatusUsingPUT(Guid.NewGuid().ToString(), publicationChangeCommandDto);
    }

    [TestMethod]
    public async Task RenewListing()
    {
        var offers = await IntegrationFixture.AllegroRestClient.API.SearchOffersUsingGET(publication_status: [Anonymous.ENDED]);
        if (offers.Count == 0)
        {
            Assert.Inconclusive("No ended offers found.");
        }
        var offerId = offers.Offers?.FirstOrDefault()?.Id;
        var publicationChangeCommandDto = new PublicationChangeCommandDto()
        {
            Publication = new Publication_modification()
            {
                Action = Publication_modificationAction.ACTIVATE
            },
            OfferCriteria = [new OfferCriterium() { Offers = [new OfferId() { Id = offerId }], Type = OfferCriteriumType.CONTAINS_OFFERS }]
        };

        var result = await IntegrationFixture.AllegroRestClient.API.ChangePublicationStatusUsingPUT(Guid.NewGuid().ToString(), publicationChangeCommandDto);
    }
}
