using CustomerCampaign.Application.Common.Exceptions;
using CustomerCampaign.Application.Common.Interfaces;
using CustomerCampaign.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerCampaign.Api.Controllers;

[ApiController]
[Route("api/v1/customers")]
[Authorize(Policy = "CrmLookup")]
public class CustomersController(ICustomerDirectory customerDirectory) : ControllerBase
{
    [HttpGet("{externalId:int}")]
    public async Task<ActionResult<CustomerInfo>> Get(int externalId, CancellationToken ct)
    {
        var customer = await customerDirectory.FindByIdAsync(externalId, ct);
        return customer is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, title: "Customer not found",
                      detail: $"Customer {externalId} does not exist in CRM.")
            : Ok(customer);
    }
}