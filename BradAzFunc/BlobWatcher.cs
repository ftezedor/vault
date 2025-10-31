using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


public class BlobWatcher
{
    private readonly ILogger<BlobWatcher> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _downstreamUrl;

    public BlobWatcher(ILogger<BlobWatcher> logger, IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _downstreamUrl = config["DOWNSTREAM_FUNCTION_URL"]!;
    }

    public record PipelineRequest(string Container, string Name, long Size);

    [Function("BlobWatcher")]
    public async Task Run(
        [BlobTrigger("samples-workitems/{name}", Connection = "AzureWebJobsStorage")]
        Stream blob, string name)
    {
        var payload = new PipelineRequest("samples-workitems", name, blob.Length);

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Calling downstream at {Url}", _downstreamUrl);
        var client = _httpClientFactory.CreateClient();
        var resp = await client.PostAsync(_downstreamUrl, content);

        var body = await resp.Content.ReadAsStringAsync();
        _logger.LogInformation("Downstream responded {Status} with body: {Body}", resp.StatusCode, body);

        //resp.EnsureSuccessStatusCode();        
    }
}
