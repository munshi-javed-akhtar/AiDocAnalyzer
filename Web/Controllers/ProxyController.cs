using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/proxy")]
public class ProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProxyController> _logger;

    public ProxyController(IHttpClientFactory httpClientFactory, ILogger<ProxyController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");
        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "file", file.FileName);

        var response = await client.PostAsync("/documents/upload", content);
        var body = await response.Content.ReadAsStringAsync();

        return StatusCode((int)response.StatusCode, body);
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] object payload)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");
        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/documents/search", content);
        var body = await response.Content.ReadAsStringAsync();
        return StatusCode((int)response.StatusCode, body);
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] object payload)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");
        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/documents/ask", content);
        var body = await response.Content.ReadAsStringAsync();
        return StatusCode((int)response.StatusCode, body);
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("/health/vector");
            return StatusCode((int)response.StatusCode);
        }
        catch
        {
            return StatusCode(503);
        }
    }
}
