using Basket.API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Basket.API.Services;
using System;
using System.Threading.Tasks;
using eShopOnContainers.Services.IntegrationEvents.Events;
using MassTransit;

namespace Basket.API.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize]
    public class BasketController : Controller
    {
        private readonly IBasketRepository _repository;
        private readonly IIdentityService _identitySvc;
        private readonly IPublishEndpoint _endpoint;

        public BasketController(IBasketRepository repository, 
            IIdentityService identityService, IPublishEndpoint endpoint)
        {
            _repository = repository;
            _identitySvc = identityService;
            _endpoint = endpoint;
        }
        // GET /id
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var basket = await _repository.GetBasketAsync(id);

            return Ok(basket);
        }

        // POST /value
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CustomerBasket value)
        {
            var basket = await _repository.UpdateBasketAsync(value);

            return Ok(basket);
        }

        [Route("checkout")]
        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody]BasketCheckout basketCheckout, [FromHeader(Name = "x-requestid")] string requestId)
        {
            var userId = _identitySvc.GetUserIdentity();
            basketCheckout.RequestId = (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty) ?
                guid : basketCheckout.RequestId;

            var basket = await _repository.GetBasketAsync(userId);

            // Once basket is checkout, sends an integration event to
            // ordering.api to convert basket to order and proceeds with
            // order creation process
            await _endpoint.Publish<UserCheckoutAcceptedIntegrationEvent>(new {
                UserId = userId,
                City = basketCheckout.City,
                Street = basketCheckout.Street,
                State = basketCheckout.State,
                Country = basketCheckout.Country,
                ZipCode = basketCheckout.ZipCode,
                CardNumber = basketCheckout.CardNumber,
                CardHolderName = basketCheckout.CardHolderName,
                CardExpiration = basketCheckout.CardExpiration,
                CardSecurityNumber = basketCheckout.CardSecurityNumber,
                CardTypeId = basketCheckout.CardTypeId,
                Buyer = basketCheckout.Buyer,
                Basket = basket,
                RequestId = requestId
            });

            if (basket == null)
            {
                return BadRequest();
            }

            return Accepted();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            _repository.DeleteBasketAsync(id);
        }

    }
}
