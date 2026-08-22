using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SurveyApp.Web.Controllers;

public class HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration) : Controller
{
    public IActionResult Index()
    {
        ViewBag.ForgotPasswordUrl = $"{configuration["Identity:Authority"]}/Account/ForgotPassword";
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");

        var client = httpClientFactory.CreateClient("SurveyAppApi");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.GetAsync("/me");
        ViewBag.MeResponse = response.IsSuccessStatusCode
            ? JsonSerializer.Serialize(await response.Content.ReadFromJsonAsync<JsonElement>(), new JsonSerializerOptions { WriteIndented = true })
            : $"GET /me failed: {(int)response.StatusCode} {response.ReasonPhrase}";

        return View();
    }
}
