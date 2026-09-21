using CustomerCampaign.Portal.Models;
using CustomerCampaign.Portal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerCampaign.Portal.Controllers;

[Authorize]
public class HomeController(CampaignApiClient api, ILogger<HomeController> logger) : Controller
{
    [HttpGet]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var model = new DashboardViewModel();
        try
        {
            model.Daily = await api.GetMyRewardsAsync(ct);
        }
        catch (ApiException ex)
        {
            model.ErrorMessage = ex.Message;
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> Register(int customerExternalId, string? note, CancellationToken ct)
    {
        try
        {
            var reward = await api.CreateRewardAsync(customerExternalId, note, ct);
            TempData["Success"] = $"Customer {reward.CustomerName} (ID {reward.CustomerExternalId}) was registered.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        try
        {
            await api.CancelRewardAsync(id, ct);
            TempData["Success"] = "Reward was cancelled.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> Lookup(int id, CancellationToken ct)
    {
        try
        {
            var customer = await api.LookupCustomerAsync(id, ct);
            return Json(new { found = true, name = customer.FullName, city = customer.City, age = customer.Age });
        }
        catch (ApiException ex)
        {
            return Json(new { found = false, message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Results(CancellationToken ct)
    {
        try
        {
            return View(await api.GetCurrentResultsAsync(ct));
        }
        catch (ApiException ex)
        {
            logger.LogWarning("Could not load results: {Message}", ex.Message);
            TempData["Error"] = ex.Message;
            return View(null as CampaignResultsDto);
        }
    }

    [AllowAnonymous]
    public IActionResult Error() => View();
}