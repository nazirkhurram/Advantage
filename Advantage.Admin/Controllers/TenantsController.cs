using Advantage.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace Advantage.Admin.Controllers;

public class TenantsController(IHttpClientFactory httpClientFactory) : Controller
{
    private HttpClient TenancyClient => httpClientFactory.CreateClient("TenancyApi");

    public async Task<IActionResult> Index(string? product)
    {
        var query = string.IsNullOrWhiteSpace(product) ? "" : $"?product={Uri.EscapeDataString(product)}";
        var tenants = await TenancyClient.GetFromJsonAsync<List<TenantDto>>($"/tenants{query}") ?? [];

        ViewBag.SelectedProduct = product;
        ViewBag.Products = tenants.Select(t => t.Product).Distinct().OrderBy(p => p).ToList();

        return View(tenants.OrderBy(t => t.Product).ThenBy(t => t.Name).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> Suspend(Guid id)
    {
        await TenancyClient.PostAsync($"/tenants/{id}/suspend", null);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Reactivate(Guid id)
    {
        await TenancyClient.PostAsync($"/tenants/{id}/reactivate", null);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        await TenancyClient.DeleteAsync($"/tenants/{id}?confirm=true");
        return RedirectToAction(nameof(Index));
    }
}
